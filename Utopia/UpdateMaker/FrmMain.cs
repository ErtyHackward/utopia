﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using ProtoBuf;
using UpdateMaker.Properties;
using Utopia.Shared.Structs;
using Utopia.Updater;

namespace UpdateMaker
{
    public partial class FrmMain : Form
    {
        private UpdateFile _previousFile;

        private HashSet<string> _ignoredFiles = new HashSet<string>();
        private string baseFolderPath;

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
            baseFolderPath = Path.GetDirectoryName(Settings.Default.BaseFile);

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
                    baseFolderPath = Path.GetDirectoryName(Settings.Default.BaseFile);
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

        private void button1_Click(object sender, EventArgs e)
        {
            // check files we need to update
            updateFile = new UpdateFile();

            updateFile.Message = textBox1.Text;
            updateFile.Version = Assembly.LoadFile(Settings.Default.BaseFile).GetName().Version.ToString();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var f = new UpdateFileInfo();

                f.SystemPath = row.Cells[0].Value.ToString();
                f.Md5Hash = row.Cells[1].Value.ToString();
                f.DownloadUri = "http://update.utopiarealms.com/files/" + f.SystemPath.Replace('\\', '/');

                var fi = new FileInfo(Path.Combine(baseFolderPath, f.SystemPath));
                f.Length = fi.Length;
                f.LastWriteTime = fi.LastWriteTimeUtc;

                updateFile.Files.Add(f);
            }

            var ms = new MemoryStream();
            Serializer.Serialize(ms, updateFile.Files);
            ms.Position = 0;
            var hash = Md5Hash.Calculate(ms);
            updateFile.UpdateToken = hash.ToString();

            updateFile.Files = updateFile.GetChangedFiles(_previousFile);

            if (updateFile.Files.Count == 0)
            {
                MessageBox.Show("No changes found.");
                return;
            }


            // upload them
            finished = false;
            var progressForm = new FrmProgress();

            progressForm.Progress = progress;
            progressForm.Label = file;

            new ThreadStart(Publish).BeginInvoke(null, null);

            progressForm.Show(this);

            while (!finished)
            {
                progressForm.Progress = progress;
                progressForm.Label = file;
                progressForm.Refresh();
                Application.DoEvents();
                Thread.Sleep(10);
            }

            progressForm.Close();
            FrmMain_Load(null, null);
        }

        private float progress = 0f;
        private string file = "Prepare to upload...";
        private bool finished = false;
        private UpdateFile updateFile;

        private void Publish()
        {
            // create directories
            var dirs = updateFile.Files.Select(f => Path.GetDirectoryName(f.SystemPath)).Distinct().ToList();

            foreach (var dir in dirs)
            {
                if (dir == "")
                    continue;

                var ftpReq = (FtpWebRequest)WebRequest.Create("ftp://utopiarealms.com/files/" + dir.Replace('\\', '/'));
                ftpReq.Method = WebRequestMethods.Ftp.MakeDirectory;
                ftpReq.Credentials = new NetworkCredential("update", "xe6ORAXoNO");
                try
                {
                    var response = (FtpWebResponse)ftpReq.GetResponse();
                    Console.WriteLine("Create dir complete, status {0}", response.StatusDescription);
                    response.Close();
                }
                catch (WebException)
                {

                }
            }

            int index = 0;
            // upload files
            foreach (var updateFileInfo in updateFile.Files)
            {
                var url = "ftp://utopiarealms.com/files/" + updateFileInfo.SystemPath.Replace('\\', '/');
                file = updateFileInfo.SystemPath;
                progress = (float)index / updateFile.Files.Count;
                using (var fs = File.OpenRead(Path.Combine(baseFolderPath, updateFileInfo.SystemPath)))
                {
                    UploadFile(url, fs);
                }
                index++;
            }

            // upload index file
            this.file = "index file...";
            MemoryStream ms = new MemoryStream();
            Serializer.Serialize(ms, updateFile);
            ms.Position = 0;
            UploadFile("ftp://utopiarealms.com/index.update", ms);

            finished = true;
        }

        private void UploadFile(string uri, Stream filestream)
        {
            var ftpReq = (FtpWebRequest)WebRequest.Create(uri);

            ftpReq.Method = WebRequestMethods.Ftp.UploadFile;
            ftpReq.Credentials = new NetworkCredential("update", "xe6ORAXoNO");
            ftpReq.UseBinary = true;

            ftpReq.ContentLength = filestream.Length;

            var stream = ftpReq.GetRequestStream();
            filestream.CopyTo(stream);
            stream.Close();

            var response = (FtpWebResponse)ftpReq.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            var req = (HttpWebRequest)WebRequest.Create("http://update.utopiarealms.com/index.update");

            using (var resp = req.GetResponse())
            {
                var stream = resp.GetResponseStream();
                _previousFile = Serializer.Deserialize<UpdateFile>(stream);
            }

            label4.Text = _previousFile.Version + " " + _previousFile.UpdateToken;
            textBox1.Text = _previousFile.Message;
        }

    }
}
