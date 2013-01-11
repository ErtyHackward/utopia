using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Wav2Adpcm
{
    public partial class Form1 : Form
    {
        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const UInt32 SWP_NOSIZE = 0x0001;
        const UInt32 SWP_NOMOVE = 0x0002;
        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public Form1()
        {
            InitializeComponent();
        }

        private void label1_DragDrop(object sender, DragEventArgs e)
        {
            CompressWaveFile(e);
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            CompressWaveFile(e);
        }

        private void CompressWaveFile(DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.Exists && fi.Extension == ".wav" && fi.Name.Contains("adpcm") == false)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = "AdpcmEncode.exe";
                    p.StartInfo.Arguments = "\"" + fi.FullName + "\"" + " " + "\"" + fi.Directory +  "\\" + Path.GetFileNameWithoutExtension(fi.Name) + ".adpcm.wav" + "\"";
                    p.StartInfo.UseShellExecute = false;
                    p.StartInfo.RedirectStandardOutput = true;
                    p.Start();
                    string output = p.StandardOutput.ReadToEnd();
                    Resultoutput.Items.Add(output);
                    p.WaitForExit();

                    if (deleteOriginalFile.Checked == true)
                    {
                        fi.Delete();
                    }
                }
            }
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            DragBegin(e);
        }

        private void label1_DragEnter(object sender, DragEventArgs e)
        {
            DragBegin(e);
        }

        private void DragBegin(DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }
        private void listBox1_DragDrop(object sender, DragEventArgs e)
        {
            CompressWaveFile(e);
        }

        private void listBox1_DragEnter(object sender, DragEventArgs e)
        {
            DragBegin(e);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            SetWindowPos(this.Handle, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
        }
    }
}
