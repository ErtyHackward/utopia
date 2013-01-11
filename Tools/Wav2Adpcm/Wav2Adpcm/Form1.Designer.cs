namespace Wav2Adpcm
{
    partial class Form1
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.label1 = new System.Windows.Forms.Label();
            this.Resultoutput = new System.Windows.Forms.ListBox();
            this.deleteOriginalFile = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AllowDrop = true;
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15.75F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Bold | System.Drawing.FontStyle.Italic))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(34, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(208, 25);
            this.label1.TabIndex = 0;
            this.label1.Text = "Drag Wav file here";
            this.label1.DragDrop += new System.Windows.Forms.DragEventHandler(this.label1_DragDrop);
            this.label1.DragEnter += new System.Windows.Forms.DragEventHandler(this.label1_DragEnter);
            // 
            // Resultoutput
            // 
            this.Resultoutput.FormattingEnabled = true;
            this.Resultoutput.HorizontalScrollbar = true;
            this.Resultoutput.Location = new System.Drawing.Point(12, 75);
            this.Resultoutput.Name = "Resultoutput";
            this.Resultoutput.Size = new System.Drawing.Size(258, 147);
            this.Resultoutput.TabIndex = 1;
            this.Resultoutput.DragDrop += new System.Windows.Forms.DragEventHandler(this.listBox1_DragDrop);
            this.Resultoutput.DragEnter += new System.Windows.Forms.DragEventHandler(this.listBox1_DragEnter);
            // 
            // deleteOriginalFile
            // 
            this.deleteOriginalFile.AutoSize = true;
            this.deleteOriginalFile.Location = new System.Drawing.Point(13, 229);
            this.deleteOriginalFile.Name = "deleteOriginalFile";
            this.deleteOriginalFile.Size = new System.Drawing.Size(109, 17);
            this.deleteOriginalFile.TabIndex = 2;
            this.deleteOriginalFile.Text = "Delete original file";
            this.deleteOriginalFile.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(282, 252);
            this.Controls.Add(this.deleteOriginalFile);
            this.Controls.Add(this.Resultoutput);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "Form1";
            this.Text = "Wav 2 Adpcm";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.Form1_Load);
            this.DragDrop += new System.Windows.Forms.DragEventHandler(this.Form1_DragDrop);
            this.DragEnter += new System.Windows.Forms.DragEventHandler(this.Form1_DragEnter);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox Resultoutput;
        private System.Windows.Forms.CheckBox deleteOriginalFile;
    }
}

