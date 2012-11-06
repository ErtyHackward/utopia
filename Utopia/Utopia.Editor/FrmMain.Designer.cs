namespace Utopia.Editor
{
    partial class FrmMain
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
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("General", 1, 1);
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Entities", 2, 2);
            System.Windows.Forms.TreeNode treeNode3 = new System.Windows.Forms.TreeNode("Cubes", 4, 4);
            System.Windows.Forms.TreeNode treeNode4 = new System.Windows.Forms.TreeNode("WorldProcessor Params", 5, 5);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMain));
            this.contextMenuCategories = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pgDetails = new System.Windows.Forms.PropertyGrid();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newRealmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openRealmToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveAsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.officialSiteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.tvMainCategories = new System.Windows.Forms.TreeView();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.contextMenuCategories.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenuCategories
            // 
            this.contextMenuCategories.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem});
            this.contextMenuCategories.Name = "contextMenuStrip1";
            this.contextMenuCategories.Size = new System.Drawing.Size(106, 26);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(105, 22);
            this.addToolStripMenuItem.Text = "Add...";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // pgDetails
            // 
            this.pgDetails.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pgDetails.Location = new System.Drawing.Point(3, 3);
            this.pgDetails.Name = "pgDetails";
            this.pgDetails.Size = new System.Drawing.Size(661, 640);
            this.pgDetails.TabIndex = 0;
            this.pgDetails.PropertyValueChanged += new System.Windows.Forms.PropertyValueChangedEventHandler(this.pgDetails_PropertyValueChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.aboutToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(859, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newRealmToolStripMenuItem,
            this.openRealmToolStripMenuItem,
            this.saveToolStripMenuItem,
            this.saveAsToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // newRealmToolStripMenuItem
            // 
            this.newRealmToolStripMenuItem.Name = "newRealmToolStripMenuItem";
            this.newRealmToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.newRealmToolStripMenuItem.Text = "New...";
            this.newRealmToolStripMenuItem.Click += new System.EventHandler(this.newRealmToolStripMenuItem_Click);
            // 
            // openRealmToolStripMenuItem
            // 
            this.openRealmToolStripMenuItem.Name = "openRealmToolStripMenuItem";
            this.openRealmToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.openRealmToolStripMenuItem.Text = "Open realm...";
            this.openRealmToolStripMenuItem.Click += new System.EventHandler(this.openRealmToolStripMenuItem_Click);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Enabled = false;
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveToolStripMenuItem.Text = "Save";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // saveAsToolStripMenuItem
            // 
            this.saveAsToolStripMenuItem.Enabled = false;
            this.saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
            this.saveAsToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.saveAsToolStripMenuItem.Text = "Save as...";
            this.saveAsToolStripMenuItem.Click += new System.EventHandler(this.saveAsToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(149, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.officialSiteToolStripMenuItem,
            this.aboutToolStripMenuItem1});
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.aboutToolStripMenuItem.Text = "Help";
            // 
            // officialSiteToolStripMenuItem
            // 
            this.officialSiteToolStripMenuItem.Name = "officialSiteToolStripMenuItem";
            this.officialSiteToolStripMenuItem.Size = new System.Drawing.Size(133, 22);
            this.officialSiteToolStripMenuItem.Text = "Official site";
            this.officialSiteToolStripMenuItem.Click += new System.EventHandler(this.officialSiteToolStripMenuItem_Click);
            // 
            // aboutToolStripMenuItem1
            // 
            this.aboutToolStripMenuItem1.Name = "aboutToolStripMenuItem1";
            this.aboutToolStripMenuItem1.Size = new System.Drawing.Size(133, 22);
            this.aboutToolStripMenuItem1.Text = "About...";
            this.aboutToolStripMenuItem1.Click += new System.EventHandler(this.aboutToolStripMenuItem1_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Utopia realm|*.realm|Any file|*.*";
            this.openFileDialog1.Title = "Open an utopia realm...";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "realm";
            this.saveFileDialog1.Filter = "Utopia realm|*.realm|Any file|*.*";
            this.saveFileDialog1.Title = "Save the utopia realm as...";
            // 
            // tvMainCategories
            // 
            this.tvMainCategories.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tvMainCategories.Enabled = false;
            this.tvMainCategories.ImageIndex = 0;
            this.tvMainCategories.ImageList = this.imageList1;
            this.tvMainCategories.Location = new System.Drawing.Point(3, 22);
            this.tvMainCategories.Name = "tvMainCategories";
            treeNode1.ImageIndex = 1;
            treeNode1.Name = "General";
            treeNode1.SelectedImageIndex = 1;
            treeNode1.Text = "General";
            treeNode2.ContextMenuStrip = this.contextMenuCategories;
            treeNode2.ImageIndex = 2;
            treeNode2.Name = "Entities";
            treeNode2.SelectedImageIndex = 2;
            treeNode2.Text = "Entities";
            treeNode3.ContextMenuStrip = this.contextMenuCategories;
            treeNode3.ImageIndex = 4;
            treeNode3.Name = "Cubes";
            treeNode3.SelectedImageIndex = 4;
            treeNode3.Text = "Cubes";
            treeNode4.ImageIndex = 5;
            treeNode4.Name = "WorldProcessor Params";
            treeNode4.SelectedImageIndex = 5;
            treeNode4.Text = "WorldProcessor Params";
            this.tvMainCategories.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2,
            treeNode3,
            treeNode4});
            this.tvMainCategories.SelectedImageIndex = 0;
            this.tvMainCategories.Size = new System.Drawing.Size(182, 621);
            this.tvMainCategories.TabIndex = 5;
            this.tvMainCategories.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvMainCategories_AfterSelect);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "question.png");
            this.imageList1.Images.SetKeyName(1, "general.png");
            this.imageList1.Images.SetKeyName(2, "entities.png");
            this.imageList1.Images.SetKeyName(3, "start.png");
            this.imageList1.Images.SetKeyName(4, "cube.png");
            this.imageList1.Images.SetKeyName(5, "world.png");
            this.imageList1.Images.SetKeyName(6, "wrench.png");
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Margin = new System.Windows.Forms.Padding(3);
            this.label1.Name = "label1";
            this.label1.Padding = new System.Windows.Forms.Padding(3);
            this.label1.Size = new System.Drawing.Size(83, 19);
            this.label1.TabIndex = 7;
            this.label1.Text = "Realm explorer";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.tvMainCategories);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.pgDetails);
            this.splitContainer1.Panel2.Padding = new System.Windows.Forms.Padding(3);
            this.splitContainer1.Size = new System.Drawing.Size(859, 646);
            this.splitContainer1.SplitterDistance = 188;
            this.splitContainer1.TabIndex = 8;
            // 
            // FrmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(859, 670);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "FrmMain";
            this.Text = "Utopia realm editor";
            this.contextMenuCategories.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PropertyGrid pgDetails;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem newRealmToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openRealmToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveAsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem officialSiteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.TreeView tvMainCategories;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ContextMenuStrip contextMenuCategories;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.SplitContainer splitContainer1;
    }
}

