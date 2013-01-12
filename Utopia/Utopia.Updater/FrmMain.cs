using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Windows.Forms;
using Utopia.Shared.Structs;

namespace Utopia.Updater
{
    public partial class FrmMain : Form
    {
        private string _basePath;
        private string _localToken;
        private string _serverToken;
        private AsyncOperation _ao;
        private UpdateFile _updateFile;


        public FrmMain(string serverToken)
        {
            _serverToken = serverToken;
            InitializeComponent();
            _ao = AsyncOperationManager.CreateOperation(null);
        }

        public void StartGame()
        {
            Process.Start(Path.Combine(_basePath, "Realms.exe"));
            Application.Exit();
        }

        private void FrmMain_Load(object sender, EventArgs e)
        {
            _basePath = Path.GetDirectoryName(Application.ExecutablePath);

            if (string.IsNullOrEmpty(_serverToken))
            {
                // receive server token
                try
                {
                    var request = WebRequest.Create("http://update.utopiarealms.com/token");
                    request.BeginGetResponse(TokenReceived, request);
                }
                catch (Exception)
                {
                    if (
                        MessageBox.Show(
                            "Unable to access utopia server, check your internet connection. Would you like to play offline?",
                            "Question", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        StartGame();
                    }
                    else
                    {
                        Application.Exit();
                    }
                }
            }
            else
            {
                CheckToken();
            }
        }

        private void CheckToken()
        {
            var tokenPath = Path.Combine(_basePath, "update.token");

            if (File.Exists(tokenPath))
            {
                _localToken = File.ReadAllText(tokenPath);
            }

            if (_serverToken == _localToken)
            {
                StartGame();
                Application.Exit();
                return;
            }
            
            // check if we need to have admin rights
            try
            {
                new FileStream(Path.Combine(_basePath, "update.token"), FileMode.OpenOrCreate, FileAccess.ReadWrite).Dispose();
            }
            catch (Exception)
            {
                VistaSecurity.RestartElevated(_serverToken);
                return;
            }

            // update the game
            new ThreadStart(Update).BeginInvoke(null, null);
        }

        private new void Update()
        {
            try
            {
                Status("Load update info...");
                // get update index
                var req = WebRequest.Create("http://update.utopiarealms.com/index.update");

                using (var response = req.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var zs = new GZipStream(stream, CompressionMode.Decompress))
                {
                    var reader = new BinaryReader(zs);
                    _updateFile = new UpdateFile();
                    _updateFile.Load(reader);
                }

                Status("Check files to update...");

                var files = new List<UpdateFileInfo>();

                foreach (var file in _updateFile.Files)
                {
                    var filePath = Path.Combine(_basePath, file.SystemPath);

                    if (File.Exists(filePath))
                    {
                        using (var fs = File.OpenRead(filePath))
                        {
                            var hash = Md5Hash.Calculate(fs);
                            if (hash.ToString() == file.Md5Hash && fs.Length == file.Length)
                                continue;
                        }
                    }

                    files.Add(file);
                }

                float index = 0f;
                foreach (var file in files)
                {
                    Status("Updating " + file.SystemPath, index / files.Count);
                    try
                    {

                        var filePath = Path.Combine(_basePath, file.SystemPath);
                        if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                            Directory.CreateDirectory(Path.GetDirectoryName(filePath));

                        var fileReq = WebRequest.Create(file.DownloadUri);

                        using (var response = fileReq.GetResponse())
                        using (var fs = File.OpenWrite(filePath))
                        {
                            fs.SetLength(0);
                            using (var stream = response.GetResponseStream())
                            {
                                if (file.Compressed)
                                {
                                    using (var zip = new GZipStream(stream, CompressionMode.Decompress))
                                    {
                                        zip.CopyTo(fs);
                                    }
                                }
                                else
                                {
                                    stream.CopyTo(fs);
                                }


                            }
                        }
                    }
                    catch (Exception x)
                    {
                        MessageBox.Show(x.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Application.Exit();
                    }

                    index++;
                }

                // save current token
                File.WriteAllText(Path.Combine(_basePath, "update.token"), _updateFile.UpdateToken);

                StartGame();
            }
            catch (Exception x)
            {
                MessageBox.Show(string.Format("Exception occured: {0}\nPlease post this information to the forum at http://utopiarealms.com", x.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Status(string status, float progress = 0f)
        {
            _ao.Post(o =>
                {
                    label1.Text = status;
                    progressBar1.Value = (int)( 100 * progress );
                }, null);
        }

        private void TokenReceived(IAsyncResult result)
        {
            var request = (WebRequest)result.AsyncState;
            
            using (var response = request.EndGetResponse(result))
            using (var reader = new StreamReader(response.GetResponseStream()))
            {
                _serverToken = reader.ReadToEnd();
            }

            _ao.Post((o) => CheckToken(), null);
        }
    }
}
