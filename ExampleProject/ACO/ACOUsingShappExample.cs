using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Shapp;

namespace ExampleProject.ACO {
    class ACOUsingShappExample {

        public static readonly int numCities = 100;
        public static readonly int numberOfWorkers = Environment.ProcessorCount - 1;

        public void Run() {
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
