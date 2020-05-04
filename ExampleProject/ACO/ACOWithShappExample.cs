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


        public static readonly int numberOfWorkers = 3;

        public static readonly int duration = 10;
        public static readonly int workerStatusReportingPeriodInSeconds = 10;

        public static void Run(string[] argv) {
            if (SelfSubmitter.AmIRootProcess()) {
                var coordinator = new ACOShappCoordinator();
                coordinator.Run();
            } else {
                var worker = new ACOShappWorker();
                worker.Run(int.Parse(argv[0]));
            }
        }
    }
}
