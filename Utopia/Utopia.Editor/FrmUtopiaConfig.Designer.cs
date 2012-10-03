namespace Utopia.Editor
{
    partial class FrmUtopiaConfig
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.rangeBar1 = new Utopia.Editor.RangeBar();
            this.SuspendLayout();
            // 
            // rangeBar1
            // 
            this.rangeBar1.Location = new System.Drawing.Point(74, 49);
            this.rangeBar1.Name = "rangeBar1";
            this.rangeBar1.Size = new System.Drawing.Size(386, 68);
            this.rangeBar1.TabIndex = 0;
            // 
            // FrmUtopiaConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.rangeBar1);
            this.Name = "FrmUtopiaConfig";
            this.Size = new System.Drawing.Size(732, 354);
            this.ResumeLayout(false);

        }

        #endregion

        private RangeBar rangeBar1;

    }
}
