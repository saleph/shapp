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
using System.Runtime.InteropServices;


namespace Shapp
{
    public class PythonScriptsExecutor
    {
        public string Execute(string script)
        {
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = GetPythonInterpreterPath();
            Console.Out.WriteLine(start.Arguments);
            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            start.RedirectStandardInput = true;

            using (Process process = Process.Start(start))
            {
                using (StreamWriter writer = process.StandardInput)
                {
                    writer.Write(script);
                }
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    return result;
                }
            }
        }

        private static string GetPythonInterpreterPath()
        {
            bool isWindows = Environment.OSVersion.ToString().Contains("Windows");
            if (isWindows)
                return @"C:\Python27\python.exe";
            return @"/usr/bin/python2";
        }
    }
}
