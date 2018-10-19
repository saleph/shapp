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
            string FileName = string.Format("{0}Resources\\test.py", Path.GetFullPath(Path.Combine(RunningPath, @"..\..\")));
            run_cmd(@"C:\batch.py", "");
            return 0;
        }

        private void run_cmd(string cmd, string args)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\Python27\python.exe";
            start.Arguments = string.Format("{0} {1}", cmd, args);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
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
