﻿using System;
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
using System.Configuration;
using System.Text.RegularExpressions;

namespace FileWatcher
{
    public partial class Form1 : Form
    {
        string pathNewDocuments = null;
        string pathInbox = null;
        string pathLog = null;
        string logFileName = "NewDocuments-log-" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".txt";
        string logFileNameInbox = "Inbox-log-" + DateTime.Now.ToString("yyyyMMddTHHmmss") + ".txt";
        int countCopy = 3;
        int delayCopy = 1;

        bool taskOpen = false;
        System.Threading.Timer timer;
        FileSystemWatcher fs;
        FileSystemWatcher fsInbox;
        string textOneLineLog = "";
        string textOneLineLogInbox = "";
        string fullPath = "";
        string fullPathInbox;

        public Form1()
        {
            InitializeComponent();

            this.pathNewDocuments = ReadSetting("pathNewDocuments");
            this.pathInbox = ReadSetting("pathInbox");
            this.pathLog = ReadSetting("pathLog");
            this.countCopy = Convert.ToInt32(ReadSetting("countCopy"));
            this.delayCopy = Convert.ToInt32(ReadSetting("delayCopy"));

            textBox2.Text = this.pathNewDocuments;
            textBox3.Text = this.pathInbox;
            textBox1.Text = this.countCopy.ToString();
            textBox4.Text = this.pathLog;
            textBox5.Text = this.delayCopy.ToString();

            this.fullPath = this.pathLog + "/" + this.logFileName;
            this.fullPathInbox = this.pathLog + "/" + this.logFileNameInbox;
            RunWatch();
        }

        public void AddUpdateAppSettings(string key, string value)
        {
            try
            {
                var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = configFile.AppSettings.Settings;
                if (settings[key] == null)
                {
                    settings.Add(key, value);
                }
                else
                {
                    settings[key].Value = value;
                }
                configFile.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection(configFile.AppSettings.SectionInformation.Name);
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error writing app settings");
            }
        }

        public string ReadSetting(string key)
        {
            string result = "";
            try
            {
                var appSettings = ConfigurationManager.AppSettings;
                result = appSettings[key] ?? "Not Found";
            }
            catch (ConfigurationErrorsException)
            {
                MessageBox.Show("Error reading app settings");
            }
            return result;
        }

        private void RunWatch()
        {
            //FileInfo fileInf = new FileInfo(pathFile);
            //fileInf.Create();

            if(fs == null)
            {
                fs = new FileSystemWatcher();
                fs.Path = this.pathNewDocuments;
                fs.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                //fs.Filter = "*.pdf";
                fs.Changed += new FileSystemEventHandler(onChange);
                fs.Created += new FileSystemEventHandler(onChange);
                fs.Deleted += new FileSystemEventHandler(onChange);
                fs.Renamed += new RenamedEventHandler(onChange);
                fs.EnableRaisingEvents = true;
            }

            if (fsInbox == null)
            {
                fsInbox = new FileSystemWatcher();
                fsInbox.Path = this.pathInbox;
                fsInbox.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                //fs.Filter = "*.pdf";
                fsInbox.Created += new FileSystemEventHandler(onChangeInbox);
                fsInbox.Deleted += new FileSystemEventHandler(onChangeInbox);
                fsInbox.EnableRaisingEvents = true;
            }

            if (this.timer != null)
            {
                this.timer.Change(Timeout.Infinite, Timeout.Infinite);
            }
            TimerCallback tm = new TimerCallback(Count);
            this.timer = new System.Threading.Timer(tm, null, 0, 500);

        }

        private void onChangeInbox(object sender, FileSystemEventArgs e)
        {
            
            textOneLineLogInbox = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " --File: " + e.FullPath + " " + e.ChangeType + "\n\r";
            richTextBox2.Invoke(new Action(() => richTextBox2.Text += textOneLineLogInbox));
            richTextBox2.Invoke(new Action(() => richTextBox2.SaveFile(this.fullPathInbox, RichTextBoxStreamType.PlainText)));
           
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
            
            textOneLineLog = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString() + " --File: " + e.FullPath + " " + e.ChangeType + "\n\r";
            richTextBox1.Invoke(new Action(() => richTextBox1.Text += textOneLineLog));
            richTextBox1.Invoke(new Action(() => richTextBox1.SaveFile(this.fullPath, RichTextBoxStreamType.PlainText)));
            
        }

        private async void setTimer()
        {
            await Task.Delay(this.delayCopy*60*1000);
            fileMove();
            taskOpen = false;
        }

        private void fileMove()
        {
            DirectoryInfo dir = new DirectoryInfo(this.pathInbox);
            FileInfo[] files = dir.GetFiles();

            int k = 0;
            foreach (FileInfo f in files)
            {
                if (k == countCopy)
                {
                    break;
                }
                if (f.Length > 0 && f.Extension == ".pdf")
                {
                    try
                    {
                        f.MoveTo(this.pathNewDocuments + "/" + f.Name);
                    }
                    catch (Exception ex)
                    {
                        DisplayException(ex);
                    }
                    k++;
                }
                
            }
        }

        private void DisplayException(Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("An exception of type \"");
            sb.Append(ex.GetType().FullName);
            sb.Append("\" has occurred.\r\n");
            sb.Append(ex.Message);
            sb.Append("\r\nStack trace information:\r\n");

            MatchCollection matchCol = Regex.Matches(ex.StackTrace,
@"(at\s)(.+)(\.)([^\.]*)(\()([^\)]*)(\))((\sin\s)(.+)(:line )([\d]*))?");
            int L = matchCol.Count;
            string[] argList;
            Match matchObj;
            int y, K;
            for (int x = 0; x < L; x++)
            {
                matchObj = matchCol[x];
                sb.Append(matchObj.Result("\r\n\r\n$1 $2$3$4$5"));
                argList = matchObj.Groups[6].Value.Split(new char[] { ',' });
                K = argList.Length;
                for (y = 0; y < K; y++)
                {
                    sb.Append("\r\n    ");
                    sb.Append(argList[y].Trim().Replace(" ", "        "));
                    sb.Append(',');
                }
                sb.Remove(sb.Length - 1, 1);
                sb.Append("\r\n)");
                if (0 < matchObj.Groups[8].Length)
                {
                    sb.Append(matchObj.Result("\r\n$10\r\nline $12"));
                }
            }
            argList = null;
            matchObj = null;
            matchCol = null;
            MessageBox.Show(sb.ToString());
            sb = null;
        }

        private bool isDirectoryEmpty()
        {
            return !Directory.EnumerateFileSystemEntries(this.pathNewDocuments).Any();
        }

        private bool isDirectorySourceEmpty()
        {
            return !Directory.EnumerateFileSystemEntries(this.pathInbox).Any();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.pathNewDocuments = textBox2.Text;
            this.pathInbox = textBox3.Text;
            this.pathLog = textBox4.Text;
            this.countCopy = Convert.ToInt32(textBox1.Text);
            this.delayCopy = Convert.ToInt32(textBox5.Text);

            AddUpdateAppSettings("pathNewDocuments", this.pathNewDocuments);
            AddUpdateAppSettings("pathInbox", this.pathInbox);
            AddUpdateAppSettings("pathLog", this.pathLog);
            AddUpdateAppSettings("countCopy", Convert.ToString(this.countCopy));
            AddUpdateAppSettings("delayCopy",Convert.ToString(this.delayCopy));

            this.fullPath = this.pathLog + "/" + this.logFileName;
            this.fullPathInbox = this.pathLog + "/" + this.logFileNameInbox;
            RunWatch();
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {

        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void aboutFileWatcher100ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AboutForm form2 = new AboutForm();
            form2.Show();
        }
    }
}
