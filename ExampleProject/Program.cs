using System;
using System.Collections.Generic;
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
            //JobId id = newJobSubmitter.SubmitNewJob();
            JobDescriptor jobDescriptor = new JobDescriptor(new JobId("1.0"))
            {
                State = JobState.RUNNING
            };
            JobStateFetcher jobStateFetcher = new JobStateFetcher();
            jobDescriptor.State = jobStateFetcher.GetJobState(new JobId(19, 0));
        }
    }
}
