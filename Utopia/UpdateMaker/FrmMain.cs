using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using UpdateMaker.Properties;
using Utopia.Updater;

namespace UpdateMaker
{
    public partial class FrmMain : Form
    {
        private UpdateFile _previousFile;

        private readonly HashSet<string> _ignoredFiles = new HashSet<string>();
        private string _baseFolderPath;
        private float _progress;
        private string _file = "Prepare to upload...";
        private bool _finished = false;
        private UpdateFile _updateFile;
        private List<UpdateFileInfo> _newFiles = new List<UpdateFileInfo>(); 

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
            _baseFolderPath = Path.GetDirectoryName(Settings.Default.BaseFile);

            if (File.Exists(IgnoreListPath))
            {
                foreach (var file in File.ReadAllLines(IgnoreListPath))
                {
                    _ignoredFiles.Add(file);
                }
            }

            if (File.Exists(Settings.Default.BaseFile))
            {
                button2_Click(null, null);
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
                    _baseFolderPath = Path.GetDirectoryName(Settings.Default.BaseFile);
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
            _updateFile = new UpdateFile();

            _updateFile.Message = textBox1.Text;
            _updateFile.Version = Assembly.LoadFile(Settings.Default.BaseFile).GetName().Version.ToString();

            foreach (DataGridViewRow row in dataGridView1.Rows)
            {
                var f = new UpdateFileInfo();

                f.SystemPath = row.Cells[0].Value.ToString();
                f.Md5Hash = row.Cells[1].Value.ToString();
                f.DownloadUri = "http://update.utopiarealms.com/files/" + f.SystemPath.Replace('\\', '/');

                var fi = new FileInfo(Path.Combine(_baseFolderPath, f.SystemPath));
                f.Length = fi.Length;
                f.LastWriteTime = fi.LastWriteTimeUtc;

                _updateFile.Files.Add(f);
            }

            var ms = new MemoryStream();
            var writer = new BinaryWriter(ms);
            foreach (var fileInfo in _updateFile.Files)
            {
                fileInfo.Save(writer);
            }
            ms.Position = 0;
            var hash = Md5Hash.Calculate(ms);
            _updateFile.UpdateToken = hash.ToString();

            if (_previousFile != null)
                _newFiles = _updateFile.GetChangedFiles(_previousFile);
            else
            {
                _newFiles = _updateFile.Files;
            }

            if (_newFiles.Count == 0)
            {
                MessageBox.Show("No changes found.");
                return;
            }


            // upload them
            _finished = false;
            var progressForm = new FrmProgress();

            progressForm.Progress = _progress;
            progressForm.Label = _file;

            new ThreadStart(Publish).BeginInvoke(null, null);

            progressForm.Show(this);

            while (!_finished)
            {
                progressForm.Progress = _progress;
                progressForm.Label = _file;
                progressForm.Refresh();
                Application.DoEvents();
                Thread.Sleep(10);
            }

            progressForm.Close();
            FrmMain_Load(null, null);

            MessageBox.Show("Autoupdate publish is complete","Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void Publish()
        {
            // create directories
            var dirs = _newFiles.Select(f => Path.GetDirectoryName(f.SystemPath)).Distinct().ToList();

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
            foreach (var updateFileInfo in _newFiles)
            {
                var url = "ftp://update.utopiarealms.com/files/" + updateFileInfo.SystemPath.Replace('\\', '/');
                _file = updateFileInfo.SystemPath;
                _progress = (float)index / _newFiles.Count;
                using (var fs = File.OpenRead(Path.Combine(_baseFolderPath, updateFileInfo.SystemPath)))
                {
                    UploadFile(url, fs);
                }
                index++;
            }

            // upload index file
            _file = "index file...";
            using (var ms = new MemoryStream())
            {
                using (var zs = new GZipStream(ms, CompressionMode.Compress))
                {
                    var bwriter = new BinaryWriter(zs);
                    _updateFile.Save(bwriter);
                }
                var ms2 = new MemoryStream(ms.ToArray());
                UploadFile("ftp://update.utopiarealms.com/index.update", ms2);
            }

            using (var ms = new MemoryStream())
            {
                using (var writer = new StreamWriter(ms, Encoding.UTF8))
                {
                    writer.Write(_updateFile.UpdateToken);
                    writer.Flush();
                    UploadFile("ftp://update.utopiarealms.com/token", ms);
                }
            }
            _finished = true;
        }

        private void UploadFile(string uri, Stream filestream)
        {
            var ftpReq = (FtpWebRequest)WebRequest.Create(uri);

            ftpReq.Method = WebRequestMethods.Ftp.UploadFile;
            ftpReq.Credentials = new NetworkCredential("update", "xe6ORAXoNO");
            ftpReq.UseBinary = true;

            ftpReq.ContentLength = filestream.Length;

            filestream.Position = 0;
            var stream = ftpReq.GetRequestStream();
            filestream.CopyTo(stream);
            stream.Close();

            var response = (FtpWebResponse)ftpReq.GetResponse();

            Console.WriteLine("Upload File Complete, status {0}", response.StatusDescription);

            response.Close();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            try
            {
                var req = (HttpWebRequest)WebRequest.Create("http://update.utopiarealms.com/index.update");

                using (var resp = req.GetResponse())
                {
                    var stream = resp.GetResponseStream();
                    using (var zs = new GZipStream(stream, CompressionMode.Decompress))
                    {
                        var reader = new BinaryReader(zs);
                        _previousFile = new UpdateFile();
                        _previousFile.Load(reader);
                    }
                }

                label4.Text = _previousFile.Version + " " + _previousFile.UpdateToken;
                textBox1.Text = _previousFile.Message;
            }
            catch (Exception)
            {


            }
        }

    }
}
