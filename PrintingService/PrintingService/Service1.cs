using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace PrintingService
{
    public partial class Service1 : ServiceBase
    {
        private System.Timers.Timer timer;

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            timer = new System.Timers.Timer();
            timer.Interval = 1000; // 1 seconds interval
            timer.Elapsed += TimerElapsed;
            timer.Start();

            this.WriteToFile("Simple Service started {0}");

        }

        protected override void OnStop()
        {
            this.WriteToFile("Simple Service stopped {0}");
        }
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" + DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.   
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
        private void TimerElapsed(object sender, ElapsedEventArgs e)
        {
            string path = ConfigurationManager.AppSettings["path"];
            string printerpath = ConfigurationManager.AppSettings["printerip"];
            string downloadsFolder = path + "\\Downloads";
            //this.WriteToFile(downloadsFolder + Environment.NewLine);
            if (Directory.Exists(downloadsFolder))
            {
                string filePath = Path.Combine(downloadsFolder, "example.prn");

                if (File.Exists(filePath))
                {
                    string content = File.ReadAllText(filePath);
                    //this.WriteToFile("File Content:\n" + content);
                    var printerIp = IPAddress.Parse(printerpath);
                    var printerPort = 9100;
                    using (var client = new TcpClient())
                    {
                        client.Connect(printerIp, printerPort);
                        var prnData = System.IO.File.ReadAllBytes(filePath);

                        using (var stream = client.GetStream())
                        {
                            stream.Write(prnData, 0, prnData.Length);
                            stream.Flush();
                        }
                    }
                    File.Delete(filePath);
                    this.WriteToFile("File Deleted" + DateTime.Now.ToShortTimeString());
                }
                else
                {
                    // this.WriteToFile("File not found.");
                }
            }
            else
            {
                //this.WriteToFile("Download Folder no found {0}");
            }
        }

    }
}
