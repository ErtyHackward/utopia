using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using UpdateMaker.Properties;

namespace UpdateMaker
{
    public partial class FrmMain : Form
    {
        private HashSet<string> _ignoredFiles = new HashSet<string>();

        public string IgnoreListPath
        {
            get { return Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "ignore.txt"); }
        }

        public FrmMain()
        {
            InitializeComponent();

            if (string.IsNullOrEmpty(Settings.Default.BaseFile))
            {
                SelectBaseFolder();
            }

            if (string.IsNullOrEmpty(Settings.Default.BaseFile))
                Application.Exit();

            linkLabel1.Text = Settings.Default.BaseFile;

            if (File.Exists(IgnoreListPath))
            {
                foreach (var file in File.ReadAllLines(IgnoreListPath))
                {
                    _ignoredFiles.Add(file);
                }
            }

        }

        public void SelectBaseFolder()
        {
            using (var fileDialog = new OpenFileDialog())
            {
                fileDialog.Title = "Please select base file where update should be generated from";
                fileDialog.Filter = "root executable|*.exe";

                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    Settings.Default.BaseFile = fileDialog.FileName;
                    Settings.Default.Save();
                    linkLabel1.Text = Settings.Default.BaseFile;
                }
            }
        }

        public void LoadFiles(string path)
        {
            foreach (var file in Directory.EnumerateFiles(path))
            {
                var filePath = file.Remove(0, Path.GetDirectoryName(Settings.Default.BaseFile).Length + 1);

                if (!_ignoredFiles.Contains(filePath))
                    listView1.Items.Add(filePath);
            }

            foreach (var directory in Directory.EnumerateDirectories(path))
            {
                LoadFiles(directory);
            }

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            SelectBaseFolder();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            listView1.Items.Clear();
            LoadFiles(Path.GetDirectoryName(Settings.Default.BaseFile));
        }

        private void ingoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem item in listView1.SelectedItems)
            {
                _ignoredFiles.Add(item.Text);
                listView1.Items.Remove(item);
            }

            File.WriteAllLines(IgnoreListPath, _ignoredFiles);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(IgnoreListPath);
        }
    }
}
