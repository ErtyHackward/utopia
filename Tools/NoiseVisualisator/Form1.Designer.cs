namespace NoiseVisualisator
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
            this.btStart = new System.Windows.Forms.Button();
            this.bt2DRender = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label3 = new System.Windows.Forms.Label();
            this.lblGenerationTime = new System.Windows.Forms.Label();
            this.forward = new System.Windows.Forms.Button();
            this.backward = new System.Windows.Forms.Button();
            this.withThresHold = new System.Windows.Forms.CheckBox();
            this.thresholdValue = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBelow = new System.Windows.Forms.TextBox();
            this.withBelow = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btStart
            // 
            this.btStart.Location = new System.Drawing.Point(13, 12);
            this.btStart.Name = "btStart";
            this.btStart.Size = new System.Drawing.Size(107, 35);
            this.btStart.TabIndex = 0;
            this.btStart.Text = "Start 3D Visualisator";
            this.btStart.UseVisualStyleBackColor = true;
            this.btStart.Click += new System.EventHandler(this.btStart_Click);
            // 
            // bt2DRender
            // 
            this.bt2DRender.Location = new System.Drawing.Point(152, 12);
            this.bt2DRender.Name = "bt2DRender";
            this.bt2DRender.Size = new System.Drawing.Size(107, 35);
            this.bt2DRender.TabIndex = 2;
            this.bt2DRender.Text = "Start 2D Visualisator";
            this.bt2DRender.UseVisualStyleBackColor = true;
            this.bt2DRender.Click += new System.EventHandler(this.bt2DRender_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pictureBox1.Location = new System.Drawing.Point(50, 116);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(900, 300);
            this.pictureBox1.TabIndex = 13;
            this.pictureBox1.TabStop = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(140, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "Noise generation time (ms) : ";
            // 
            // lblGenerationTime
            // 
            this.lblGenerationTime.AutoSize = true;
            this.lblGenerationTime.Location = new System.Drawing.Point(158, 79);
            this.lblGenerationTime.Name = "lblGenerationTime";
            this.lblGenerationTime.Size = new System.Drawing.Size(13, 13);
            this.lblGenerationTime.TabIndex = 17;
            this.lblGenerationTime.Text = "0";
            // 
            // forward
            // 
            this.forward.Location = new System.Drawing.Point(955, 116);
            this.forward.Name = "forward";
            this.forward.Size = new System.Drawing.Size(31, 300);
            this.forward.TabIndex = 18;
            this.forward.Text = ">>";
            this.forward.UseVisualStyleBackColor = true;
            this.forward.Click += new System.EventHandler(this.forward_Click);
            // 
            // backward
            // 
            this.backward.Location = new System.Drawing.Point(8, 116);
            this.backward.Name = "backward";
            this.backward.Size = new System.Drawing.Size(31, 300);
            this.backward.TabIndex = 19;
            this.backward.Text = "<<";
            this.backward.UseVisualStyleBackColor = true;
            this.backward.Click += new System.EventHandler(this.backward_Click);
            // 
            // withThresHold
            // 
            this.withThresHold.AutoSize = true;
            this.withThresHold.Checked = true;
            this.withThresHold.CheckState = System.Windows.Forms.CheckState.Checked;
            this.withThresHold.Location = new System.Drawing.Point(6, 22);
            this.withThresHold.Name = "withThresHold";
            this.withThresHold.Size = new System.Drawing.Size(170, 17);
            this.withThresHold.TabIndex = 20;
            this.withThresHold.Text = "Use ThresHoldValue (Solid if) :";
            this.withThresHold.UseVisualStyleBackColor = true;
            // 
            // thresholdValue
            // 
            this.thresholdValue.Location = new System.Drawing.Point(76, 40);
            this.thresholdValue.Name = "thresholdValue";
            this.thresholdValue.Size = new System.Drawing.Size(100, 20);
            this.thresholdValue.TabIndex = 21;
            this.thresholdValue.Text = "0,5";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(159, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(13, 13);
            this.label4.TabIndex = 23;
            this.label4.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 59);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(144, 13);
            this.label5.TabIndex = 22;
            this.label5.Text = "Noise Initialisation time (ms) : ";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.withBelow);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.txtBelow);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.withThresHold);
            this.groupBox1.Controls.Add(this.thresholdValue);
            this.groupBox1.Location = new System.Drawing.Point(275, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(288, 98);
            this.groupBox1.TabIndex = 24;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Threshold";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(44, 13);
            this.label1.TabIndex = 22;
            this.label1.Text = "Above :";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(26, 67);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(42, 13);
            this.label2.TabIndex = 24;
            this.label2.Text = "Below :";
            // 
            // txtBelow
            // 
            this.txtBelow.Location = new System.Drawing.Point(76, 65);
            this.txtBelow.Name = "txtBelow";
            this.txtBelow.Size = new System.Drawing.Size(100, 20);
            this.txtBelow.TabIndex = 23;
            this.txtBelow.Text = "0,6";
            // 
            // withBelow
            // 
            this.withBelow.AutoSize = true;
            this.withBelow.Location = new System.Drawing.Point(182, 66);
            this.withBelow.Name = "withBelow";
            this.withBelow.Size = new System.Drawing.Size(77, 17);
            this.withBelow.TabIndex = 25;
            this.withBelow.Text = "WithBelow";
            this.withBelow.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(996, 430);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.backward);
            this.Controls.Add(this.forward);
            this.Controls.Add(this.lblGenerationTime);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.bt2DRender);
            this.Controls.Add(this.btStart);
            this.Name = "Form1";
            this.Text = "Form1";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btStart;
        private System.Windows.Forms.Button bt2DRender;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblGenerationTime;
        private System.Windows.Forms.Button forward;
        private System.Windows.Forms.Button backward;
        private System.Windows.Forms.CheckBox withThresHold;
        private System.Windows.Forms.TextBox thresholdValue;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBelow;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox withBelow;
    }
}