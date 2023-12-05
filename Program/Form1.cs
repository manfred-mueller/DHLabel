using Microsoft.Win32;
using Spire.Pdf;
using System;
using System.Drawing;
using System.Drawing.Printing;
using System.Windows.Forms;
using AutoUpdaterDotNET;
using Image = System.Drawing.Image;
using PrintDialog = System.Windows.Forms.PrintDialog;
using System.Xml.Linq;

namespace DHLabel
{
    public partial class Form1 : Form
    {
        RegistryKey progKey = Registry.CurrentUser.OpenSubKey("Software\\Classes\\" + Application.ProductName, true);
        protected StatusBar mainStatusBar = new StatusBar();
        protected StatusBarPanel statusPanel = new StatusBarPanel();
        public bool isHeavy;
        public int labelType;
        public string titleString;
        public PdfDocument pdfDoc;
        public Form1(string[] args)
        {
            InitializeComponent();
            CreateStatusBar();
            checkKey();
            cbOntop.Checked = Properties.Settings.Default.onTop;
            cbOpenWith.Checked = Properties.Settings.Default.openWith;
            labelType = Properties.Settings.Default.labelType;
            rbStandard.Checked = labelType == 0;
            rbBusiness.Checked = labelType == 1;
            rbReturn.Checked = labelType == 2;
            cbHeavy.Checked = Properties.Settings.Default.heavyPackage;
            isHeavy = cbHeavy.Checked;
            setTitle();
            if (args.Length == 1)
            {
                string path = args[0];
                if (System.IO.File.Exists(path))
                {
                    picboxLabel.Image = convertPDF(path);
                    statusPanel.Text = args[0];
                    enableControls();
                }
            }
            else
            {
                picboxLabel.Image = dropBitmap();
            }
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
            ClearCaches();
            statusPanel.Text = string.Format(Properties.Resources.ConvertingPleaseWait, filename);

            pdfDoc = new PdfDocument();
            Image source;
            Rectangle rectMain;
            Rectangle rectMiddle;
            Rectangle rectBar;
            Rectangle rectLine;
            Rectangle rectMail;
            Rectangle rectGoGreen;
            Rectangle rectPayed;
            Bitmap bitmapLabel;
            if (labelType == 2)
            {
                rectMain = new Rectangle(1920, 65, 1075, 755);
                rectMiddle = new Rectangle(1850, 885, 860, 145);
                rectBar = new Rectangle(1912, 1100, 1075, 860);
            }
            else if (labelType == 1)
            {
                rectMain = new Rectangle(-20, 0, 1120, 705);
                rectMiddle = new Rectangle(0, 700, 1090, 140);
                rectBar = new Rectangle(20, 1230, 1075, 798);
            }
            else
            {
                rectMain = new Rectangle(1860, 95, 1075, 705);
                rectMiddle = new Rectangle(1850, 885, 860, 145);
                rectBar = new Rectangle(1860, 1350, 1075, 860);
            }

            rectLine = new Rectangle(1860, 1018, 1075, 17);
            rectMail = new Rectangle(2625, 705, 300, 65);
            rectGoGreen = new Rectangle(2120, 810, 430, 75);
            rectPayed = new Rectangle(2705, 890, 215, 80);
            bitmapLabel = new Bitmap(1640, 1164);

            try
            {
                Bitmap bitmapMain;
                Bitmap bitmapMiddle;
                Bitmap bitmapBar;
                Bitmap bitmapLine;
                Bitmap bitmapMail;
                Bitmap bitmapGoGreen;
                Bitmap bitmapPayed;

                pdfDoc.LoadFromFile(filename);
                source = pdfDoc.SaveAsImage(0, 273, 273);
                //source.Save("C://Temp//dhlabel.jpg");

                if (labelType != 1) source.RotateFlip(RotateFlipType.Rotate90FlipNone);
                bitmapMain = getClip(source, rectMain);
                bitmapMiddle = getClip(source, rectMiddle);
                bitmapBar = getClip(source, rectBar);
                bitmapMain.RotateFlip(RotateFlipType.Rotate270FlipNone);
                bitmapMiddle.RotateFlip(RotateFlipType.Rotate270FlipNone);
                bitmapBar.RotateFlip(RotateFlipType.Rotate270FlipNone);
                bitmapPayed = getClip(source, rectPayed);
                bitmapLine = getClip(source, rectLine);
                bitmapMail = getClip(source, rectMail);
                bitmapGoGreen = getClip(source, rectGoGreen);
                bitmapPayed.RotateFlip(RotateFlipType.Rotate270FlipNone);
                bitmapLine.RotateFlip(RotateFlipType.Rotate270FlipNone);
                bitmapMail.RotateFlip(RotateFlipType.Rotate270FlipNone);
                bitmapGoGreen.RotateFlip(RotateFlipType.Rotate270FlipNone);

                Bitmap bitmapHeavy = Properties.Resources.Heavy;
                bitmapHeavy.RotateFlip(RotateFlipType.Rotate270FlipNone);

                using (Graphics g = Graphics.FromImage(bitmapLabel))
                {
                    g.Clear(Color.White);
                    g.DrawImage(bitmapMain, 0, 1);
                    if (labelType == 2)
                    {
                        g.DrawImage(bitmapBar, 760, -70);
                        if (isHeavy) g.DrawImage(bitmapHeavy, 475, 4);

                    }
                    else if (labelType == 1)
                    {
                        g.DrawImage(bitmapBar, 705, 5);
                        g.DrawImage(bitmapMiddle, 620, 5);
                        if (isHeavy) g.DrawImage(bitmapHeavy, 475, 4);

                    }
                    else
                    {
                        g.DrawImage(bitmapMiddle, 607, 219);
                        g.DrawImage(bitmapBar, 710, -8);
                        g.DrawImage(bitmapPayed, 605, 4);
                        g.DrawImage(bitmapMail, 400, 10);
                        g.DrawImage(bitmapLine, 595, -8);
                        g.DrawImage(bitmapGoGreen, 675, 4, 37, 215);
                        if (isHeavy) g.DrawImage(bitmapHeavy, 460, 4);
                    }
                }

                return bitmapLabel;
            }
            catch (Spire.Pdf.Exceptions.PdfDocumentException)
            {
                return null;
            }
            finally
            {
                ClearCaches();
            }
        }
        private void ClearCaches()
        {
            pdfDoc = new PdfDocument();
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
                pdfDoc = new PdfDocument();
                PdfSection section = pdfDoc.Sections.Add();
                PdfPageBase page = pdfDoc.Pages.Add(PdfPageSize.A6, new Spire.Pdf.Graphics.PdfMargins(0), PdfPageRotateAngle.RotateAngle90, PdfPageOrientation.Landscape);
                Spire.Pdf.Graphics.PdfImage image = Spire.Pdf.Graphics.PdfImage.FromImage(picboxLabel.Image);
                float widthFitRate = image.PhysicalDimension.Width / page.Canvas.ClientSize.Width;
                float heightFitRate = image.PhysicalDimension.Height / page.Canvas.ClientSize.Height;
                float fitRate = Math.Max(widthFitRate, heightFitRate);
                float fitWidth = image.PhysicalDimension.Width / fitRate;
                float fitHeight = image.PhysicalDimension.Height / fitRate;

                pdfDoc.DocumentInformation.Author = Properties.Resources.Copyright;
                pdfDoc.DocumentInformation.Title = Properties.Resources.DHLabelPackageLabel;
                pdfDoc.DocumentInformation.Producer = Application.ProductName;
                pdfDoc.DocumentInformation.Keywords = Properties.Resources.DHLabelDHLPackageLabel;
                page.Canvas.DrawImage(image, 10, 10, fitWidth, fitHeight);
                pdfDoc.SaveToFile(filename);
                pdfDoc.Close();
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

        private void PrintLabel(Image label)
        {
            printDocument1 = new PrintDocument();
            // Set the printing properties once instead of in each call
            if (printDocument1.PrinterSettings.PrinterName != Properties.Settings.Default.printOn)
            {
                printDocument1.DefaultPageSettings.Landscape = true;
                Margins margins = new Margins(12, 0, 15, 0);
                printDocument1.OriginAtMargins = true;
                printDocument1.DefaultPageSettings.Margins = margins;
                ForcePageSize(printDocument1, PaperKind.A6);

                // Set the printer name
                string printerName = Properties.Settings.Default.printOn;
                if (string.IsNullOrEmpty(printerName))
                {
                    TopMost = false;
                    setPrinter();
                    if (!string.IsNullOrEmpty(Properties.Settings.Default.printOn))
                    {
                        printerName = Properties.Settings.Default.printOn;
                    }
                }

                if (!string.IsNullOrEmpty(printerName))
                {
                    TopMost = false;

                    // Set up printing event
                    printDocument1.PrintPage += (sender, args) =>
                    {
                        Rectangle printRect = args.PageBounds;

                        if ((double)label.Width / label.Height > (double)printRect.Width / printRect.Height)
                        {
                            printRect.Height = (int)((double)label.Height / label.Width * printRect.Width);
                        }
                        else
                        {
                            printRect.Width = (int)((double)label.Width / label.Height * printRect.Height);
                        }

                        args.Graphics.DrawImage(label, printRect);
                    };

                    // Set printer name
                    printDocument1.PrinterSettings.PrinterName = printerName;
                }
            }

            // Print
            printDocument1.Print();
            // Restore TopMost setting
            TopMost = cbOntop.Checked;
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

        private void updateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            TopMost = false;
            AutoUpdater.Start("https://github.com/manfred-mueller/DHLabel/raw/master/version.xml");
        }


        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }


        private void printLabel_Click(object sender, EventArgs e)
        {
            statusPanel.Text = string.Format(Properties.Resources.ConvertingDataForPrintingPleaseWait);
            PrintLabel(picboxLabel.Image);
            statusPanel.Text = string.Format(Properties.Resources.Done);
        }

        private void setPrinter_Click(object sender, EventArgs e)
        {
            setPrinter();
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

        public void setPrinter()
        {
            PrintDialog PrintDialog = new PrintDialog();
            if (PrintDialog.ShowDialog() == DialogResult.OK)
            {
                Properties.Settings.Default.printOn = PrintDialog.PrinterSettings.PrinterName;
                Properties.Settings.Default.Save();
                setTitle();
            }
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
            Controls.Add(mainStatusBar);
        }

        private void cbOntop_CheckedChanged(object sender, EventArgs e)
        {
            TopMost = cbOntop.Checked;
            Properties.Settings.Default.onTop = cbOntop.Checked;
            Properties.Settings.Default.Save();
        }

        private void rbBusiness_CheckedChanged(object sender, EventArgs e)
        {
            if (rbBusiness.Checked)
            {
                labelType = 1;
                Properties.Settings.Default.labelType = 1;
                Properties.Settings.Default.Save();
            }
        }

        private void rbReturn_CheckedChanged(object sender, EventArgs e)
        {
            if (rbReturn.Checked)
            {
                labelType = 2;
                Properties.Settings.Default.labelType = 2;
                Properties.Settings.Default.Save();
            }
        }

        private void rbStandard_CheckedChanged(object sender, EventArgs e)
        {
            if (rbStandard.Checked)
            {
                labelType = 0;
                Properties.Settings.Default.labelType = 0;
                Properties.Settings.Default.Save();
            }
        }

        private void cbHeavy_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.heavyPackage = cbHeavy.Checked;
            isHeavy = cbHeavy.Checked;
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

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            try
            {
                string[] args = Environment.GetCommandLineArgs();
                LoadFile(args[1]);
            }
            catch (IndexOutOfRangeException)
            {
                picboxLabel.Image = dropBitmap();
            }
        }
        public void LoadFile(string file)
        {
            if (System.IO.File.Exists(file))
            {
                picboxLabel.Image = convertPDF(file);
                statusPanel.Text = file;
                enableControls();
            }

        }

        public void setTitle()
        {
            if (!string.IsNullOrEmpty(Properties.Settings.Default.printOn))
            {
                Text = string.Format(Application.ProductName + Properties.Resources.Printer + Properties.Settings.Default.printOn);
                Refresh();
            }
            else
            {
                Text = Application.ProductName;
                Refresh();
            }
        }
    }
}
