using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace EasyPublish
{
    class Program
    {
        private static string _issCompilerPath;

        static bool ReadYN()
        {
            var key = Console.ReadLine();

            if (key.ToLower() == "y")
                return true;
            return false;
        }

        static void Main(string[] args)
        {
            if (args.Length != 1 || args[0] != "blessYou")
            {
                Console.WriteLine(" Warning! Do not run this program yourself.");
                Console.WriteLine(" To make the publish select Publish configuration in the Visual Studio! Thank you!");
                Console.WriteLine(" Press enter to exit ");
                Console.ReadLine();
                return;
            }

            _issCompilerPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Inno Setup 5\\compil32.exe");

            if (!File.Exists(_issCompilerPath))
            {
                Console.WriteLine("Could not find inno setup compiler. Please install inno setup and repeat the publish");
                Console.WriteLine(" Press enter to exit ");
                Console.ReadLine();
                return;
            }

            var currentFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            var rootFolder = Directory.GetParent(Directory.GetParent(Directory.GetParent(Directory.GetParent(currentFolder).FullName).FullName).FullName).FullName;

            Console.WriteLine("Hello there! Welcome to the easy publish of Utopia Realms!");

            Console.WriteLine("Do you want to publish Utopia Realms Client distributive ? [y/N]");
            if (ReadYN())
            {
                var issFilePath = Path.Combine(rootFolder, "Setup", "realms.iss");
                var programPath = Path.Combine(rootFolder, "Utopia", "Realms", "Realms.Client", "bin", "Release", "Realms.exe");
                var distributivePath = Path.Combine(rootFolder, "Setup", "Output", "setup_realms.exe");

                if (!File.Exists(issFilePath))
                    ShowError("Can't find realms.iss file");
                if (!File.Exists(programPath))
                    ShowError("Can't find Realms.exe file, try to rebuild the game in release configuration");

                PublishDistributive(issFilePath, programPath, distributivePath);
            }
            
            Console.WriteLine("Do you want to publish Utopia Realms Server distributive ? [y/N]");
            if (ReadYN())
            {
                var issFilePath = Path.Combine(rootFolder, "Setup", "realms_server.iss");
                var releasePath = Path.Combine(rootFolder, "Utopia", "Realms", "Realms.Server", "bin", "Release");
                var programPath = Path.Combine(releasePath, "Realms.Server.exe");
                var outputPath = Path.Combine(rootFolder, "Setup", "Output");
                var distributivePath = Path.Combine(outputPath, "setup_realms_server.exe");
                var linuxDistributiveFile = Path.Combine(outputPath, "realms.server.tar.gz");

                if (!File.Exists(issFilePath))
                    ShowError("Can't find realms_server.iss file");
                if (!File.Exists(programPath))
                    ShowError("Can't find Realms.Server.exe file, try to rebuild the server in release configuration");
                
                PublishDistributive(issFilePath, programPath, distributivePath);

                // filter files
                var tempPath = Path.Combine(rootFolder, "Setup", "linux-files");

                if (!Directory.Exists(tempPath))
                    Directory.CreateDirectory(tempPath);

                if (File.Exists(linuxDistributiveFile))
                    File.Delete(linuxDistributiveFile);

                var arg = string.Format("/C copy \"{0}\\*\" \"{1}\\\"", releasePath, tempPath);

                Console.WriteLine("Preparing files...");
                RunCommandSilent("cmd.exe", arg);

                var filterExts = new[] { ".xml", ".pdb" };

                foreach (var file in Directory.EnumerateFiles(tempPath).Where( f => filterExts.Contains(Path.GetExtension(f))))
                {
                    File.Delete(file);
                }
                
                // linux version, we will pack to tar.gz

                Console.WriteLine("Compressing realms.server.tar.gz...");
                RunCommandSilent(Path.Combine(rootFolder, "Setup", "7z.exe"),
                    string.Format("a -ttar \"{0}\\realms.server.tar\" \"{1}\\*\"", outputPath, tempPath));

                RunCommandSilent(Path.Combine(rootFolder, "Setup", "7z.exe"),
                    string.Format("a -tgzip \"{0}\\realms.server.tar.gz\" \"{0}\\realms.server.tar\"", outputPath));

                Console.WriteLine("Uploading file...");
                UploadFile("ftp://update.utopiarealms.com/realms.server.tar.gz", linuxDistributiveFile);

                try
                {
                    Console.WriteLine("Uploading new version token");
                    var version = Assembly.LoadFrom(programPath).GetName().Version;
                    using (var stream = new MemoryStream())
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(version.ToString());
                        writer.Flush();
                        stream.Position = 0;
                        UploadFile("ftp://update.utopiarealms.com/token_server", stream);
                    }
                }
                catch (Exception)
                {
                    
                }
                

                Console.WriteLine("Cleanup...");
                Directory.Delete(tempPath, true);
                File.Delete(Path.Combine(outputPath, "realms.server.tar"));
                Console.WriteLine();
            }

            Console.WriteLine("Do you want to publish client auto update? [y/N]");
            if (ReadYN())
            {
                var realmsPath = Path.Combine(rootFolder, "Utopia", "Realms", "Realms.Client", "bin", "Release", "Realms.exe");
                var programPath = Path.Combine(rootFolder, "Utopia", "UpdateMaker", "bin", "Release", "UpdateMaker.exe");
                Process.Start(programPath, "\"" + realmsPath + "\"").WaitForExit();
            }

            Console.WriteLine("We're done! Thank you!");
        }

        private static void RunCommandSilent(string command, string arguments)
        {
            var process = new Process();
            var startInfo = new ProcessStartInfo();
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            startInfo.FileName = command;
            startInfo.Arguments = arguments;
            process.StartInfo = startInfo;
            process.Start();
            process.WaitForExit();
        }

        static void ShowError(string message)
        {
            Console.WriteLine(message);
            Console.WriteLine("Press enter to exit");
            Console.ReadLine();
            Environment.Exit(0);
        }

        static void PublishDistributive(string issFile, string exeFile, string distibutiveFile)
        {
            var version = Assembly.LoadFile(exeFile).GetName().Version.ToString(3);

            Console.WriteLine("Updating product version...");
            UpdateVersion(issFile, version);
            Console.WriteLine("Compiling the distributive...");
            Process.Start(_issCompilerPath, "/cc " + issFile).WaitForExit();
            Console.WriteLine("Uploading file...");
            UploadFile("ftp://update.utopiarealms.com/" + Path.GetFileName(distibutiveFile), distibutiveFile);
            Console.WriteLine();
        }

        static void UploadFile(string uri, string filePath)
        {
            using (var fs = File.OpenRead(filePath))
            {
                UploadFile(uri, fs);
            }
        }

        static void UploadFile(string uri, Stream filestream)
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

        static void UpdateVersion(string issFilePath, string version)
        {
            var lines = File.ReadAllLines(issFilePath);

            var prevVersion = "";

            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].StartsWith("VersionInfoVersion"))
                {
                    prevVersion = lines[i].Substring(lines[i].IndexOf('=') + 1).Trim();
                    lines[i] = "VersionInfoVersion=" + version;
                }

                if (lines[i].StartsWith("AppVerName"))
                {
                    lines[i] = lines[i].Replace(prevVersion, version);
                }
            }

            File.WriteAllLines(issFilePath, lines);
        }
    }
}
