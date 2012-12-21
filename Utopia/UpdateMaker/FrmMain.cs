using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using UpdateMaker.Properties;
using Utopia.Shared.Structs;

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
                {
                    Md5Hash hash;
                    using (var fs = File.OpenRead(file))
                    {
                        hash = Md5Hash.Calculate(fs);
                    }
                    dataGridView1.Rows.Add(filePath,hash.ToString());
                }
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
            dataGridView1.Rows.Clear();
            LoadFiles(Path.GetDirectoryName(Settings.Default.BaseFile));
        }

        private void ingoreToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView1.SelectedRows)
            {
                _ignoredFiles.Add((string)row.Cells[0].Value);
                dataGridView1.Rows.Remove(row);
            }

            File.WriteAllLines(IgnoreListPath, _ignoredFiles);
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start(IgnoreListPath);
        }
    }
}
