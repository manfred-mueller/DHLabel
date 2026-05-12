using Microsoft.Win32;
using PdfSharp.Drawing;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;

namespace DHLabel
{
    public partial class Form1 : Form
    {
        private PdfLabelService _pdfService = new PdfLabelService();
        private Bitmap _currentLabel;
        private string _currentOriginalPdf;

        // Druckversatz-Konstanten (in mm)
        private const float EndlessOffsetXMm = 5.0f;
        private const float EndlessOffsetYMm = -2.0f;
        private const float A6OffsetXMm      = 3.0f;

        protected StatusBar mainStatusBar = new StatusBar();
        protected StatusBarPanel statusPanel = new StatusBarPanel();

        public Form1(string[] args)
        {
            InitializeComponent();

            CreateStatusBar();

            // PrintPage-Handler einmalig im Konstruktor registrieren
            printDocument1.PrintPage += PrintPageHandler;

            checkKey();

            cbOntop.Checked    = Properties.Settings.Default.onTop;
            cbOpenWith.Checked = Properties.Settings.Default.openWith;
            cbEndless.Checked  = Properties.Settings.Default.endless;

            setTitle();
        }

        // ==========================================================
        // DATEI LADEN
        // ==========================================================

        public void LoadFile(string file)
        {
            try
            {
                _currentOriginalPdf = file;

                picboxLabel.Image?.Dispose();
                _currentLabel = _pdfService.RenderPdf(file, 203);
                picboxLabel.Image = _currentLabel;

                statusPanel.Text = file;
                enableControls();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Properties.Resources.ErrorLoadingPDFN + ex.Message);
            }
        }

        private void openPDF_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                LoadFile(openFileDialog1.FileName);
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            LoadFile(files[0]);
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effect = DragDropEffects.Copy;
        }

        // ==========================================================
        // DRUCK
        // ==========================================================

        private void printLabel_Click(object sender, EventArgs e)
        {
            PrintLabel();
        }

        private void PrintLabel()
        {
            if (string.IsNullOrEmpty(_currentOriginalPdf))
            {
                MessageBox.Show(Properties.Resources.NoPrintableLabelFound);
                return;
            }

            try
            {
                string printerName = Properties.Settings.Default.printOn;

                if (string.IsNullOrWhiteSpace(printerName))
                {
                    MessageBox.Show(Properties.Resources.NoPrinterConfigured);
                    return;
                }

                printDocument1.PrinterSettings.PrinterName = printerName;

                if (!printDocument1.PrinterSettings.IsValid)
                {
                    MessageBox.Show(Properties.Resources.PrinterNotAvailable);
                    return;
                }

                if (Properties.Settings.Default.endless)
                {
                    printDocument1.DefaultPageSettings.PaperSize =
                        new PaperSize("Label100x150",
                            (int)(100 / 25.4 * 100),
                            (int)(150 / 25.4 * 100));

                    printDocument1.DefaultPageSettings.Margins =
                        new Margins(0, 0, 0, 0);
                }
                else
                {
                    printDocument1.DefaultPageSettings.PaperSize =
                        new PaperSize("A6", 413, 583);
                }

                printDocument1.DefaultPageSettings.Landscape = false;
                printDocument1.PrintController = new StandardPrintController();

                printDocument1.Print();
            }
            catch (Exception ex)
            {
                MessageBox.Show(Properties.Resources.PrintingErrorN + ex.Message);
            }
        }

        private void PrintPageHandler(object sender, PrintPageEventArgs e)
        {
            float dpi     = e.Graphics.DpiX;
            int targetDpi = (dpi < 250) ? 203 : 300;

            using (var img = _pdfService.RenderPdf(_currentOriginalPdf, targetDpi))
            {
                e.Graphics.TranslateTransform(
                    -e.PageSettings.HardMarginX,
                    -e.PageSettings.HardMarginY);

                float offsetX;
                float offsetY = 0f;

                if (Properties.Settings.Default.endless)
                {
                    offsetX = (EndlessOffsetXMm / 2f) / 25.4f * dpi;
                    offsetY = EndlessOffsetYMm / 25.4f * dpi;
                }
                else
                {
                    offsetX = (A6OffsetXMm / 2f) / 25.4f * dpi;
                }

                e.Graphics.DrawImage(img, offsetX, offsetY);
            }

            e.HasMorePages = false;
        }

        private void setPrinter_Click(object sender, EventArgs e)
        {
            PrintDialog dlg = new PrintDialog();
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.printOn = dlg.PrinterSettings.PrinterName;
                Properties.Settings.Default.Save();
                setTitle();
            }
        }

        private void saveLabel_Click(object sender, EventArgs e)
        {
            if (_currentLabel == null)
            {
                MessageBox.Show(Properties.Resources.NoLabelLoaded);
                return;
            }

            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
                SaveCurrentLabel(saveFileDialog1.FileName);
        }

        // ==========================================================
        // EINSTELLUNGEN
        // ==========================================================

        private void cbEndless_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.endless = cbEndless.Checked;
            Properties.Settings.Default.Save();
        }

        private void cbOntop_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = cbOntop.Checked;
            Properties.Settings.Default.onTop = cbOntop.Checked;
            Properties.Settings.Default.Save();
        }

        private void cbOpenWith_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.openWith = cbOpenWith.Checked;
            Properties.Settings.Default.Save();
        }

        // ==========================================================
        // UPDATE (eigene schlanke Implementierung, kein AutoUpdater.NET)
        // Liest version.xml vom GitHub-Master und vergleicht mit der
        // laufenden Assembly-Version.  Bei neuer Version wird der
        // Download-Link im Standard-Browser geöffnet.
        // ==========================================================

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;

                const string versionUrl =
                    "https://github.com/manfred-mueller/DHLabel/raw/master/version.xml";

                string xml;
                using (var wc = new WebClient())
                    xml = wc.DownloadString(versionUrl);

                var doc = new XmlDocument();
                doc.LoadXml(xml);

                string remoteVersionStr = doc.SelectSingleNode("//version")?.InnerText?.Trim();
                string downloadUrl      = doc.SelectSingleNode("//url")?.InnerText?.Trim();

                if (string.IsNullOrEmpty(remoteVersionStr))
                {
                    MessageBox.Show(Properties.Resources.UpdateErrorN + "Keine Versionsinformation gefunden.");
                    return;
                }

                var remoteVersion = new Version(remoteVersionStr);
                var localVersion  = Assembly.GetExecutingAssembly().GetName().Version;

                // Nur Major.Minor.Build vergleichen (Revision ignorieren)
                var local3  = new Version(localVersion.Major,  localVersion.Minor,  localVersion.Build);
                var remote3 = new Version(remoteVersion.Major, remoteVersion.Minor, remoteVersion.Build);

                if (remote3 > local3)
                {
                    string msg = string.Format(
                        "Version {0} ist verfügbar (aktuell: {1}).\nJetzt herunterladen?",
                        remoteVersionStr, local3);

                    if (MessageBox.Show(msg, "Update verfügbar",
                            MessageBoxButtons.YesNo, MessageBoxIcon.Information)
                        == DialogResult.Yes && !string.IsNullOrEmpty(downloadUrl))
                    {
                        Process.Start(new ProcessStartInfo(downloadUrl) { UseShellExecute = true });
                    }
                }
                else
                {
                    MessageBox.Show(
                        string.Format("Sie verwenden bereits die aktuelle Version ({0}).", local3),
                        "Kein Update",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Properties.Resources.UpdateErrorN + ex.Message);
            }
        }

        // ==========================================================
        // ABOUT / QUIT
        // ==========================================================

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutBox1().ShowDialog();
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        // ==========================================================
        // UI HILFSFUNKTIONEN
        // ==========================================================

        private void enableControls()
        {
            btnPrint.Enabled                 = true;
            btnSavePDF.Enabled               = true;
            savePDFToolStripMenuItem.Enabled = true;
            printToolStripMenuItem.Enabled   = true;
        }

        private void CreateStatusBar()
        {
            statusPanel.BorderStyle = StatusBarPanelBorderStyle.Sunken;
            statusPanel.AutoSize    = StatusBarPanelAutoSize.Spring;
            mainStatusBar.Panels.Add(statusPanel);
            mainStatusBar.ShowPanels = true;
            Controls.Add(mainStatusBar);
        }

        /// <summary>
        /// Prüft ob der "Öffnen mit"-Registry-Schlüssel gesetzt ist.
        /// RegistryKey wird korrekt per using disposed.
        /// </summary>
        private void checkKey()
        {
            using (RegistryKey progKey =
                Registry.CurrentUser.OpenSubKey(
                    "Software\\Classes\\" + Application.ProductName, false))
            {
                cbOpenWith.Checked = (progKey != null);
            }
        }

        public void setTitle()
        {
            Text = !string.IsNullOrEmpty(Properties.Settings.Default.printOn)
                ? Application.ProductName + " \u2013 " + Properties.Settings.Default.printOn
                : Application.ProductName;
        }

        /// <summary>
        /// Speichert das aktuelle Label als PDF.
        /// Berücksichtigt den endless-Modus (150 mm) vs. A6 (148 mm).
        /// Tempfile wird per try/finally auch bei Exceptions gelöscht.
        /// </summary>
        private void SaveCurrentLabel(string filename)
        {
            bool endless = Properties.Settings.Default.endless;

            try
            {
                using (var img = _pdfService.RenderPdf(_currentOriginalPdf, 300))
                {
                    var doc  = new PdfSharp.Pdf.PdfDocument();
                    var page = doc.AddPage();

                    double pageWidthMm  = endless ? 100.0 : 105.0;
                    double pageHeightMm = endless ? 150.0 : 148.0;

                    page.Width  = XUnit.FromMillimeter(pageWidthMm);
                    page.Height = XUnit.FromMillimeter(pageHeightMm);

                    using (XGraphics gfx = XGraphics.FromPdfPage(page))
                    {
                        double offsetXmm = (pageWidthMm - 100.0) / 2.0;

                        string tempPath = Path.GetTempFileName() + ".png";
                        try
                        {
                            img.Save(tempPath, System.Drawing.Imaging.ImageFormat.Png);

                            using (XImage xImg = XImage.FromFile(tempPath))
                            {
                                gfx.DrawImage(
                                    xImg,
                                    XUnit.FromMillimeter(offsetXmm).Point,
                                    0,
                                    XUnit.FromMillimeter(100).Point,
                                    XUnit.FromMillimeter(pageHeightMm).Point);
                            }
                        }
                        finally
                        {
                            if (File.Exists(tempPath))
                                File.Delete(tempPath);
                        }
                    }

                    doc.Save(filename);
                    doc.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(Properties.Resources.ErrorWhileSavingN + ex.Message);
            }
        }
    }
}
