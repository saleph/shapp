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
            Shapp.NewJobSubmitter newJobSubmitter = new Shapp.NewJobSubmitter();
            newJobSubmitter.Command = "batch.py";
            newJobSubmitter.LogFileName = "logjob.log";
            newJobSubmitter.UserStandardOutputFileName = "output.out";
            System.Console.Out.WriteLine(newJobSubmitter.SubmitNewJob());
        }
    }
}
