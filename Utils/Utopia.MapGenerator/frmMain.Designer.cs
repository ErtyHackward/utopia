namespace Utopia.MapGenerator
{
    partial class frmMain
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
            this.button1 = new System.Windows.Forms.Button();
            this.voronoiPolyNumeric = new System.Windows.Forms.NumericUpDown();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.voronoiSeedNumeric = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.voronoiRelaxNumeric = new System.Windows.Forms.NumericUpDown();
            this.label3 = new System.Windows.Forms.Label();
            this.noiseButton = new System.Windows.Forms.Button();
            this.noiseZoomNumeric = new System.Windows.Forms.NumericUpDown();
            this.label4 = new System.Windows.Forms.Label();
            this.elevateCheckBox = new System.Windows.Forms.CheckBox();
            this.label5 = new System.Windows.Forms.Label();
            this.noiseSeedNumeric = new System.Windows.Forms.NumericUpDown();
            this.bordersCheckBox = new System.Windows.Forms.CheckBox();
            this.makeIsandcheck = new System.Windows.Forms.CheckBox();
            this.centerElevationCheck = new System.Windows.Forms.CheckBox();
            this.moisturizeCheck = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.voronoiPolyNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.voronoiSeedNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.voronoiRelaxNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.noiseZoomNumeric)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.noiseSeedNumeric)).BeginInit();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(12, 257);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(120, 23);
            this.button1.TabIndex = 0;
            this.button1.Text = "Generate";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // voronoiPolyNumeric
            // 
            this.voronoiPolyNumeric.Location = new System.Drawing.Point(12, 54);
            this.voronoiPolyNumeric.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.voronoiPolyNumeric.Name = "voronoiPolyNumeric";
            this.voronoiPolyNumeric.Size = new System.Drawing.Size(120, 20);
            this.voronoiPolyNumeric.TabIndex = 1;
            this.voronoiPolyNumeric.Value = new decimal(new int[] {
            600,
            0,
            0,
            0});
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Location = new System.Drawing.Point(155, 19);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(724, 509);
            this.pictureBox1.TabIndex = 2;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            // 
            // voronoiSeedNumeric
            // 
            this.voronoiSeedNumeric.Location = new System.Drawing.Point(12, 139);
            this.voronoiSeedNumeric.Maximum = new decimal(new int[] {
            1000000000,
            0,
            0,
            0});
            this.voronoiSeedNumeric.Name = "voronoiSeedNumeric";
            this.voronoiSeedNumeric.Size = new System.Drawing.Size(120, 20);
            this.voronoiSeedNumeric.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 120);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Grid seed";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 35);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(80, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Polygons count";
            // 
            // voronoiRelaxNumeric
            // 
            this.voronoiRelaxNumeric.Location = new System.Drawing.Point(12, 97);
            this.voronoiRelaxNumeric.Name = "voronoiRelaxNumeric";
            this.voronoiRelaxNumeric.Size = new System.Drawing.Size(120, 20);
            this.voronoiRelaxNumeric.TabIndex = 6;
            this.voronoiRelaxNumeric.Value = new decimal(new int[] {
            3,
            0,
            0,
            0});
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(64, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Relax count";
            // 
            // noiseButton
            // 
            this.noiseButton.Location = new System.Drawing.Point(12, 422);
            this.noiseButton.Name = "noiseButton";
            this.noiseButton.Size = new System.Drawing.Size(120, 23);
            this.noiseButton.TabIndex = 8;
            this.noiseButton.Text = "Show elevation noise";
            this.noiseButton.UseVisualStyleBackColor = true;
            this.noiseButton.Click += new System.EventHandler(this.button2_Click);
            // 
            // noiseZoomNumeric
            // 
            this.noiseZoomNumeric.DecimalPlaces = 6;
            this.noiseZoomNumeric.Increment = new decimal(new int[] {
            0,
            0,
            0,
            0});
            this.noiseZoomNumeric.Location = new System.Drawing.Point(12, 396);
            this.noiseZoomNumeric.Maximum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.noiseZoomNumeric.Name = "noiseZoomNumeric";
            this.noiseZoomNumeric.Size = new System.Drawing.Size(120, 20);
            this.noiseZoomNumeric.TabIndex = 9;
            this.noiseZoomNumeric.Value = new decimal(new int[] {
            8,
            0,
            0,
            262144});
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 377);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(34, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Zoom";
            // 
            // elevateCheckBox
            // 
            this.elevateCheckBox.AutoSize = true;
            this.elevateCheckBox.Checked = true;
            this.elevateCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.elevateCheckBox.Location = new System.Drawing.Point(12, 166);
            this.elevateCheckBox.Name = "elevateCheckBox";
            this.elevateCheckBox.Size = new System.Drawing.Size(61, 17);
            this.elevateCheckBox.TabIndex = 11;
            this.elevateCheckBox.Text = "elevate";
            this.elevateCheckBox.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 335);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(79, 13);
            this.label5.TabIndex = 13;
            this.label5.Text = "Elevation Seed";
            // 
            // noiseSeedNumeric
            // 
            this.noiseSeedNumeric.Location = new System.Drawing.Point(12, 354);
            this.noiseSeedNumeric.Maximum = new decimal(new int[] {
            100000000,
            0,
            0,
            0});
            this.noiseSeedNumeric.Name = "noiseSeedNumeric";
            this.noiseSeedNumeric.Size = new System.Drawing.Size(120, 20);
            this.noiseSeedNumeric.TabIndex = 12;
            this.noiseSeedNumeric.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // bordersCheckBox
            // 
            this.bordersCheckBox.AutoSize = true;
            this.bordersCheckBox.Location = new System.Drawing.Point(79, 165);
            this.bordersCheckBox.Name = "bordersCheckBox";
            this.bordersCheckBox.Size = new System.Drawing.Size(61, 17);
            this.bordersCheckBox.TabIndex = 14;
            this.bordersCheckBox.Text = "borders";
            this.bordersCheckBox.UseVisualStyleBackColor = true;
            // 
            // makeIsandcheck
            // 
            this.makeIsandcheck.AutoSize = true;
            this.makeIsandcheck.Checked = true;
            this.makeIsandcheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.makeIsandcheck.Location = new System.Drawing.Point(12, 188);
            this.makeIsandcheck.Name = "makeIsandcheck";
            this.makeIsandcheck.Size = new System.Drawing.Size(82, 17);
            this.makeIsandcheck.TabIndex = 15;
            this.makeIsandcheck.Text = "make island";
            this.makeIsandcheck.UseVisualStyleBackColor = true;
            // 
            // centerElevationCheck
            // 
            this.centerElevationCheck.AutoSize = true;
            this.centerElevationCheck.Checked = true;
            this.centerElevationCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.centerElevationCheck.Location = new System.Drawing.Point(12, 211);
            this.centerElevationCheck.Name = "centerElevationCheck";
            this.centerElevationCheck.Size = new System.Drawing.Size(102, 17);
            this.centerElevationCheck.TabIndex = 16;
            this.centerElevationCheck.Text = "center elevation";
            this.centerElevationCheck.UseVisualStyleBackColor = true;
            // 
            // moisturizeCheck
            // 
            this.moisturizeCheck.AutoSize = true;
            this.moisturizeCheck.Location = new System.Drawing.Point(12, 234);
            this.moisturizeCheck.Name = "moisturizeCheck";
            this.moisturizeCheck.Size = new System.Drawing.Size(72, 17);
            this.moisturizeCheck.TabIndex = 17;
            this.moisturizeCheck.Text = "moisturize";
            this.moisturizeCheck.UseVisualStyleBackColor = true;
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(12, 452);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(120, 23);
            this.button2.TabIndex = 18;
            this.button2.Text = "Show moiture noise";
            this.button2.UseVisualStyleBackColor = true;
            this.button2.Click += new System.EventHandler(this.button2_Click_1);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(152, 3);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(35, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "label6";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(891, 534);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.moisturizeCheck);
            this.Controls.Add(this.centerElevationCheck);
            this.Controls.Add(this.makeIsandcheck);
            this.Controls.Add(this.bordersCheckBox);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.noiseSeedNumeric);
            this.Controls.Add(this.elevateCheckBox);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.noiseZoomNumeric);
            this.Controls.Add(this.noiseButton);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.voronoiRelaxNumeric);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.voronoiSeedNumeric);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.voronoiPolyNumeric);
            this.Controls.Add(this.button1);
            this.Name = "frmMain";
            this.Text = "Utopia world planner";
            ((System.ComponentModel.ISupportInitialize)(this.voronoiPolyNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.voronoiSeedNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.voronoiRelaxNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.noiseZoomNumeric)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.noiseSeedNumeric)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.NumericUpDown voronoiPolyNumeric;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.NumericUpDown voronoiSeedNumeric;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown voronoiRelaxNumeric;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button noiseButton;
        private System.Windows.Forms.NumericUpDown noiseZoomNumeric;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox elevateCheckBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.NumericUpDown noiseSeedNumeric;
        private System.Windows.Forms.CheckBox bordersCheckBox;
        private System.Windows.Forms.CheckBox makeIsandcheck;
        private System.Windows.Forms.CheckBox centerElevationCheck;
        private System.Windows.Forms.CheckBox moisturizeCheck;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label6;
    }
}

