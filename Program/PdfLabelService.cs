using PDFiumSharp;
using PDFiumSharp.Enums;
using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace DHLabel
{

/// <summary>
/// Rendert, beschneidet und skaliert ein PDF-Versandetikett
/// für die Druckausgabe oder Vorschau.
/// </summary>
public class PdfLabelService
{
    // ------------------------------------------------------------------
    // Konfiguration: alle Maßangaben in Millimetern
    // ------------------------------------------------------------------
    private static class CropSettings
    {
        // Ausschnitt aus der Original-PDF-Seite
        public const double CropXStartMm     =   4.0;
        public const double CropYStartMm     =  23.0;
        public const double CropXEndMm       = 208.0;
        public const double CropYEndMm       = 125.0;

        // Vertikale Bereiche, die nach der 90°-Drehung entfernt werden
        public const double CutAStartMm      =  55.0;
        public const double CutAEndMm        =  60.0;
        public const double CutBStartMm      =  90.0;
        public const double CutBEndMm        = 120.0;

        // Zielhöhen je nach Papierformat
        public const double TargetHeightMm   = 148.0;   // A6 / Einzelblatt
        public const double EndlessHeightMm  = 158.0;   // Endlosetiketten
    }

    // ------------------------------------------------------------------
    // Öffentliche API
    // ------------------------------------------------------------------

    /// <summary>
    /// Rendert Seite 0 des angegebenen PDFs und liefert eine zugeschnittene,
    /// gedrehte und skalierte Bitmap zurück.
    /// Der Aufrufer ist für das Dispose der zurückgegebenen Bitmap verantwortlich.
    /// </summary>
    /// <param name="pdfPath">Pfad zur PDF-Datei.</param>
    /// <param name="dpi">Ziel-Auflösung in DPI (z. B. 203 oder 300).</param>
    /// <param name="scale">Optionaler Skalierungsfaktor (Standard 1,0).</param>
    public Bitmap RenderPdf(string pdfPath, int dpi, float scale = 1.0f)
    {
        bool endless = DHLabel.Properties.Settings.Default.endless;

        using (var document = new PdfDocument(pdfPath))
        {
            var page = document.Pages[0];

            int fullWidthPx  = MmToPx(page.Size.Width  / 72.0 * 25.4, dpi);
            int fullHeightPx = MmToPx(page.Size.Height / 72.0 * 25.4, dpi);

            using (var pdfBitmap = new PDFiumBitmap(fullWidthPx, fullHeightPx, hasAlpha: true))
            {
                pdfBitmap.FillRectangle(0, 0, fullWidthPx, fullHeightPx, 0xFFFFFFFF);
                page.Render(pdfBitmap, PageOrientations.Normal, RenderingFlags.Annotations);

                using (var stream    = pdfBitmap.AsBmpStream())
                using (var fullImage = new Bitmap(stream))
                {
                    fullImage.SetResolution(dpi, dpi);

                    // Schritt 1 – Ausschnitt aus der PDF-Seite
                    using (var cropped = CropImage(fullImage, dpi))
                    {
                        // Schritt 2 – 90° im Uhrzeigersinn drehen
                        cropped.RotateFlip(RotateFlipType.Rotate90FlipNone);

                        // Schritt 3 – Unerwünschte Bereiche entfernen
                        using (var assembled = RemoveStrips(cropped, dpi))
                        {
                            // Schritt 4 – Proportional auf Zielhöhe skalieren
                            return ScaleToTargetHeight(assembled, dpi, scale, endless);
                        }
                    }
                }
            }
        }
    }

    // ------------------------------------------------------------------
    // Private Hilfsmethoden
    // ------------------------------------------------------------------

    /// <summary>Schneidet den relevanten Bereich aus dem gerenderten Vollbild aus.</summary>
    private static Bitmap CropImage(Bitmap source, int dpi)
    {
        int xStart     = MmToPx(CropSettings.CropXStartMm, dpi);
        int yStart     = MmToPx(CropSettings.CropYStartMm, dpi);
        int cropWidth  = MmToPx(CropSettings.CropXEndMm - CropSettings.CropXStartMm, dpi);
        int cropHeight = MmToPx(CropSettings.CropYEndMm - CropSettings.CropYStartMm, dpi);

        var rect = new Rectangle(xStart, yStart, cropWidth, cropHeight);
        return source.Clone(rect, source.PixelFormat);
    }

    /// <summary>
    /// Entfernt zwei horizontale Streifen aus dem (bereits gedrehten) Bild
    /// und setzt die verbleibenden Blöcke nahtlos zusammen.
    /// </summary>
    private static Bitmap RemoveStrips(Bitmap source, int dpi)
    {
        int cutA_start = MmToPx(CropSettings.CutAStartMm, dpi);
        int cutA_end   = MmToPx(CropSettings.CutAEndMm,   dpi);
        int cutB_start = MmToPx(CropSettings.CutBStartMm, dpi);
        int cutB_end   = MmToPx(CropSettings.CutBEndMm,   dpi);

        int h1 = cutA_start;                    // Block 1: 0 → CutA-Start
        int h2 = cutB_start - cutA_end;         // Block 2: CutA-Ende → CutB-Start
        int h3 = source.Height - cutB_end;      // Block 3: CutB-Ende → Bildende

        int newHeight = h1 + h2 + h3;

        Bitmap result = new Bitmap(source.Width, newHeight);
        result.SetResolution(dpi, dpi);

        using (Graphics g = Graphics.FromImage(result))
        {
            g.Clear(Color.White);
            int destY = 0;

            DrawStrip(g, source, destY, 0,        h1, source.Width); destY += h1;
            DrawStrip(g, source, destY, cutA_end, h2, source.Width); destY += h2;
            DrawStrip(g, source, destY, cutB_end, h3, source.Width);
        }

        return result;
    }

    /// <summary>Kopiert einen horizontalen Streifen von <paramref name="src"/> nach <paramref name="g"/>.</summary>
    private static void DrawStrip(Graphics g, Bitmap src, int destY, int srcY, int height, int width)
    {
        if (height <= 0) return;

        g.DrawImage(
            src,
            new Rectangle(0, destY, width, height),
            new Rectangle(0, srcY,  width, height),
            GraphicsUnit.Pixel);
    }

    /// <summary>
    /// Skaliert die Bitmap proportional auf die konfigurierte Zielhöhe.
    /// Der Aufrufer erhält eine neue Bitmap und ist für deren Dispose verantwortlich.
    /// </summary>
    private static Bitmap ScaleToTargetHeight(Bitmap source, int dpi, float scale, bool endless)
    {
        double targetHeightMm = endless
            ? CropSettings.EndlessHeightMm
            : CropSettings.TargetHeightMm;

        int targetHeightPx  = (int)Math.Round(targetHeightMm / 25.4 * dpi * scale);
        double aspectRatio  = (double)source.Width / source.Height;
        int targetWidthPx   = (int)Math.Round(targetHeightPx * aspectRatio);

        Bitmap final = new Bitmap(targetWidthPx, targetHeightPx);
        try
        {
            final.SetResolution(dpi, dpi);

            using (Graphics g = Graphics.FromImage(final))
            {
                g.Clear(Color.White);
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.DrawImage(source, new Rectangle(0, 0, targetWidthPx, targetHeightPx));
            }

            return final;
        }
        catch
        {
            final.Dispose();
            throw;
        }
    }

    /// <summary>Rechnet Millimeter in Pixel um (gerundet).</summary>
    private static int MmToPx(double mm, int dpi)
        => (int)Math.Round(mm / 25.4 * dpi);
}

} // namespace DHLabel
