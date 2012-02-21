namespace Sandbox.Client.GUI.Forms.CustControls
{
    partial class Config
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
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.GameP = new System.Windows.Forms.TabPage();
            this.dtGridGameParam = new System.Windows.Forms.DataGridView();
            this.GraphP = new System.Windows.Forms.TabPage();
            this.dtGridGraphParam = new System.Windows.Forms.DataGridView();
            this.KeybP = new System.Windows.Forms.TabPage();
            this.tabControl2 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.dtGridKeyboard = new System.Windows.Forms.DataGridView();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dtGridKeyboardMove = new System.Windows.Forms.DataGridView();
            this.btDefaultQWERTY = new System.Windows.Forms.Button();
            this.btDefaultAZERTY = new System.Windows.Forms.Button();
            this.btSaveConfig = new System.Windows.Forms.Button();
            this.btCancel = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.GameP.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtGridGameParam)).BeginInit();
            this.GraphP.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtGridGraphParam)).BeginInit();
            this.KeybP.SuspendLayout();
            this.tabControl2.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtGridKeyboard)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dtGridKeyboardMove)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.BackColor = System.Drawing.Color.Transparent;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, ((System.Drawing.FontStyle)((System.Drawing.FontStyle.Italic | System.Drawing.FontStyle.Underline))), System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Maroon;
            this.label1.Location = new System.Drawing.Point(13, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(183, 25);
            this.label1.TabIndex = 2;
            this.label1.Text = "Client Configuration";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.GameP);
            this.tabControl1.Controls.Add(this.GraphP);
            this.tabControl1.Controls.Add(this.KeybP);
            this.tabControl1.Location = new System.Drawing.Point(3, 40);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(546, 341);
            this.tabControl1.TabIndex = 3;
            // 
            // GameP
            // 
            this.GameP.Controls.Add(this.dtGridGameParam);
            this.GameP.Location = new System.Drawing.Point(4, 22);
            this.GameP.Name = "GameP";
            this.GameP.Padding = new System.Windows.Forms.Padding(3);
            this.GameP.Size = new System.Drawing.Size(538, 315);
            this.GameP.TabIndex = 2;
            this.GameP.Text = "Game parameters";
            this.GameP.UseVisualStyleBackColor = true;
            // 
            // dtGridGameParam
            // 
            this.dtGridGameParam.AllowUserToAddRows = false;
            this.dtGridGameParam.AllowUserToDeleteRows = false;
            this.dtGridGameParam.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dtGridGameParam.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtGridGameParam.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtGridGameParam.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dtGridGameParam.Location = new System.Drawing.Point(3, 3);
            this.dtGridGameParam.MultiSelect = false;
            this.dtGridGameParam.Name = "dtGridGameParam";
            this.dtGridGameParam.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dtGridGameParam.Size = new System.Drawing.Size(532, 309);
            this.dtGridGameParam.TabIndex = 2;
            // 
            // GraphP
            // 
            this.GraphP.Controls.Add(this.dtGridGraphParam);
            this.GraphP.Location = new System.Drawing.Point(4, 22);
            this.GraphP.Name = "GraphP";
            this.GraphP.Padding = new System.Windows.Forms.Padding(3);
            this.GraphP.Size = new System.Drawing.Size(538, 315);
            this.GraphP.TabIndex = 1;
            this.GraphP.Text = "Graphic parameters";
            this.GraphP.UseVisualStyleBackColor = true;
            // 
            // dtGridGraphParam
            // 
            this.dtGridGraphParam.AllowUserToAddRows = false;
            this.dtGridGraphParam.AllowUserToDeleteRows = false;
            this.dtGridGraphParam.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dtGridGraphParam.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtGridGraphParam.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtGridGraphParam.EditMode = System.Windows.Forms.DataGridViewEditMode.EditOnEnter;
            this.dtGridGraphParam.Location = new System.Drawing.Point(3, 3);
            this.dtGridGraphParam.MultiSelect = false;
            this.dtGridGraphParam.Name = "dtGridGraphParam";
            this.dtGridGraphParam.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dtGridGraphParam.Size = new System.Drawing.Size(532, 309);
            this.dtGridGraphParam.TabIndex = 2;
            // 
            // KeybP
            // 
            this.KeybP.Controls.Add(this.tabControl2);
            this.KeybP.Controls.Add(this.btDefaultQWERTY);
            this.KeybP.Controls.Add(this.btDefaultAZERTY);
            this.KeybP.Location = new System.Drawing.Point(4, 22);
            this.KeybP.Name = "KeybP";
            this.KeybP.Padding = new System.Windows.Forms.Padding(3);
            this.KeybP.Size = new System.Drawing.Size(538, 315);
            this.KeybP.TabIndex = 0;
            this.KeybP.Text = "Keyboard Mapping";
            this.KeybP.UseVisualStyleBackColor = true;
            // 
            // tabControl2
            // 
            this.tabControl2.Controls.Add(this.tabPage1);
            this.tabControl2.Controls.Add(this.tabPage2);
            this.tabControl2.Location = new System.Drawing.Point(6, 56);
            this.tabControl2.Name = "tabControl2";
            this.tabControl2.SelectedIndex = 0;
            this.tabControl2.Size = new System.Drawing.Size(526, 253);
            this.tabControl2.TabIndex = 3;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.dtGridKeyboard);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(518, 227);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Various";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // dtGridKeyboard
            // 
            this.dtGridKeyboard.AllowUserToAddRows = false;
            this.dtGridKeyboard.AllowUserToDeleteRows = false;
            this.dtGridKeyboard.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dtGridKeyboard.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtGridKeyboard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtGridKeyboard.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dtGridKeyboard.Location = new System.Drawing.Point(3, 3);
            this.dtGridKeyboard.MultiSelect = false;
            this.dtGridKeyboard.Name = "dtGridKeyboard";
            this.dtGridKeyboard.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dtGridKeyboard.Size = new System.Drawing.Size(512, 221);
            this.dtGridKeyboard.TabIndex = 1;
            this.dtGridKeyboard.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dtGridKeyboard_KeyDown);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dtGridKeyboardMove);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(518, 227);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Move";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dtGridKeyboardMove
            // 
            this.dtGridKeyboardMove.AllowUserToAddRows = false;
            this.dtGridKeyboardMove.AllowUserToDeleteRows = false;
            this.dtGridKeyboardMove.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.dtGridKeyboardMove.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dtGridKeyboardMove.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dtGridKeyboardMove.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
            this.dtGridKeyboardMove.Location = new System.Drawing.Point(3, 3);
            this.dtGridKeyboardMove.MultiSelect = false;
            this.dtGridKeyboardMove.Name = "dtGridKeyboardMove";
            this.dtGridKeyboardMove.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dtGridKeyboardMove.Size = new System.Drawing.Size(512, 221);
            this.dtGridKeyboardMove.TabIndex = 0;
            this.dtGridKeyboardMove.KeyDown += new System.Windows.Forms.KeyEventHandler(this.dtGridKeyboard_KeyDown);
            // 
            // btDefaultQWERTY
            // 
            this.btDefaultQWERTY.Location = new System.Drawing.Point(6, 7);
            this.btDefaultQWERTY.Name = "btDefaultQWERTY";
            this.btDefaultQWERTY.Size = new System.Drawing.Size(104, 43);
            this.btDefaultQWERTY.TabIndex = 2;
            this.btDefaultQWERTY.Text = "Set to default Qwerty";
            this.btDefaultQWERTY.UseVisualStyleBackColor = true;
            this.btDefaultQWERTY.Click += new System.EventHandler(this.btDefaultQWERTY_Click);
            // 
            // btDefaultAZERTY
            // 
            this.btDefaultAZERTY.Location = new System.Drawing.Point(116, 7);
            this.btDefaultAZERTY.Name = "btDefaultAZERTY";
            this.btDefaultAZERTY.Size = new System.Drawing.Size(104, 43);
            this.btDefaultAZERTY.TabIndex = 1;
            this.btDefaultAZERTY.Text = "Set to default Azerty";
            this.btDefaultAZERTY.UseVisualStyleBackColor = true;
            this.btDefaultAZERTY.Click += new System.EventHandler(this.btDefaultAZERTY_Click);
            // 
            // btSaveConfig
            // 
            this.btSaveConfig.Location = new System.Drawing.Point(424, 387);
            this.btSaveConfig.Name = "btSaveConfig";
            this.btSaveConfig.Size = new System.Drawing.Size(104, 33);
            this.btSaveConfig.TabIndex = 4;
            this.btSaveConfig.Text = "Save changes";
            this.btSaveConfig.UseVisualStyleBackColor = true;
            this.btSaveConfig.Click += new System.EventHandler(this.btSaveConfig_Click);
            // 
            // btCancel
            // 
            this.btCancel.Location = new System.Drawing.Point(305, 387);
            this.btCancel.Name = "btCancel";
            this.btCancel.Size = new System.Drawing.Size(104, 33);
            this.btCancel.TabIndex = 5;
            this.btCancel.Text = "Cancel changes";
            this.btCancel.UseVisualStyleBackColor = true;
            this.btCancel.Click += new System.EventHandler(this.btCancel_Click);
            // 
            // Config
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btCancel);
            this.Controls.Add(this.btSaveConfig);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.label1);
            this.Name = "Config";
            this.Size = new System.Drawing.Size(552, 423);
            this.tabControl1.ResumeLayout(false);
            this.GameP.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtGridGameParam)).EndInit();
            this.GraphP.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtGridGraphParam)).EndInit();
            this.KeybP.ResumeLayout(false);
            this.tabControl2.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtGridKeyboard)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dtGridKeyboardMove)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage GameP;
        private System.Windows.Forms.TabPage GraphP;
        private System.Windows.Forms.TabPage KeybP;
        private System.Windows.Forms.Button btDefaultQWERTY;
        private System.Windows.Forms.Button btDefaultAZERTY;
        private System.Windows.Forms.DataGridView dtGridKeyboardMove;
        private System.Windows.Forms.Button btSaveConfig;
        private System.Windows.Forms.Button btCancel;
        private System.Windows.Forms.TabControl tabControl2;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.DataGridView dtGridKeyboard;
        private System.Windows.Forms.DataGridView dtGridGameParam;
        private System.Windows.Forms.DataGridView dtGridGraphParam;
    }
}
