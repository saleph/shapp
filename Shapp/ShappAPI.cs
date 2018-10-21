using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using IronPython.Runtime;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;


namespace Shapp
{
    public class ShappAPI
    {
        public int Elo()
        {
            string RunningPath = AppDomain.CurrentDomain.BaseDirectory;
            run_cmd(Properties.Resources.testscript, "");
            return 0;
        }

        private void run_cmd(string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\Python27\python.exe";
            //start.Arguments = string.Format("{0}\" {1}", cmd, args);
            Console.Out.WriteLine(start.Arguments);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardInput = true;
            //string FileName = GetScriptsPath();

            //start.EnvironmentVariables[name] = value;

            using (Process process = Process.Start(start))
            {
                using (StreamWriter writer = process.StandardInput)
                {
                    writer.Write(cmd);
                }
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }

        private static void SetEnvironmentVariable(ProcessStartInfo start, string name, string value)
        {
        }
    }
}
