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
            run_cmd("testscript", "");
            return 0;
        }

        private void run_cmd(string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\Python27\python.exe";
            start.Arguments = string.Format("-m \"{0}\" {1}", cmd, args);
            Console.Out.WriteLine(start.Arguments);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            string RunningPath = AppDomain.CurrentDomain.BaseDirectory;
            string FileName = string.Format("{0}Resources\\", Path.GetFullPath(Path.Combine(RunningPath, @"..\..\..\Shapp\")));
            start.EnvironmentVariables["PYTHONPATH"] = string.Format("{0};{1}", start.EnvironmentVariables["PYTHONPATH"], FileName);

            foreach(string val in start.EnvironmentVariables.Keys)
            {
                Console.Out.WriteLine(val +": "+ start.EnvironmentVariables[val]);
            }
            Console.Out.WriteLine(start.EnvironmentVariables.Values.ToString());
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.Write(result);
                }
            }
        }
    }
}
