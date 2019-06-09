using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;


namespace Shapp
{
    /// <summary>
    /// Class responsible for executing python scripts.
    /// 
    /// It's ugly. Why such terrible solution?
    /// 1. IronPython - it can't be used. HTCondor API is precompiled into .pyd. Such format
    ///     can't be executed from IronPython.
    /// 2. Why not to simply call a script from some location as "python /path/to/script"?
    ///     Because of accessability - a C# resource can't be access from somewhere outside the program.
    ///     Using some an arbitrary directory with scripts in it sounds like a bad idea in terms of
    ///     "easy and simple library interface".
    /// 3. Potentialy same action could have been performed directly from HTCondor CLI, but:
    ///     a) python equivalent is easier to manipulate with (in this case - dumping HTCondor response
    ///         into easily parsable JSON)
    ///     b) works in the same way on every platform with python installed.
    /// </summary>
    class PythonScriptsExecutor
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string ScriptToExecute;

        /// <summary>
        /// Response from executed script from stdout (every "print "something"").
        /// </summary>
        public string Response { get; private set; }
        /// <summary>
        /// Stderr content. Contains python exceptions and some warnings from HTCondor API.
        /// </summary>
        public string Errors { get; private set; }

        /// <summary>
        /// Construct executor to work with fully prepared script.
        /// Script won't be launched yet.
        /// </summary>
        /// <param name="scriptToExecute">python script to execute</param>
        public PythonScriptsExecutor(string scriptToExecute)
        {
            ScriptToExecute = scriptToExecute;
        }

        /// <summary>
        /// Method for launching the script.
        /// 
        /// After it's end, both field Response and Errors are available.
        /// Can be called multiple times (to execute exactly same script).
        /// Previous Response and Errors won't be preserved.
        /// </summary>
        public void Execute()
        {
            Response = string.Empty;
            Errors = string.Empty;
            ProcessStartInfo processStartInfo = BuildProcessStartInfo();
            LaunchScriptAndGatherResponse(processStartInfo);
            if (Errors.Length != 0)
                throw new ShappException("Error during python script execution: \n" + Errors);
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
