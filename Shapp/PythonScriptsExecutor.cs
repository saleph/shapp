using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;
using System.Runtime.InteropServices;


namespace Shapp
{
    public class PythonScriptsExecutor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string ScriptToExecute;

        public string Response { get; private set; }
        public string Errors { get; private set; }

        public PythonScriptsExecutor(string scriptToExecute)
        {
            ScriptToExecute = scriptToExecute;
        }

        public void Execute()
        {
            ProcessStartInfo processStartInfo = BuildProcessStartInfo();
            LaunchScriptAndGatherResponse(processStartInfo);
            if (Errors.Length != 0)
                throw new ShappException("Error during python script execution");
        }

        private void LaunchScriptAndGatherResponse(ProcessStartInfo processStartInfo)
        {
            using (Process process = Process.Start(processStartInfo))
            {
                WriteScriptToStandardInput(process);
                ReadFromStandarOutput(process);
                ReadFromStandarError(process);
            }
        }

        private static ProcessStartInfo BuildProcessStartInfo()
        {
            return new ProcessStartInfo
            {
                FileName = GetPythonInterpreterPath(),
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardInput = true,
                RedirectStandardError = true
            };
        }

        private static string GetPythonInterpreterPath()
        {
            bool isWindows = Environment.OSVersion.ToString().Contains("Windows");
            if (isWindows)
                return @"C:\Python27\python.exe";
            return @"/usr/bin/python2";
        }

        private void WriteScriptToStandardInput(Process process)
        {
            using (StreamWriter writer = process.StandardInput)
            {
                log.DebugFormat("Python script about to execute:\n{0}", ScriptToExecute);
                writer.Write(ScriptToExecute);
            }
        }

        private void ReadFromStandarOutput(Process process)
        {
            using (StreamReader reader = process.StandardOutput)
            {
                Response = reader.ReadToEnd();
                log.DebugFormat("Python script result:\n{0}", Response);
            }
        }

        private void ReadFromStandarError(Process process)
        {
            using (StreamReader reader = process.StandardError)
            {
                Errors = reader.ReadToEnd();
                log.DebugFormat("Python script errors:\n{0}", Errors);
            }
        }
    }
}
