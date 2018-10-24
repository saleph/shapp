using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExampleProject
{
    class Program
    {
        static void Main(string[] args)
        {
            Shapp.NewJobSubmitter newJobSubmitter = new Shapp.NewJobSubmitter
            {
                Command = "batch.py",
                LogFileName = "logjob.log",
                UserStandardOutputFileName = "output.out"
            };
            Console.Out.WriteLine(newJobSubmitter.SubmitNewJob());
            Shapp.JobDescriptor jobDescriptor = new Shapp.JobDescriptor(new Shapp.JobId("1.0"))
            {
                State = Shapp.JobState.RUNNING
            };
        }
    }
}
