using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace DHLabel
{
    partial class Form1
    {

        private Dictionary<string, SizeF> paperSizes = new Dictionary<string, SizeF>();

        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        
        /// <summary>
                 /// Erforderliche Methode für die Designerunterstützung.
                 /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
                 /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.btnPrint = new System.Windows.Forms.Button();
            this.picboxLabel = new System.Windows.Forms.PictureBox();
            this.btnSavePDF = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openPDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.savePDFToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.setPrinterToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.printToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.updateToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.quitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.printDialog1 = new System.Windows.Forms.PrintDialog();
            this.printDocument1 = new System.Drawing.Printing.PrintDocument();
            this.btnOpen = new System.Windows.Forms.Button();
            this.cbOpenWith = new System.Windows.Forms.CheckBox();
            this.cbOntop = new System.Windows.Forms.CheckBox();
            this.labelTypeBox = new System.Windows.Forms.Panel();
            this.rbReturn = new System.Windows.Forms.RadioButton();
            this.rbBusiness = new System.Windows.Forms.RadioButton();
            this.rbStandard = new System.Windows.Forms.RadioButton();
            ((System.ComponentModel.ISupportInitialize)(this.picboxLabel)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.labelTypeBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnPrint
            // 
            this.btnPrint.Enabled = false;
            this.btnPrint.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnPrint.Image = global::DHLabel.Properties.Resources.document_print;
            this.btnPrint.Location = new System.Drawing.Point(12, 30);
            this.btnPrint.Name = "btnPrint";
            this.btnPrint.Size = new System.Drawing.Size(112, 46);
            this.btnPrint.TabIndex = 1;
            this.btnPrint.Text = global::DHLabel.Properties.Resources.Print;
            this.btnPrint.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnPrint.UseVisualStyleBackColor = true;
            this.btnPrint.Click += new System.EventHandler(this.printLabel_Click);
            // 
            // picboxLabel
            // 
            this.picboxLabel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.picboxLabel.InitialImage = null;
            this.picboxLabel.Location = new System.Drawing.Point(0, 104);
            this.picboxLabel.Name = "picboxLabel";
            this.picboxLabel.Size = new System.Drawing.Size(564, 397);
            this.picboxLabel.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.picboxLabel.TabIndex = 8;
            this.picboxLabel.TabStop = false;
            // 
            // btnSavePDF
            // 
            this.btnSavePDF.Enabled = false;
            this.btnSavePDF.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSavePDF.Image = global::DHLabel.Properties.Resources.document_save;
            this.btnSavePDF.Location = new System.Drawing.Point(129, 30);
            this.btnSavePDF.Name = "btnSavePDF";
            this.btnSavePDF.Size = new System.Drawing.Size(112, 46);
            this.btnSavePDF.TabIndex = 2;
            this.btnSavePDF.Text = global::DHLabel.Properties.Resources.SavePDF;
            this.btnSavePDF.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnSavePDF.UseVisualStyleBackColor = true;
            this.btnSavePDF.Click += new System.EventHandler(this.saveLabel_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(564, 24);
            this.menuStrip1.TabIndex = 3;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openPDFToolStripMenuItem,
            this.savePDFToolStripMenuItem,
            this.toolStripSeparator2,
            this.setPrinterToolStripMenuItem,
            this.printToolStripMenuItem,
            this.toolStripSeparator1,
            this.updateToolStripMenuItem,
            this.quitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = global::DHLabel.Properties.Resources.File;
            // 
            // openPDFToolStripMenuItem
            // 
            this.openPDFToolStripMenuItem.Image = global::DHLabel.Properties.Resources.document_open;
            this.openPDFToolStripMenuItem.Name = "openPDFToolStripMenuItem";
            this.openPDFToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.openPDFToolStripMenuItem.Text = global::DHLabel.Properties.Resources.OpenPDF;
            this.openPDFToolStripMenuItem.Click += new System.EventHandler(this.openPDF_Click);
            // 
            // savePDFToolStripMenuItem
            // 
            this.savePDFToolStripMenuItem.Enabled = false;
            this.savePDFToolStripMenuItem.Image = global::DHLabel.Properties.Resources.document_save;
            this.savePDFToolStripMenuItem.Name = "savePDFToolStripMenuItem";
            this.savePDFToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.savePDFToolStripMenuItem.Text = global::DHLabel.Properties.Resources.SavePDF;
            this.savePDFToolStripMenuItem.Click += new System.EventHandler(this.saveLabel_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(153, 6);
            // 
            // setPrinterToolStripMenuItem
            // 
            this.setPrinterToolStripMenuItem.Image = global::DHLabel.Properties.Resources.system_run;
            this.setPrinterToolStripMenuItem.Name = "setPrinterToolStripMenuItem";
            this.setPrinterToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.setPrinterToolStripMenuItem.Text = global::DHLabel.Properties.Resources.SetLabelPrinter;
            this.setPrinterToolStripMenuItem.Click += new System.EventHandler(this.setPrinter_Click);
            // 
            // printToolStripMenuItem
            // 
            this.printToolStripMenuItem.Enabled = false;
            this.printToolStripMenuItem.Image = global::DHLabel.Properties.Resources.document_print;
            this.printToolStripMenuItem.Name = "printToolStripMenuItem";
            this.printToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.printToolStripMenuItem.Text = global::DHLabel.Properties.Resources.Print;
            this.printToolStripMenuItem.Click += new System.EventHandler(this.printLabel_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(153, 6);
            // 
            // updateToolStripMenuItem
            // 
            this.updateToolStripMenuItem.Image = global::DHLabel.Properties.Resources.package_upgrade;
            this.updateToolStripMenuItem.Name = "updateToolStripMenuItem";
            this.updateToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.updateToolStripMenuItem.Text = global::DHLabel.Properties.Resources.SearchUpdate;
            this.updateToolStripMenuItem.Click += new System.EventHandler(this.updateToolStripMenuItem_Click);
            // 
            // quitToolStripMenuItem
            // 
            this.quitToolStripMenuItem.Image = global::DHLabel.Properties.Resources.application_exit;
            this.quitToolStripMenuItem.Name = "quitToolStripMenuItem";
            this.quitToolStripMenuItem.Size = new System.Drawing.Size(156, 22);
            this.quitToolStripMenuItem.Text = global::DHLabel.Properties.Resources.Quit;
            this.quitToolStripMenuItem.Click += new System.EventHandler(this.quitToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(24, 20);
            this.toolStripMenuItem1.Text = "&?";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = global::DHLabel.Properties.Resources.About;
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = global::DHLabel.Properties.Resources.PDFPackageLabelsPdfAllFiles;
            this.openFileDialog1.Title = global::DHLabel.Properties.Resources.OpenPDFPackageLabel;
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "pdf";
            this.saveFileDialog1.FileName = "DHL.pdf";
            this.saveFileDialog1.Filter = global::DHLabel.Properties.Resources.PDFFilesPdfAllFiles;
            this.saveFileDialog1.Title = global::DHLabel.Properties.Resources.SaveLabelAsPDFFile;
            // 
            // printDialog1
            // 
            this.printDialog1.AllowPrintToFile = false;
            this.printDialog1.UseEXDialog = true;
            // 
            // btnOpen
            // 
            this.btnOpen.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOpen.Image = global::DHLabel.Properties.Resources.document_open;
            this.btnOpen.Location = new System.Drawing.Point(245, 30);
            this.btnOpen.Name = "btnOpen";
            this.btnOpen.Size = new System.Drawing.Size(94, 46);
            this.btnOpen.TabIndex = 0;
            this.btnOpen.Text = global::DHLabel.Properties.Resources.OpenPDF;
            this.btnOpen.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnOpen.UseVisualStyleBackColor = true;
            this.btnOpen.Click += new System.EventHandler(this.openPDF_Click);
            // 
            // cbOpenWith
            // 
            this.cbOpenWith.Location = new System.Drawing.Point(442, 31);
            this.cbOpenWith.Name = "cbOpenWith";
            this.cbOpenWith.Size = new System.Drawing.Size(116, 16);
            this.cbOpenWith.TabIndex = 3;
            this.cbOpenWith.Text = global::DHLabel.Properties.Resources.OpenWith;
            this.cbOpenWith.CheckedChanged += new System.EventHandler(this.cbOpenWith_CheckedChanged);
            // 
            // cbOntop
            // 
            this.cbOntop.Location = new System.Drawing.Point(442, 46);
            this.cbOntop.Name = "cbOntop";
            this.cbOntop.Size = new System.Drawing.Size(116, 16);
            this.cbOntop.TabIndex = 3;
            this.cbOntop.Text = global::DHLabel.Properties.Resources.AlwaysOnTop;
            this.cbOntop.CheckedChanged += new System.EventHandler(this.cbOntop_CheckedChanged);
            // 
            // labelTypeBox
            // 
            this.labelTypeBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.labelTypeBox.Controls.Add(this.rbReturn);
            this.labelTypeBox.Controls.Add(this.rbBusiness);
            this.labelTypeBox.Controls.Add(this.rbStandard);
//            this.labelTypeBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.labelTypeBox.Location = new System.Drawing.Point(346, 30);
            this.labelTypeBox.Name = "labelTypeBox";
            this.labelTypeBox.Size = new System.Drawing.Size(96, 48);
            this.labelTypeBox.TabIndex = 12;
            this.labelTypeBox.TabStop = false;
            // 
            // rbReturn
            // 
            this.rbReturn.AutoSize = true;
            this.rbReturn.Location = new System.Drawing.Point(0, 30);
            this.rbReturn.Name = "rbReturn";
            this.rbReturn.Size = new System.Drawing.Size(82, 17);
            this.rbReturn.TabIndex = 2;
            this.rbReturn.TabStop = true;
            this.rbReturn.Text = global::DHLabel.Properties.Resources.ReturnLabel;
            this.rbReturn.UseVisualStyleBackColor = true;
            this.rbReturn.CheckedChanged += new System.EventHandler(this.rbReturn_CheckedChanged);
            // 
            // rbBusiness
            // 
            this.rbBusiness.AutoSize = true;
            this.rbBusiness.Location = new System.Drawing.Point(0, 15);
            this.rbBusiness.Name = "rbBusiness";
            this.rbBusiness.Size = new System.Drawing.Size(92, 17);
            this.rbBusiness.TabIndex = 1;
            this.rbBusiness.TabStop = true;
            this.rbBusiness.Text = global::DHLabel.Properties.Resources.BusinessLabel;
            this.rbBusiness.UseVisualStyleBackColor = true;
            this.rbBusiness.CheckedChanged += new System.EventHandler(this.rbBusiness_CheckedChanged);
            // 
            // rbStandard
            // 
            this.rbStandard.AutoSize = true;
            this.rbStandard.Location = new System.Drawing.Point(0, 0);
            this.rbStandard.Name = "rbStandard";
            this.rbStandard.Size = new System.Drawing.Size(93, 17);
            this.rbStandard.TabIndex = 0;
            this.rbStandard.TabStop = true;
            this.rbStandard.Text = global::DHLabel.Properties.Resources.StandardLabel;
            this.rbStandard.UseVisualStyleBackColor = true;
            this.rbStandard.CheckedChanged += new System.EventHandler(this.rbStandard_CheckedChanged);
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(564, 501);
            this.Controls.Add(this.labelTypeBox);
            this.Controls.Add(this.btnOpen);
            this.Controls.Add(this.cbOpenWith);
            this.Controls.Add(this.cbOntop);
            this.Controls.Add(this.btnPrint);
            this.Controls.Add(this.btnSavePDF);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.picboxLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(580, 540);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DHLabel";
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            ((System.ComponentModel.ISupportInitialize)(this.picboxLabel)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.labelTypeBox.ResumeLayout(false);
            this.labelTypeBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnPrint;
        private System.Windows.Forms.PictureBox picboxLabel;
        private System.Windows.Forms.Button btnSavePDF;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem printToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setPrinterToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem savePDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem updateToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem quitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openPDFToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.PrintDialog printDialog1;
        private System.Drawing.Printing.PrintDocument printDocument1;
        private System.Windows.Forms.Button btnOpen;
        private System.Windows.Forms.CheckBox cbOntop;
        private System.Windows.Forms.CheckBox cbOpenWith;
        private System.Windows.Forms.Panel labelTypeBox;
        private System.Windows.Forms.RadioButton rbReturn;
        private System.Windows.Forms.RadioButton rbBusiness;
        private System.Windows.Forms.RadioButton rbStandard;
    }
}

