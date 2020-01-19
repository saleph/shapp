using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shapp;

namespace ExampleProject.ACO {
    class ACOShappCoordinator {

        private readonly List<JobDescriptor> descriptors = new List<JobDescriptor>();
        private readonly AsynchronousServer server = new AsynchronousServer(C.DEFAULT_PORT);

        private static readonly int maxTime = 30;
        private readonly int numberOfWorkers = ACOUsingShappExample.numberOfWorkers;
        readonly Queue<WorkerStatus> workersStatutes = new Queue<WorkerStatus>();
        readonly object workersStatusesLock = new object();
        private int[] bestTrail = null;
        private double bestLength = double.MaxValue;

        public void Run() {
            InjectDelegatesForMessages();
            PrepareServer();
            StartWorkers();
            DoMainProcessingLoop();
        }

        private void InjectDelegatesForMessages() {
            WorkerStatus.OnReceive += (socket, workerStatus) => {
                Console.WriteLine("Received msg from " + socket.ToString());
                lock (workersStatusesLock) {
                    workersStatutes.Enqueue(workerStatus);
                }
            };
        }

        private void PrepareServer() {
            server.NewMessageReceivedEvent += (objectRecv, sock) => {
                if (objectRecv is ISystemMessage hello)
                    hello.Dispatch(sock);
            };
            server.Start();
        }

        private void StartWorkers() {
            var submitter = new SelfSubmitter();
            for (int i = 0; i < numberOfWorkers; ++i) {
                var descriptor = submitter.Submit();
                descriptors.Add(descriptor);
            }
        }

        private void DoMainProcessingLoop() {
            int time = 0;

            Console.WriteLine("\nEntering UpdateAnts - UpdatePheromones loop\n");
            while (time < maxTime) {
                List<WorkerStatus> statuses = GetStatusesWaitingForProcessing();
                if (statuses.Count >= 0) {
                    ProcessWorkerStatuses(statuses);
                }

                Thread.Sleep(1000);
                time += 1;
            }

            Console.WriteLine("Ending...");
            KillWorkers();
            Console.WriteLine("Best path:");
            ACOExample.Display(bestTrail);
            Console.WriteLine("\nBest path length: " + bestLength);
            Console.ReadLine();
        }

        private List<WorkerStatus> GetStatusesWaitingForProcessing() {
            List<WorkerStatus> statuses = new List<WorkerStatus>();
            lock (workersStatusesLock) {
                if (workersStatutes.Count >= numberOfWorkers) {
                    for (int i = 0; i < numberOfWorkers; ++i) {
                        statuses.Add(workersStatutes.Dequeue());
                    }
                }
            }
            return statuses;
        }

        private void ProcessWorkerStatuses(List<WorkerStatus> statuses) {
            ProcessReceivedPheromones(statuses);
            ProcessReceivedTrails(statuses);
        }

        private void ProcessReceivedPheromones(List<WorkerStatus> statuses) {
            IList<double[][]> pheromones = statuses.Select((status) => status.pheromones).ToList();
            var sumPheromones = ACOExample.InitPheromones(pheromones[0].Length, 0.0);
            for (int k = 0; k < pheromones.Count; k++) {
                for (int i = 0; i <= pheromones[k].Length - 1; i++) {
                    for (int j = i + 1; j <= pheromones[k][i].Length - 1; j++) {
                        sumPheromones[i][j] += pheromones[k][i][j];
                        if (sumPheromones[i][j] < 0.0001) {
                            sumPheromones[i][j] = 0.0001;
                        } else if (sumPheromones[i][j] > 100000.0) {
                            sumPheromones[i][j] = 100000.0;
                        }
                    }
                }
            }
            NotifyWorkersAboutNewPheromones(sumPheromones);
        }

        private void NotifyWorkersAboutNewPheromones(double[][] sumPheromones) {
            var pheromonesUpdate = new PheromonesUpdate() {
                pheromones = sumPheromones
            };
            descriptors.ForEach((descriptor) => {
                descriptor.Send(pheromonesUpdate);
            });
        }

        private void ProcessReceivedTrails(List<WorkerStatus> statuses) {
            var bestWorkerStatus = statuses.OrderBy((status) => status.bestPathLength).First();
            var bestLastLength = bestWorkerStatus.bestPathLength;
            Console.WriteLine("> best out of last " + bestLastLength.ToString("F1"));

            if (bestLastLength < bestLength) {
                bestLength = bestLastLength;
                bestTrail = bestWorkerStatus.bestTrail;
                Console.WriteLine("New best length of " + bestLength.ToString("F1"));
            }
        }

        private void KillWorkers() {
            descriptors.ForEach((d) => d.HardRemove());
        }
    }
}
