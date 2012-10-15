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
            this.components = new System.ComponentModel.Container();
            this.label1 = new System.Windows.Forms.Label();
            this.tabUtopiaProcessor = new System.Windows.Forms.TabControl();
            this.WorldParam = new System.Windows.Forms.TabPage();
            this.OceanHeight = new System.Windows.Forms.Label();
            this.wHeight = new System.Windows.Forms.Label();
            this.trackBar2 = new System.Windows.Forms.TrackBar();
            this.label18 = new System.Windows.Forms.Label();
            this.maxHeight = new System.Windows.Forms.TrackBar();
            this.label17 = new System.Windows.Forms.Label();
            this.LandscapeParam = new System.Windows.Forms.TabPage();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.pContinent = new System.Windows.Forms.Panel();
            this.udContinentOct = new System.Windows.Forms.NumericUpDown();
            this.udContinentFreq = new System.Windows.Forms.NumericUpDown();
            this.label15 = new System.Windows.Forms.Label();
            this.label14 = new System.Windows.Forms.Label();
            this.pIsland = new System.Windows.Forms.Panel();
            this.udIslandSize = new System.Windows.Forms.NumericUpDown();
            this.label16 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.worldType = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.udGroundOct = new System.Windows.Forms.NumericUpDown();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.udGroundFeq = new System.Windows.Forms.NumericUpDown();
            this.label7 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.BasicLandscapes = new System.Windows.Forms.GroupBox();
            this.udPlainOct = new System.Windows.Forms.NumericUpDown();
            this.label11 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.udPlainFreq = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.BiomesPage = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tvBiomeList = new System.Windows.Forms.TreeView();
            this.contextMenuUtopia = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pgBiomes = new System.Windows.Forms.PropertyGrid();
            this.rangeBarWorld = new Utopia.Editor.RangeBar();
            this.rangeBarOcean = new Utopia.Editor.RangeBar();
            this.rangeBarGround = new Utopia.Editor.RangeBar();
            this.rangeBarBasicOcean = new Utopia.Editor.RangeBar();
            this.rangeBarBasicPlain = new Utopia.Editor.RangeBar();
            this.rangeBarBasicMontain = new Utopia.Editor.RangeBar();
            this.rangeBarBasicMidLand = new Utopia.Editor.RangeBar();
            this.tabUtopiaProcessor.SuspendLayout();
            this.WorldParam.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxHeight)).BeginInit();
            this.LandscapeParam.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.pContinent.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udContinentOct)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udContinentFreq)).BeginInit();
            this.pIsland.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udIslandSize)).BeginInit();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udGroundOct)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udGroundFeq)).BeginInit();
            this.BasicLandscapes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udPlainOct)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.udPlainFreq)).BeginInit();
            this.BiomesPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.contextMenuUtopia.SuspendLayout();
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
            this.tabUtopiaProcessor.Controls.Add(this.WorldParam);
            this.tabUtopiaProcessor.Controls.Add(this.LandscapeParam);
            this.tabUtopiaProcessor.Controls.Add(this.BiomesPage);
            this.tabUtopiaProcessor.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabUtopiaProcessor.Location = new System.Drawing.Point(0, 0);
            this.tabUtopiaProcessor.Name = "tabUtopiaProcessor";
            this.tabUtopiaProcessor.SelectedIndex = 0;
            this.tabUtopiaProcessor.Size = new System.Drawing.Size(672, 630);
            this.tabUtopiaProcessor.TabIndex = 13;
            // 
            // WorldParam
            // 
            this.WorldParam.Controls.Add(this.OceanHeight);
            this.WorldParam.Controls.Add(this.wHeight);
            this.WorldParam.Controls.Add(this.trackBar2);
            this.WorldParam.Controls.Add(this.label18);
            this.WorldParam.Controls.Add(this.maxHeight);
            this.WorldParam.Controls.Add(this.label17);
            this.WorldParam.Location = new System.Drawing.Point(4, 22);
            this.WorldParam.Name = "WorldParam";
            this.WorldParam.Padding = new System.Windows.Forms.Padding(3);
            this.WorldParam.Size = new System.Drawing.Size(664, 604);
            this.WorldParam.TabIndex = 2;
            this.WorldParam.Text = "Global World Parameters";
            this.WorldParam.UseVisualStyleBackColor = true;
            // 
            // OceanHeight
            // 
            this.OceanHeight.AutoSize = true;
            this.OceanHeight.Location = new System.Drawing.Point(431, 72);
            this.OceanHeight.Name = "OceanHeight";
            this.OceanHeight.Size = new System.Drawing.Size(26, 13);
            this.OceanHeight.TabIndex = 5;
            this.OceanHeight.Text = "XxX";
            // 
            // wHeight
            // 
            this.wHeight.AutoSize = true;
            this.wHeight.Location = new System.Drawing.Point(431, 18);
            this.wHeight.Name = "wHeight";
            this.wHeight.Size = new System.Drawing.Size(24, 13);
            this.wHeight.TabIndex = 4;
            this.wHeight.Text = "xXx";
            // 
            // trackBar2
            // 
            this.trackBar2.BackColor = System.Drawing.Color.White;
            this.trackBar2.Location = new System.Drawing.Point(120, 72);
            this.trackBar2.Maximum = 256;
            this.trackBar2.Minimum = 1;
            this.trackBar2.Name = "trackBar2";
            this.trackBar2.Size = new System.Drawing.Size(304, 45);
            this.trackBar2.TabIndex = 3;
            this.trackBar2.TickFrequency = 10;
            this.trackBar2.Value = 64;
            this.trackBar2.ValueChanged += new System.EventHandler(this.trackBar2_ValueChanged);
            // 
            // label18
            // 
            this.label18.AutoSize = true;
            this.label18.Location = new System.Drawing.Point(16, 72);
            this.label18.Name = "label18";
            this.label18.Size = new System.Drawing.Size(101, 13);
            this.label18.TabIndex = 2;
            this.label18.Text = "World Ocean level :";
            // 
            // maxHeight
            // 
            this.maxHeight.BackColor = System.Drawing.Color.White;
            this.maxHeight.LargeChange = 16;
            this.maxHeight.Location = new System.Drawing.Point(120, 18);
            this.maxHeight.Maximum = 256;
            this.maxHeight.Minimum = 128;
            this.maxHeight.Name = "maxHeight";
            this.maxHeight.Size = new System.Drawing.Size(304, 45);
            this.maxHeight.TabIndex = 1;
            this.maxHeight.TickFrequency = 16;
            this.maxHeight.Value = 128;
            this.maxHeight.ValueChanged += new System.EventHandler(this.maxHeight_ValueChanged);
            // 
            // label17
            // 
            this.label17.AutoSize = true;
            this.label17.Location = new System.Drawing.Point(16, 18);
            this.label17.Name = "label17";
            this.label17.Size = new System.Drawing.Size(98, 13);
            this.label17.TabIndex = 0;
            this.label17.Text = "World Max Height :";
            // 
            // LandscapeParam
            // 
            this.LandscapeParam.Controls.Add(this.groupBox2);
            this.LandscapeParam.Controls.Add(this.groupBox1);
            this.LandscapeParam.Controls.Add(this.BasicLandscapes);
            this.LandscapeParam.Location = new System.Drawing.Point(4, 22);
            this.LandscapeParam.Name = "LandscapeParam";
            this.LandscapeParam.Padding = new System.Windows.Forms.Padding(3);
            this.LandscapeParam.Size = new System.Drawing.Size(664, 604);
            this.LandscapeParam.TabIndex = 1;
            this.LandscapeParam.Text = "Landscape";
            this.LandscapeParam.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.pContinent);
            this.groupBox2.Controls.Add(this.pIsland);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.worldType);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.rangeBarWorld);
            this.groupBox2.Location = new System.Drawing.Point(14, 462);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(636, 136);
            this.groupBox2.TabIndex = 15;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "World";
            // 
            // pContinent
            // 
            this.pContinent.Controls.Add(this.udContinentOct);
            this.pContinent.Controls.Add(this.udContinentFreq);
            this.pContinent.Controls.Add(this.label15);
            this.pContinent.Controls.Add(this.label14);
            this.pContinent.Location = new System.Drawing.Point(0, 58);
            this.pContinent.Name = "pContinent";
            this.pContinent.Size = new System.Drawing.Size(110, 56);
            this.pContinent.TabIndex = 25;
            // 
            // udContinentOct
            // 
            this.udContinentOct.Location = new System.Drawing.Point(46, 33);
            this.udContinentOct.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.udContinentOct.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udContinentOct.Name = "udContinentOct";
            this.udContinentOct.Size = new System.Drawing.Size(61, 20);
            this.udContinentOct.TabIndex = 21;
            this.udContinentOct.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udContinentOct.ValueChanged += new System.EventHandler(this.udContinentOct_ValueChanged);
            // 
            // udContinentFreq
            // 
            this.udContinentFreq.DecimalPlaces = 2;
            this.udContinentFreq.Location = new System.Drawing.Point(46, 7);
            this.udContinentFreq.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.udContinentFreq.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.udContinentFreq.Name = "udContinentFreq";
            this.udContinentFreq.Size = new System.Drawing.Size(61, 20);
            this.udContinentFreq.TabIndex = 18;
            this.udContinentFreq.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udContinentFreq.ValueChanged += new System.EventHandler(this.udContinentFreq_ValueChanged);
            // 
            // label15
            // 
            this.label15.AutoSize = true;
            this.label15.Location = new System.Drawing.Point(6, 9);
            this.label15.Name = "label15";
            this.label15.Size = new System.Drawing.Size(34, 13);
            this.label15.TabIndex = 19;
            this.label15.Text = "Freq :";
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Location = new System.Drawing.Point(6, 35);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(30, 13);
            this.label14.TabIndex = 20;
            this.label14.Text = "Oct :";
            // 
            // pIsland
            // 
            this.pIsland.Controls.Add(this.udIslandSize);
            this.pIsland.Controls.Add(this.label16);
            this.pIsland.Location = new System.Drawing.Point(242, 13);
            this.pIsland.Name = "pIsland";
            this.pIsland.Size = new System.Drawing.Size(116, 32);
            this.pIsland.TabIndex = 24;
            // 
            // udIslandSize
            // 
            this.udIslandSize.DecimalPlaces = 4;
            this.udIslandSize.Location = new System.Drawing.Point(44, 6);
            this.udIslandSize.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.udIslandSize.Name = "udIslandSize";
            this.udIslandSize.Size = new System.Drawing.Size(61, 20);
            this.udIslandSize.TabIndex = 22;
            this.udIslandSize.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udIslandSize.ValueChanged += new System.EventHandler(this.udIslandSize_ValueChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Location = new System.Drawing.Point(5, 9);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(33, 13);
            this.label16.TabIndex = 23;
            this.label16.Text = "Size :";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(8, 16);
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
            this.worldType.Location = new System.Drawing.Point(115, 13);
            this.worldType.Name = "worldType";
            this.worldType.Size = new System.Drawing.Size(121, 21);
            this.worldType.TabIndex = 10;
            this.worldType.SelectedIndexChanged += new System.EventHandler(this.worldType_SelectedIndexChanged_1);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 42);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(41, 13);
            this.label8.TabIndex = 9;
            this.label8.Text = "World :";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.udGroundOct);
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.label13);
            this.groupBox1.Controls.Add(this.udGroundFeq);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.rangeBarOcean);
            this.groupBox1.Controls.Add(this.rangeBarGround);
            this.groupBox1.Location = new System.Drawing.Point(14, 297);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(636, 157);
            this.groupBox1.TabIndex = 14;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Ground and Ocean";
            // 
            // udGroundOct
            // 
            this.udGroundOct.Location = new System.Drawing.Point(48, 59);
            this.udGroundOct.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.udGroundOct.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udGroundOct.Name = "udGroundOct";
            this.udGroundOct.Size = new System.Drawing.Size(61, 20);
            this.udGroundOct.TabIndex = 17;
            this.udGroundOct.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udGroundOct.ValueChanged += new System.EventHandler(this.udGroundOct_ValueChanged);
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(12, 61);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(30, 13);
            this.label12.TabIndex = 16;
            this.label12.Text = "Oct :";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(8, 35);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(34, 13);
            this.label13.TabIndex = 15;
            this.label13.Text = "Freq :";
            // 
            // udGroundFeq
            // 
            this.udGroundFeq.DecimalPlaces = 2;
            this.udGroundFeq.Location = new System.Drawing.Point(48, 33);
            this.udGroundFeq.Maximum = new decimal(new int[] {
            4,
            0,
            0,
            0});
            this.udGroundFeq.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.udGroundFeq.Name = "udGroundFeq";
            this.udGroundFeq.Size = new System.Drawing.Size(61, 20);
            this.udGroundFeq.TabIndex = 14;
            this.udGroundFeq.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udGroundFeq.ValueChanged += new System.EventHandler(this.udGroundFeq_ValueChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(8, 102);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(45, 13);
            this.label7.TabIndex = 9;
            this.label7.Text = "Ocean :";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 16);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(48, 13);
            this.label6.TabIndex = 8;
            this.label6.Text = "Ground :";
            // 
            // BasicLandscapes
            // 
            this.BasicLandscapes.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BasicLandscapes.Controls.Add(this.udPlainOct);
            this.BasicLandscapes.Controls.Add(this.label11);
            this.BasicLandscapes.Controls.Add(this.label10);
            this.BasicLandscapes.Controls.Add(this.udPlainFreq);
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
            this.BasicLandscapes.Size = new System.Drawing.Size(636, 274);
            this.BasicLandscapes.TabIndex = 13;
            this.BasicLandscapes.TabStop = false;
            this.BasicLandscapes.Text = "Basic landscape forms";
            // 
            // udPlainOct
            // 
            this.udPlainOct.Location = new System.Drawing.Point(48, 58);
            this.udPlainOct.Maximum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.udPlainOct.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udPlainOct.Name = "udPlainOct";
            this.udPlainOct.Size = new System.Drawing.Size(61, 20);
            this.udPlainOct.TabIndex = 13;
            this.udPlainOct.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udPlainOct.ValueChanged += new System.EventHandler(this.udPlainOct_ValueChanged);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(8, 60);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(30, 13);
            this.label11.TabIndex = 12;
            this.label11.Text = "Oct :";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 34);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(34, 13);
            this.label10.TabIndex = 11;
            this.label10.Text = "Freq :";
            // 
            // udPlainFreq
            // 
            this.udPlainFreq.DecimalPlaces = 2;
            this.udPlainFreq.Increment = new decimal(new int[] {
            1,
            0,
            0,
            65536});
            this.udPlainFreq.Location = new System.Drawing.Point(48, 32);
            this.udPlainFreq.Maximum = new decimal(new int[] {
            40,
            0,
            0,
            65536});
            this.udPlainFreq.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            65536});
            this.udPlainFreq.Name = "udPlainFreq";
            this.udPlainFreq.Size = new System.Drawing.Size(61, 20);
            this.udPlainFreq.TabIndex = 10;
            this.udPlainFreq.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.udPlainFreq.ValueChanged += new System.EventHandler(this.udPlainFreq_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(93, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Plains landscape :";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 211);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(97, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Ocean landscape :";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(102, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Midland landscape :";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 155);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(103, 13);
            this.label4.TabIndex = 6;
            this.label4.Text = "Montain landscape :";
            // 
            // BiomesPage
            // 
            this.BiomesPage.Controls.Add(this.splitContainer1);
            this.BiomesPage.Location = new System.Drawing.Point(4, 22);
            this.BiomesPage.Name = "BiomesPage";
            this.BiomesPage.Padding = new System.Windows.Forms.Padding(3);
            this.BiomesPage.Size = new System.Drawing.Size(664, 604);
            this.BiomesPage.TabIndex = 0;
            this.BiomesPage.Text = "Biomes";
            this.BiomesPage.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvBiomeList);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pgBiomes);
            this.splitContainer1.Size = new System.Drawing.Size(658, 598);
            this.splitContainer1.SplitterDistance = 189;
            this.splitContainer1.TabIndex = 0;
            // 
            // tvBiomeList
            // 
            this.tvBiomeList.ContextMenuStrip = this.contextMenuUtopia;
            this.tvBiomeList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvBiomeList.Location = new System.Drawing.Point(0, 0);
            this.tvBiomeList.Name = "tvBiomeList";
            this.tvBiomeList.Size = new System.Drawing.Size(189, 598);
            this.tvBiomeList.TabIndex = 0;
            this.tvBiomeList.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvBiomeList_AfterSelect);
            // 
            // contextMenuUtopia
            // 
            this.contextMenuUtopia.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.contextMenuUtopia.Name = "contextMenuStrip1";
            this.contextMenuUtopia.Size = new System.Drawing.Size(118, 48);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // pgBiomes
            // 
            this.pgBiomes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgBiomes.Location = new System.Drawing.Point(0, 0);
            this.pgBiomes.Name = "pgBiomes";
            this.pgBiomes.Size = new System.Drawing.Size(465, 598);
            this.pgBiomes.TabIndex = 0;
            this.pgBiomes.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgBiomes_PropertyValueChanged);
            // 
            // rangeBarWorld
            // 
            this.rangeBarWorld.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarWorld.Location = new System.Drawing.Point(113, 51);
            this.rangeBarWorld.Name = "rangeBarWorld";
            this.rangeBarWorld.Size = new System.Drawing.Size(504, 70);
            this.rangeBarWorld.TabIndex = 0;
            // 
            // rangeBarOcean
            // 
            this.rangeBarOcean.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarOcean.Location = new System.Drawing.Point(115, 89);
            this.rangeBarOcean.Name = "rangeBarOcean";
            this.rangeBarOcean.Size = new System.Drawing.Size(504, 70);
            this.rangeBarOcean.TabIndex = 1;
            // 
            // rangeBarGround
            // 
            this.rangeBarGround.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarGround.Location = new System.Drawing.Point(115, 19);
            this.rangeBarGround.Name = "rangeBarGround";
            this.rangeBarGround.Size = new System.Drawing.Size(504, 70);
            this.rangeBarGround.TabIndex = 0;
            // 
            // rangeBarBasicOcean
            // 
            this.rangeBarBasicOcean.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarBasicOcean.Location = new System.Drawing.Point(115, 198);
            this.rangeBarBasicOcean.Name = "rangeBarBasicOcean";
            this.rangeBarBasicOcean.Size = new System.Drawing.Size(504, 70);
            this.rangeBarBasicOcean.TabIndex = 9;
            // 
            // rangeBarBasicPlain
            // 
            this.rangeBarBasicPlain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarBasicPlain.Location = new System.Drawing.Point(115, 16);
            this.rangeBarBasicPlain.Name = "rangeBarBasicPlain";
            this.rangeBarBasicPlain.Size = new System.Drawing.Size(504, 65);
            this.rangeBarBasicPlain.TabIndex = 3;
            // 
            // rangeBarBasicMontain
            // 
            this.rangeBarBasicMontain.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarBasicMontain.Location = new System.Drawing.Point(115, 141);
            this.rangeBarBasicMontain.Name = "rangeBarBasicMontain";
            this.rangeBarBasicMontain.Size = new System.Drawing.Size(504, 70);
            this.rangeBarBasicMontain.TabIndex = 8;
            // 
            // rangeBarBasicMidLand
            // 
            this.rangeBarBasicMidLand.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rangeBarBasicMidLand.Location = new System.Drawing.Point(115, 87);
            this.rangeBarBasicMidLand.Name = "rangeBarBasicMidLand";
            this.rangeBarBasicMidLand.Size = new System.Drawing.Size(504, 70);
            this.rangeBarBasicMidLand.TabIndex = 4;
            // 
            // FrmUtopiaProcessorConfig
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tabUtopiaProcessor);
            this.Controls.Add(this.label1);
            this.Name = "FrmUtopiaProcessorConfig";
            this.Size = new System.Drawing.Size(672, 630);
            this.tabUtopiaProcessor.ResumeLayout(false);
            this.WorldParam.ResumeLayout(false);
            this.WorldParam.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.trackBar2)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.maxHeight)).EndInit();
            this.LandscapeParam.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.pContinent.ResumeLayout(false);
            this.pContinent.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udContinentOct)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udContinentFreq)).EndInit();
            this.pIsland.ResumeLayout(false);
            this.pIsland.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udIslandSize)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udGroundOct)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udGroundFeq)).EndInit();
            this.BasicLandscapes.ResumeLayout(false);
            this.BasicLandscapes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.udPlainOct)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.udPlainFreq)).EndInit();
            this.BiomesPage.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.contextMenuUtopia.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabControl tabUtopiaProcessor;
        private System.Windows.Forms.TabPage BiomesPage;
        private System.Windows.Forms.TabPage LandscapeParam;
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
        public System.Windows.Forms.PropertyGrid pgBiomes;
        private System.Windows.Forms.ContextMenuStrip contextMenuUtopia;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.NumericUpDown udPlainFreq;
        private System.Windows.Forms.NumericUpDown udPlainOct;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.NumericUpDown udGroundOct;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.NumericUpDown udGroundFeq;
        private System.Windows.Forms.NumericUpDown udContinentOct;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.Label label15;
        private System.Windows.Forms.NumericUpDown udContinentFreq;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.NumericUpDown udIslandSize;
        private System.Windows.Forms.Panel pContinent;
        private System.Windows.Forms.Panel pIsland;
        private System.Windows.Forms.TabPage WorldParam;
        private System.Windows.Forms.TrackBar maxHeight;
        private System.Windows.Forms.Label label17;
        private System.Windows.Forms.Label label18;
        private System.Windows.Forms.TrackBar trackBar2;
        private System.Windows.Forms.Label OceanHeight;
        private System.Windows.Forms.Label wHeight;

    }
}
