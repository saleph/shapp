using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shapp;
using Shapp.Utils.WorkQueue;

namespace ExampleProject.QueueTest {

    public class InputData : IData {
        public List<int> numsToMultiply = new List<int>();
    }
    public class OutputData : IData {
        public List<int> multipliedNums = new List<int>();
        public int returnCode;
    }

    public class WorkQueueTest {

        private static int workersNumber = 1;
        private static List<JobDescriptor> children = new List<JobDescriptor>();

        public static void Run() {
            if (SelfSubmitter.AmIRootProcess()) {
                CommunicatorWithChildren.InitializeServer();
                SubmitChildren();
                RunRoot();
            } else if (SelfSubmitter.GetMyNestLevel() == 1) {
                RunChild();
            }
        }

        private static void SubmitChildren() {
            var submitter = new SelfSubmitter();
            for (var i = 0; i < workersNumber; ++i) {
                children.Add(submitter.Submit());
            }
        }

        private static void RunRoot() {
            WorkQueue.Initialize();

            // prepare input data
            var input = new InputData();
            input.numsToMultiply.Add(15);

            // put QueueTast for execution
            var task = WorkQueue.Put(new QueueTask() {
                functionToRun = ExampleFunc,
                InputData = input,
                Name = "Multiplying numbers - test"
            });

            // asynchronously hang on task promise. Will return when the soultion is delivered
            IData result = task.Result;
            if (result is OutputData output) {
                Console.WriteLine("Received answer: " + output.multipliedNums[0]);
            }
        }

        private static IData ExampleFunc(IData data) {
            var outputData = new OutputData();
            if (data is InputData input) {
                foreach (var number in input.numsToMultiply) {
                    outputData.multipliedNums.Add(number * 2);
                }
                outputData.returnCode = 0;
            } else {
                outputData.returnCode = -1;
            }
            return outputData;
        }

        private static void RunChild() {
            Worker.StartProcessingLoop();
        }
    }
}
