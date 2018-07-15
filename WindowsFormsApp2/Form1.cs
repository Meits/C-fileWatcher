using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        string path = null;
        string pathSource = null;

        string pathFile = "log-" + DateTime.Now.ToShortDateString() + ".txt";
        int countCopy = 2;
        public Form1()
        {
            InitializeComponent();
        }

        private void RunWatch()
        {
            //FileInfo fileInf = new FileInfo(pathFile);
            //fileInf.Create();

            FileSystemWatcher fs = new FileSystemWatcher();
            fs.Path = this.path;
            fs.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;

            //fs.Filter = "*.pdf";

            fs.Changed += new FileSystemEventHandler(onChange);
            fs.Created += new FileSystemEventHandler(onChange);
            fs.Deleted += new FileSystemEventHandler(onChange);
            fs.Renamed += new RenamedEventHandler(onChange);

            fs.WaitForChanged(WatcherChangeTypes.All, 10000);

            fs.EnableRaisingEvents = true;

        }

        private void onChange(object sender, FileSystemEventArgs e)
        {
            richTextBox1.Text += "File: " + e.FullPath + " " + e.ChangeType + "\n\r";
            richTextBox1.SaveFile(this.pathFile, RichTextBoxStreamType.PlainText);

            if (isDirectoryEmpty())
            {
                setTimer(richTextBox1.Text);
            }
        }

        private async void setTimer( string text)
        {
            await Task.Delay(5000);
            fileMove(text);
        }

        private void fileMove(string text)
        {
            DirectoryInfo dir = new DirectoryInfo(this.pathSource);
            FileInfo[] files = dir.GetFiles();

            int k = 0;
            foreach (FileInfo f in files)
            {
                if (k == countCopy)
                {
                    break;
                }
                if (f.Length > 0)
                {
                    f.MoveTo(this.path + "/" + f.Name);
                }

                k++;
            }

            //File f = new File(this.pathFile);
        }

        private bool isDirectoryEmpty()
        {
            return !Directory.EnumerateFileSystemEntries(this.path).Any();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.path = textBox2.Text;
            this.pathSource = textBox3.Text;
            RunWatch();
        }
    }
}
