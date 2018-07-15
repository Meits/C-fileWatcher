using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        string path = null;
        string pathSource = null;

        bool taskOpen = false;

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
            fs.EnableRaisingEvents = true;

            // устанавливаем метод обратного вызова
            TimerCallback tm = new TimerCallback(Count);
            // создаем таймер
            System.Threading.Timer timer = new System.Threading.Timer(tm, null, 0, 500);

            //fs.WaitForChanged(WatcherChangeTypes.All, 10000);

            //FileSystemWatcher fs2 = new FileSystemWatcher();
            //fs2.Path = this.pathSource;
            //fs2.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            ////fs.Filter = "*.pdf";
            //fs2.Changed += new FileSystemEventHandler(onChange);
            //fs2.Created += new FileSystemEventHandler(onChange);
            //fs2.Deleted += new FileSystemEventHandler(onChange);
            //fs2.Renamed += new RenamedEventHandler(onChange);
            //fs2.WaitForChanged(WatcherChangeTypes.All, 10000);
            //fs2.EnableRaisingEvents = true;

        }

        private void Count(object state)
        {
            if (isDirectoryEmpty() && !isDirectorySourceEmpty() && !taskOpen )
            {
                taskOpen = true;
                setTimer();
            }
        }

        private void onChange(object sender, FileSystemEventArgs e)
        {
            //FileSystemWatcher f = sender as FileSystemWatcher;
            //MessageBox.Show(f.Path);
            richTextBox1.Text += DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " --File: " + e.FullPath + " " + e.ChangeType + "\n\r";
            richTextBox1.SaveFile(this.pathFile, RichTextBoxStreamType.PlainText);

            //if (isDirectoryEmpty())
            //{
            //    setTimer();
            //}
        }

        private async void setTimer()
        {
            await Task.Delay(5000);
            fileMove();
            taskOpen = false;
        }

        private void fileMove()
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

        }

        private bool isDirectoryEmpty()
        {
            return !Directory.EnumerateFileSystemEntries(this.path).Any();
        }

        private bool isDirectorySourceEmpty()
        {
            return !Directory.EnumerateFileSystemEntries(this.pathSource).Any();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.path = textBox2.Text;
            this.pathSource = textBox3.Text;
            RunWatch();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }
    }
}
