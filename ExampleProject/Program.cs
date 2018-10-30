using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shapp;

namespace ExampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            //NewJobSubmitter newJobSubmitter = new Shapp.NewJobSubmitter
            //{
            //    Command = "batch.py",
            //    LogFileName = "logjob.log",
            //    UserStandardOutputFileName = "output.out"
            //};
            //JobDescriptor jobDescriptor = newJobSubmitter.SubmitNewJob();
            //Console.Out.WriteLine(">>>>>>>>>>>>>>>>>>> Job submitted");
            //jobDescriptor.JobStarted.WaitOne();
            //Console.Out.WriteLine(">>>>>>>>>>>>>>>>>>> Job started");
            //jobDescriptor.JobCompleted.WaitOne();
            //Console.Out.WriteLine(">>>>>>>>>>>>>>>>>>> Job completed");
            
            if (SelfSubmitter.AmIChildProcess())
            {
                // great, I was invoked on some remote worker
                return;
            }
            if (SelfSubmitter.AmIRootProcess())
            {
                Console.Out.WriteLine(Process.GetCurrentProcess().ProcessName);
                SelfSubmitter selfSubmitterHelper = new SelfSubmitter();
                var desc = selfSubmitterHelper.Submit();
                desc.JobStarted.WaitOne();
            }
        }
    }
}
