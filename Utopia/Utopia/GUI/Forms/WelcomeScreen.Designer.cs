namespace Utopia.GUI.Forms
{
    partial class WelcomeScreen
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
            this.btSinglePlayer = new System.Windows.Forms.Button();
            this.btMultiPlayer = new System.Windows.Forms.Button();
            this.btConfig = new System.Windows.Forms.Button();
            this.ChildContainer = new System.Windows.Forms.Panel();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Script MT Bold", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(128)))));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(212, 77);
            this.label1.TabIndex = 1;
            this.label1.Text = "Utopia";
            // 
            // btSinglePlayer
            // 
            this.btSinglePlayer.Enabled = false;
            this.btSinglePlayer.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btSinglePlayer.Location = new System.Drawing.Point(25, 151);
            this.btSinglePlayer.Name = "btSinglePlayer";
            this.btSinglePlayer.Size = new System.Drawing.Size(134, 53);
            this.btSinglePlayer.TabIndex = 2;
            this.btSinglePlayer.Text = "Single Player";
            this.btSinglePlayer.UseVisualStyleBackColor = false;
            this.btSinglePlayer.Click += new System.EventHandler(this.btSinglePlayer_Click);
            // 
            // btMultiPlayer
            // 
            this.btMultiPlayer.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.btMultiPlayer.Location = new System.Drawing.Point(25, 210);
            this.btMultiPlayer.Name = "btMultiPlayer";
            this.btMultiPlayer.Size = new System.Drawing.Size(134, 53);
            this.btMultiPlayer.TabIndex = 3;
            this.btMultiPlayer.Text = "Multiplayer";
            this.btMultiPlayer.UseVisualStyleBackColor = true;
            this.btMultiPlayer.Click += new System.EventHandler(this.btMultiPlayer_Click);
            // 
            // btConfig
            // 
            this.btConfig.Font = new System.Drawing.Font("Microsoft Sans Serif", 12.75F, System.Drawing.FontStyle.Bold);
            this.btConfig.Location = new System.Drawing.Point(25, 269);
            this.btConfig.Name = "btConfig";
            this.btConfig.Size = new System.Drawing.Size(134, 53);
            this.btConfig.TabIndex = 4;
            this.btConfig.Text = "Config.";
            this.btConfig.UseVisualStyleBackColor = true;
            this.btConfig.Click += new System.EventHandler(this.btConfig_Click);
            // 
            // ChildContainer
            // 
            this.ChildContainer.BackColor = System.Drawing.Color.Transparent;
            this.ChildContainer.Location = new System.Drawing.Point(230, 12);
            this.ChildContainer.Name = "ChildContainer";
            this.ChildContainer.Size = new System.Drawing.Size(552, 423);
            this.ChildContainer.TabIndex = 5;
            // 
            // WelcomeScreen
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::Utopia.Properties.Resources.WelcomeScreen;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(794, 447);
            this.Controls.Add(this.ChildContainer);
            this.Controls.Add(this.btConfig);
            this.Controls.Add(this.btMultiPlayer);
            this.Controls.Add(this.btSinglePlayer);
            this.Controls.Add(this.label1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "WelcomeScreen";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Welcome";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.WelcomeScreen_FormClosed);
            this.Shown += new System.EventHandler(this.WelcomeScreen_Shown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.Button btSinglePlayer;
        public System.Windows.Forms.Button btMultiPlayer;
        public System.Windows.Forms.Button btConfig;
        private System.Windows.Forms.Panel ChildContainer;

    }
}