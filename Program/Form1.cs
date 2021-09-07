using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Spire.Pdf;
using Microsoft.Win32;

namespace DHLabel
{
    public partial class Form1 : Form
    {
        RegistryKey progKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + Application.ProductName, true);
        protected StatusBar mainStatusBar = new StatusBar();
        protected StatusBarPanel statusPanel = new StatusBarPanel();
        public Form1(string[] args)
        {
            InitializeComponent();
            CreateStatusBar();
            checkKey();
            if (args.Length == 1)
            {
                string path = args[0];
                if (System.IO.File.Exists(path))
                {
                    picboxLabel.Image = convertPDF(path);
                    statusPanel.Text = args[0];
                    enableControls();
                }
            } else
            {
                picboxLabel.Image = dropBitmap();
            }
            cbOntop.Checked = Properties.Settings.Default.onTop;
        }

        private Bitmap dropBitmap()
        {
            Bitmap initialBitmap = new Bitmap(785, 475);
            using (Graphics g = Graphics.FromImage(initialBitmap))
            {
                StringFormat sf = new StringFormat();
                sf.Alignment = StringAlignment.Center;
                sf.LineAlignment = StringAlignment.Center;
                g.Clear(Color.White);
                g.DrawString(Properties.Resources.DropPDFHere, new Font("Microsoft Sans Serif", 24, FontStyle.Italic), Brushes.Black, new Point(230, 200));
            }
            return initialBitmap;
        }

        private Bitmap getClip(Image source, Rectangle rect)
        {
            Bitmap tempBitmap = new Bitmap(rect.Width, rect.Height);

            using (Graphics g = Graphics.FromImage(tempBitmap))
            {
                g.DrawImage(source, new Rectangle(0, 0, tempBitmap.Width, tempBitmap.Height), rect, GraphicsUnit.Pixel);
            }

            return tempBitmap;
        }

        private Bitmap convertPDF(string filename)
        {
            statusPanel.Text = string.Format(Properties.Resources.ConvertingPleaseWait, filename);
            PdfDocument doc = new PdfDocument();
            Image source;
            Rectangle rectMain = new Rectangle(1860, 95, 1075, 705);
            Rectangle rectMiddle = new Rectangle(1850, 885, 860, 155);
            Rectangle rectBar = new Rectangle(1860, 1345, 1075, 810);
            Rectangle rectLine = new Rectangle(1860, 1018, 1075, 17);
            Rectangle rectGoGreen = new Rectangle(2120, 810, 390, 75);
            Rectangle rectPayed = new Rectangle(2705, 890, 215, 80);
            Bitmap bitmapLabel = new Bitmap(1640, 1164);

            try
            {
                doc.LoadFromFile(filename);
                source = doc.SaveAsImage(0, 273, 273);
                source.RotateFlip(RotateFlipType.Rotate90FlipNone);
                Bitmap bitmapMain = getClip(source, rectMain);
                Bitmap bitmapMiddle = getClip(source, rectMiddle);
                Bitmap bitmapBar = getClip(source, rectBar);
                Bitmap bitmapLine = getClip(source, rectLine);
                Bitmap bitmapGoGreen = getClip(source, rectGoGreen);
                Bitmap bitmapPayed = getClip(source, rectPayed);
                bitmapMain.RotateFlip(RotateFlipType.Rotate270FlipNone);
                bitmapMiddle.RotateFlip(RotateFlipType.Rotate270FlipNone);
                bitmapBar.RotateFlip(RotateFlipType.Rotate270FlipNone);
                bitmapPayed.RotateFlip(RotateFlipType.Rotate270FlipNone);
                bitmapLine.RotateFlip(RotateFlipType.Rotate270FlipNone);
                bitmapGoGreen.RotateFlip(RotateFlipType.Rotate270FlipNone);

                using (Graphics g = Graphics.FromImage(bitmapLabel))
                {
                    g.Clear(Color.White);
                    g.DrawImage(bitmapMain, 0, 1);
                    g.DrawImage(bitmapBar, 745, -8);
                    g.DrawImage(bitmapMiddle, 607, 219);
                    g.DrawImage(bitmapPayed, 525, 4);
                    g.DrawImage(bitmapLine, 595, -8);
                    g.DrawImage(bitmapGoGreen, 645, 0);
                }

                return bitmapLabel;
            }
            catch (Spire.Pdf.Exceptions.PdfDocumentException) {
                return null;
            }
        }


        bool ForcePageSize(PrintDocument MyPrintDocument, PaperKind MyPaperKind)
        {
            for (int i = 0; i < MyPrintDocument.PrinterSettings.PaperSizes.Count; ++i)
            {
                if (MyPrintDocument.PrinterSettings.PaperSizes[i].Kind == MyPaperKind)
                {
                    MyPrintDocument.DefaultPageSettings.PaperSize = MyPrintDocument.PrinterSettings.PaperSizes[i];
                    return true;
                }
            }
            return false;
        }

        private bool savePDF(string filename)
        {
            try
            {
                PdfDocument doc = new PdfDocument();
                PdfSection section = doc.Sections.Add();
                PdfPageBase page = doc.Pages.Add(PdfPageSize.A6, new Spire.Pdf.Graphics.PdfMargins(0), PdfPageRotateAngle.RotateAngle90, PdfPageOrientation.Landscape);
                Spire.Pdf.Graphics.PdfImage image = Spire.Pdf.Graphics.PdfImage.FromImage(picboxLabel.Image);
                float widthFitRate = image.PhysicalDimension.Width / page.Canvas.ClientSize.Width;
                float heightFitRate = image.PhysicalDimension.Height / page.Canvas.ClientSize.Height;
                float fitRate = Math.Max(widthFitRate, heightFitRate);
                float fitWidth = image.PhysicalDimension.Width / fitRate;
                float fitHeight = image.PhysicalDimension.Height / fitRate;

                doc.DocumentInformation.Author = Properties.Resources.Copyright;
                doc.DocumentInformation.Title = Properties.Resources.DHLabelPackageLabel;
                doc.DocumentInformation.Producer = Application.ProductName;
                doc.DocumentInformation.Keywords = Properties.Resources.DHLabelDHLPackageLabel;
                page.Canvas.DrawImage(image, 10, 10, fitWidth, fitHeight);
                doc.SaveToFile(filename);
                doc.Close();
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void enableControls()
        {
            btnPrint.Enabled = true;
            btnSavePDF.Enabled = true;
            savePDFToolStripMenuItem.Enabled = true;
            printToolStripMenuItem.Enabled = true;
        }

        private void printLabel(Image label)
        {
            printDocument1.DefaultPageSettings.Landscape = true;
            printDocument1.PrintPage += (sender, args) =>
            {
                Rectangle m = args.MarginBounds;

                if ((double)label.Width / (double)label.Height > (double)m.Width / (double)m.Height)
                {
                    m.Height = (int)((double)label.Height / (double)label.Width * (double)m.Width);
                }
                else
                {
                    m.Width = (int)((double)label.Width / (double)label.Height * (double)m.Height);
                }

                args.Graphics.DrawImage(picboxLabel.Image, m);
            };
            Margins margins = new Margins(0, 0, 0, 0);
            printDocument1.DefaultPageSettings.Margins = margins;
            ForcePageSize(printDocument1, PaperKind.A6);
            printDocument1.Print();
        }

        private void printDocument1_PrintPage(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawImage(picboxLabel.Image, 0, 0);
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            Bitmap bitmapPDF = convertPDF(files[0]);
            if (bitmapPDF == null)
            {
                MessageBox.Show(Properties.Resources.NoValidPackageLabel);
            }
            else
            {
                picboxLabel.Image = bitmapPDF;
                statusPanel.Text = files[0];
                enableControls();
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private async void printLabel_Click(object sender, EventArgs e)
        {
            if (printDialog1.ShowDialog() == DialogResult.OK)
            {
                statusPanel.Text = string.Format(Properties.Resources.ConvertingDataForPrintingPleaseWait);
                await Task.Run(() => printLabel(picboxLabel.Image));
                statusPanel.Text = string.Format(Properties.Resources.Done);
            }
        }

        private void saveLabel_Click(object sender, EventArgs e)
        {
            bool userClickedOK = (saveFileDialog1.ShowDialog() == DialogResult.OK);

            if (userClickedOK == true)
            {
                if (savePDF(saveFileDialog1.FileName) == false)
                {
                    MessageBox.Show(Properties.Resources.TheDHLLabelCouldNotBeSaved);
                }
            }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox1 aboutBox = new AboutBox1();

            aboutBox.ShowDialog();
        }

        private void openPDF_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Bitmap bitmapPDF = convertPDF(openFileDialog1.FileName);
                if (bitmapPDF == null)
                {
                    MessageBox.Show(Properties.Resources.NoValidPackageLabel);
                }
                else
                {
                    picboxLabel.Image = bitmapPDF;
                    enableControls();
                }
            }
        }

        private void checkKey()
        {
            if (progKey != null)
            {
                cbOpenWith.Checked = true;
            }
            else
            {
                cbOpenWith.Checked = false;
            }
        }


        private void CreateStatusBar()
        {
            statusPanel.BorderStyle = StatusBarPanelBorderStyle.Sunken;
            statusPanel.Text = Properties.Resources.ApplicationStarted;
            statusPanel.ToolTipText = Properties.Resources.ShowStatusMessages;
            statusPanel.AutoSize = StatusBarPanelAutoSize.Spring;
            mainStatusBar.Panels.Add(statusPanel);
            mainStatusBar.ShowPanels = true;
            this.Controls.Add(mainStatusBar);
        }

        private void cbOntop_CheckedChanged(object sender, EventArgs e)
        {
            this.TopMost = cbOntop.Checked;
            Properties.Settings.Default.onTop = cbOntop.Checked;
            Properties.Settings.Default.Save();
        }

        private void cbOpenWith_CheckedChanged(object sender, EventArgs e)
        {
            RegistryKey myProgKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + Application.ProductName + "\\shell\\open\\command", true);
            RegistryKey myExtKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\.pdf", true);
            if (!cbOpenWith.Checked == true)
            {
                if (myProgKey != null)
                {
                    Registry.CurrentUser.DeleteSubKeyTree("Software\\Classes\\" + Application.ProductName);
                }
                if (myExtKey != null)
                {
                    myExtKey.SetValue("", "");
                    myExtKey.Close();
                }
            }
            else
            {
                if (myProgKey == null)
                {
                    myProgKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\" + Application.ProductName + "\\shell\\open\\command");
                    myProgKey.SetValue("", Application.ExecutablePath + " %1");
                    myProgKey.Close();
                }
                if (myExtKey == null)
                {
                    myExtKey = Registry.CurrentUser.CreateSubKey("Software\\Classes\\.pdf");
                }
                myExtKey.SetValue("", Application.ProductName);
                myExtKey.Close();
            }
            Properties.Settings.Default.openWith = cbOpenWith.Checked;
            Properties.Settings.Default.Save();
        }
    }
}
