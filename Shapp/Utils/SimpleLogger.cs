/*
MIT License

Copyright (c) 2019 Heiswayi Nrird

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
public class SimpleLogger {
    private const string FILE_EXT = ".log";
    private readonly string datetimeFormat;
    private readonly string logFilename;
    private readonly object fileLock = new object();

    public LogLevel FileLoggingLevel = LogLevel.INFO;
    public LogLevel ConsoleLoggingLevel = LogLevel.INFO;


    /// <summary>
    /// Initiate an instance of SimpleLogger class constructor.
    /// If log file does not exist, it will be created automatically.
    /// </summary>
    public SimpleLogger() {
        datetimeFormat = "yyyy-MM-dd HH:mm:ss.fff";
        if (Shapp.JobEnvVariables.GetMyJobId() != null) {
            logFilename = string.Format("x_shapp_{0}_{1}_{2}{3}",
                GetTimeDate(), Shapp.JobEnvVariables.GetMyJobId().ToString(), Shapp.Helper.RandomString(5), FILE_EXT);
        } else {
            logFilename = string.Format("x_shapp_{0}_{1}{2}",
                GetTimeDate(), Shapp.Helper.RandomString(5), FILE_EXT);
        }

        // Log file header line
        string logHeader = logFilename + " is created.";
        if (!System.IO.File.Exists(logFilename)) {
            WriteLine(System.DateTime.Now.ToString(datetimeFormat) + " " + logHeader, LogLevel.FATAL, false);
        }
    }

    private static string GetTimeDate() {
        string DateTime = System.DateTime.Now.ToString("dd-MM-yyyy_HH_mm_ss");
        return DateTime;
    }

    /// <summary>
    /// Log a DEBUG message
    /// </summary>
    /// <param name="text">Message</param>
    public void Debug(string text) {
        WriteFormattedLog(LogLevel.DEBUG, text);
    }

    /// <summary>
    /// Log an ERROR message
    /// </summary>
    /// <param name="text">Message</param>
    public void Error(string text) {
        WriteFormattedLog(LogLevel.ERROR, text);
    }

    /// <summary>
    /// Log a FATAL ERROR message
    /// </summary>
    /// <param name="text">Message</param>
    public void Fatal(string text) {
        WriteFormattedLog(LogLevel.FATAL, text);
    }

    /// <summary>
    /// Log an INFO message
    /// </summary>
    /// <param name="text">Message</param>
    public void Info(string text) {
        WriteFormattedLog(LogLevel.INFO, text);
    }

    /// <summary>
    /// Log a TRACE message
    /// </summary>
    /// <param name="text">Message</param>
    public void Trace(string text) {
        WriteFormattedLog(LogLevel.TRACE, text);
    }

    /// <summary>
    /// Log a WARNING message
    /// </summary>
    /// <param name="text">Message</param>
    public void Warning(string text) {
        WriteFormattedLog(LogLevel.WARNING, text);
    }

    private void WriteLine(string text, LogLevel level, bool append = true) {
        if (string.IsNullOrEmpty(text)) {
            return;
        }
        if (level >= FileLoggingLevel) {
            lock (fileLock) {
                using (System.IO.StreamWriter writer = new System.IO.StreamWriter(logFilename, append, System.Text.Encoding.UTF8)) {
                    writer.WriteLine(text);
                }
            }
        }
        if (level >= ConsoleLoggingLevel) {
            Console.Out.WriteLine(text);
        }
    }

    private void WriteFormattedLog(LogLevel level, string text) {
        string pretext;
        switch (level) {
            case LogLevel.TRACE:
                pretext = System.DateTime.Now.ToString(datetimeFormat) + " [TRACE]   ";
                break;
            case LogLevel.INFO:
                pretext = System.DateTime.Now.ToString(datetimeFormat) + " [INFO]    ";
                break;
            case LogLevel.DEBUG:
                pretext = System.DateTime.Now.ToString(datetimeFormat) + " [DEBUG]   ";
                break;
            case LogLevel.WARNING:
                pretext = System.DateTime.Now.ToString(datetimeFormat) + " [WARNING] ";
                break;
            case LogLevel.ERROR:
                pretext = System.DateTime.Now.ToString(datetimeFormat) + " [ERROR]   ";
                break;
            case LogLevel.FATAL:
                pretext = System.DateTime.Now.ToString(datetimeFormat) + " [FATAL]   ";
                break;
            default:
                pretext = "";
                break;
        }

        WriteLine(pretext + text, level);
    }

    [System.Flags]
    public enum LogLevel {
        TRACE,
        DEBUG,
        INFO,
        WARNING,
        ERROR,
        FATAL
    }
}