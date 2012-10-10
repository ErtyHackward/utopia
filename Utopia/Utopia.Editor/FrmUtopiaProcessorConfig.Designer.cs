namespace Utopia.Editor
{
    partial class FrmUtopiaProcessorConfig
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
            this.tabUtopiaProcessor = new System.Windows.Forms.TabControl();
            this.BiomesPage = new System.Windows.Forms.TabPage();
            this.splitContainerBiomes = new System.Windows.Forms.SplitContainer();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvBiomeList = new System.Windows.Forms.TreeView();
            this.pgBiomes = new System.Windows.Forms.PropertyGrid();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.worldType = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.rangeBarWorld = new Utopia.Editor.RangeBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.rangeBarOcean = new Utopia.Editor.RangeBar();
            this.rangeBarGround = new Utopia.Editor.RangeBar();
            this.BasicLandscapes = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.rangeBarBasicOcean = new Utopia.Editor.RangeBar();
            this.rangeBarBasicPlain = new Utopia.Editor.RangeBar();
            this.rangeBarBasicMontain = new Utopia.Editor.RangeBar();
            this.rangeBarBasicMidLand = new Utopia.Editor.RangeBar();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.tabUtopiaProcessor.SuspendLayout();
            this.BiomesPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBiomes)).BeginInit();
            this.splitContainerBiomes.Panel1.SuspendLayout();
            this.splitContainerBiomes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.BasicLandscapes.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(20, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(310, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Utopia landscape generator configuration";
            // 
            // tabUtopiaProcessor
            // 
            this.tabUtopiaProcessor.Controls.Add(this.BiomesPage);
            this.tabUtopiaProcessor.Controls.Add(this.tabPage2);
            this.tabUtopiaProcessor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabUtopiaProcessor.Location = new System.Drawing.Point(0, 0);
            this.tabUtopiaProcessor.Name = "tabUtopiaProcessor";
            this.tabUtopiaProcessor.SelectedIndex = 0;
            this.tabUtopiaProcessor.Size = new System.Drawing.Size(648, 610);
            this.tabUtopiaProcessor.TabIndex = 13;
            // 
            // BiomesPage
            // 
            this.BiomesPage.Controls.Add(this.splitContainerBiomes);
            this.BiomesPage.Location = new System.Drawing.Point(4, 22);
            this.BiomesPage.Name = "BiomesPage";
            this.BiomesPage.Padding = new System.Windows.Forms.Padding(3);
            this.BiomesPage.Size = new System.Drawing.Size(640, 584);
            this.BiomesPage.TabIndex = 0;
            this.BiomesPage.Text = "Biomes";
            this.BiomesPage.UseVisualStyleBackColor = true;
            // 
            // splitContainerBiomes
            // 
            this.splitContainerBiomes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainerBiomes.Location = new System.Drawing.Point(3, 3);
            this.splitContainerBiomes.Name = "splitContainerBiomes";
            this.splitContainerBiomes.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainerBiomes.Panel1
            // 
            this.splitContainerBiomes.Panel1.Controls.Add(this.splitContainer1);
            this.splitContainerBiomes.Size = new System.Drawing.Size(634, 578);
            this.splitContainerBiomes.SplitterDistance = 361;
            this.splitContainerBiomes.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvBiomeList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pgBiomes);
            this.splitContainer1.Size = new System.Drawing.Size(634, 361);
            this.splitContainer1.SplitterDistance = 183;
            this.splitContainer1.TabIndex = 0;
            // 
            // tvBiomeList
            // 
            this.tvBiomeList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvBiomeList.Location = new System.Drawing.Point(0, 0);
            this.tvBiomeList.Name = "tvBiomeList";
            this.tvBiomeList.Size = new System.Drawing.Size(183, 361);
            this.tvBiomeList.TabIndex = 0;
            this.tvBiomeList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvBiomeList_AfterSelect);
            // 
            // pgBiomes
            // 
            this.pgBiomes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgBiomes.Location = new System.Drawing.Point(0, 0);
            this.pgBiomes.Name = "pgBiomes";
            this.pgBiomes.Size = new System.Drawing.Size(447, 361);
            this.pgBiomes.TabIndex = 0;
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.groupBox2);
            this.tabPage2.Controls.Add(this.groupBox1);
            this.tabPage2.Controls.Add(this.BasicLandscapes);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(640, 584);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Landscape";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.worldType);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.rangeBarWorld);
            this.groupBox2.Location = new System.Drawing.Point(14, 453);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(612, 115);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "World";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 84);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(64, 13);
            this.label9.TabIndex = 11;
            this.label9.Text = "World type :";
            // 
            // worldType
            // 
            this.worldType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.worldType.FormattingEnabled = true;
            this.worldType.Items.AddRange(new object[] {
            "Island",
            "Continent"});
            this.worldType.Location = new System.Drawing.Point(115, 81);
            this.worldType.Name = "worldType";
            this.worldType.Size = new System.Drawing.Size(121, 21);
            this.worldType.TabIndex = 10;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(8, 19);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "World :";
            // 
            // rangeBarWorld
            // 
            this.rangeBarWorld.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarWorld.Location = new System.Drawing.Point(115, 19);
            this.rangeBarWorld.Name = "rangeBarWorld";
            this.rangeBarWorld.Size = new System.Drawing.Size(480, 70);
            this.rangeBarWorld.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.rangeBarOcean);
            this.groupBox1.Controls.Add(this.rangeBarGround);
            this.groupBox1.Location = new System.Drawing.Point(14, 297);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(612, 149);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ground and Ocean";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 76);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Ocean :";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 28);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Ground :";
            // 
            // rangeBarOcean
            // 
            this.rangeBarOcean.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarOcean.Location = new System.Drawing.Point(115, 73);
            this.rangeBarOcean.Name = "rangeBarOcean";
            this.rangeBarOcean.Size = new System.Drawing.Size(480, 70);
            this.rangeBarOcean.TabIndex = 1;
            // 
            // rangeBarGround
            // 
            this.rangeBarGround.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarGround.Location = new System.Drawing.Point(115, 19);
            this.rangeBarGround.Name = "rangeBarGround";
            this.rangeBarGround.Size = new System.Drawing.Size(480, 70);
            this.rangeBarGround.TabIndex = 0;
            // 
            // BasicLandscapes
            // 
            this.BasicLandscapes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BasicLandscapes.Controls.Add(this.label2);
            this.BasicLandscapes.Controls.Add(this.rangeBarBasicOcean);
            this.BasicLandscapes.Controls.Add(this.rangeBarBasicPlain);
            this.BasicLandscapes.Controls.Add(this.rangeBarBasicMontain);
            this.BasicLandscapes.Controls.Add(this.rangeBarBasicMidLand);
            this.BasicLandscapes.Controls.Add(this.label5);
            this.BasicLandscapes.Controls.Add(this.label3);
            this.BasicLandscapes.Controls.Add(this.label4);
            this.BasicLandscapes.Location = new System.Drawing.Point(14, 17);
            this.BasicLandscapes.Name = "BasicLandscapes";
            this.BasicLandscapes.Size = new System.Drawing.Size(612, 274);
            this.BasicLandscapes.TabIndex = 13;
            this.BasicLandscapes.TabStop = false;
            this.BasicLandscapes.Text = "Basic landscape forms";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 30);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Plains landscape :";
            // 
            // rangeBarBasicOcean
            // 
            this.rangeBarBasicOcean.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarBasicOcean.Location = new System.Drawing.Point(115, 199);
            this.rangeBarBasicOcean.Name = "rangeBarBasicOcean";
            this.rangeBarBasicOcean.Size = new System.Drawing.Size(480, 70);
            this.rangeBarBasicOcean.TabIndex = 9;
            // 
            // rangeBarBasicPlain
            // 
            this.rangeBarBasicPlain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarBasicPlain.Location = new System.Drawing.Point(115, 30);
            this.rangeBarBasicPlain.Name = "rangeBarBasicPlain";
            this.rangeBarBasicPlain.Size = new System.Drawing.Size(480, 70);
            this.rangeBarBasicPlain.TabIndex = 3;
            // 
            // rangeBarBasicMontain
            // 
            this.rangeBarBasicMontain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarBasicMontain.Location = new System.Drawing.Point(115, 141);
            this.rangeBarBasicMontain.Name = "rangeBarBasicMontain";
            this.rangeBarBasicMontain.Size = new System.Drawing.Size(480, 70);
            this.rangeBarBasicMontain.TabIndex = 8;
            // 
            // rangeBarBasicMidLand
            // 
            this.rangeBarBasicMidLand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarBasicMidLand.Location = new System.Drawing.Point(115, 83);
            this.rangeBarBasicMidLand.Name = "rangeBarBasicMidLand";
            this.rangeBarBasicMidLand.Size = new System.Drawing.Size(480, 70);
            this.rangeBarBasicMidLand.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 199);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Ocean landscape :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 87);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Midland landscape :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 141);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(103, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Montain landscape :";
            // 
            // FrmUtopiaProcessorConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabUtopiaProcessor);
            this.Controls.Add(this.label1);
            this.Name = "FrmUtopiaProcessorConfig";
            this.Size = new System.Drawing.Size(648, 610);
            this.tabUtopiaProcessor.ResumeLayout(false);
            this.BiomesPage.ResumeLayout(false);
            this.splitContainerBiomes.Panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainerBiomes)).EndInit();
            this.splitContainerBiomes.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.BasicLandscapes.ResumeLayout(false);
            this.BasicLandscapes.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabUtopiaProcessor;
        private System.Windows.Forms.TabPage BiomesPage;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ComboBox worldType;
        private System.Windows.Forms.Label label8;
        private RangeBar rangeBarWorld;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label6;
        private RangeBar rangeBarOcean;
        private RangeBar rangeBarGround;
        private System.Windows.Forms.GroupBox BasicLandscapes;
        private System.Windows.Forms.Label label2;
        private RangeBar rangeBarBasicOcean;
        private RangeBar rangeBarBasicPlain;
        private RangeBar rangeBarBasicMontain;
        private RangeBar rangeBarBasicMidLand;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TreeView tvBiomeList;
        public System.Windows.Forms.SplitContainer splitContainer1;
        public System.Windows.Forms.SplitContainer splitContainerBiomes;
        public System.Windows.Forms.PropertyGrid pgBiomes;

    }
}
