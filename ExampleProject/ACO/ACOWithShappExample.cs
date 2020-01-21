using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shapp;

namespace ExampleProject.ACO {
    class ACOWithShappExample {

        public static readonly int numCities = 100;
        public static readonly int numAntsPerWorker = 5;
        public static readonly int numberOfWorkers = Environment.ProcessorCount - 1;
        //public static readonly int numberOfWorkers = 1;
        public static readonly int workerStatusReportingPeriod = 100;

        public static void Run() {
            if (SelfSubmitter.AmIRootProcess()) {
                var coordinator = new ACOShappCoordinator();
                coordinator.Run();
            } else {
                var worker = new ACOShappWorker();
                worker.Run();
            }
        }
    }
}
