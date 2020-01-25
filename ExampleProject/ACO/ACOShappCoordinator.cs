using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shapp;

namespace ExampleProject.ACO {
    internal class ACOShappCoordinator {

        private readonly List<JobDescriptor> descriptors = new List<JobDescriptor>();

        private static readonly int maxTime = ACOWithShappExample.duration;
        private readonly int numberOfWorkers = ACOWithShappExample.numberOfWorkers;
        readonly Queue<WorkerStatus> workersStatutes = new Queue<WorkerStatus>();
        readonly object workersStatusesLock = new object();
        private int numberOfConnectedWorkers = 0;
        private readonly AutoResetEvent allWorkersConnected = new AutoResetEvent(false);

        private int[] bestTrail = null;
        private double bestLength = double.MaxValue;

        public void Run() {
            InjectDelegatesForMessages();
            PrepareServer();
            StartWorkers();
            WaitForWorkersToBeAbleToCommunicate();
            InformWorkersAboutProcessingStarted();
            DoMainProcessingLoop();
        }

        private void InjectDelegatesForMessages() {
            WorkerStatus.OnReceive += (socket, workerStatus) => {
                C.log.Info("Received worker status from " + workerStatus.MyJobId);
                lock (workersStatusesLock) {
                    workersStatutes.Enqueue(workerStatus);
                }
            };
            Shapp.Communications.Protocol.HelloFromChild.OnReceive += (socket, helloFromChild) => {
                ++numberOfConnectedWorkers;
                if (numberOfConnectedWorkers == numberOfWorkers) {
                    allWorkersConnected.Set();
                }
            };
        }

        private void PrepareServer() {
            Shapp.CommunicatorWithChildren.InitializeServer();
        }

        private void StartWorkers() {
            for (int i = 0; i < numberOfWorkers; ++i) {
                string[] arguments = { i.ToString() };
                var submitter = new SelfSubmitter(null, arguments);
                var descriptor = submitter.Submit();
                descriptors.Add(descriptor);
            }
        }

        private void WaitForWorkersToBeAbleToCommunicate() {
            C.log.Info("Waiting for workers to connect");
            allWorkersConnected.WaitOne();
            C.log.Info("All workers connected");
        }

        private void InformWorkersAboutProcessingStarted() {
            var startProcessing = new StartProcessing();
            descriptors.ForEach((d) => d.Send(startProcessing));
        }

        private void DoMainProcessingLoop() {
            int time = 0;
            C.log.Info("Entering UpdateAnts - UpdatePheromones loop");
            while (time < maxTime) {
                List<WorkerStatus> statuses = GetStatusesWaitingForProcessing();
                if (statuses.Count >= 0) {
                    ProcessWorkerStatuses(statuses);
                }

                Thread.Sleep(1000);
                time += 1;
            }

            C.log.Info("Ending...");
            CommunicatorWithChildren.Stop();
            KillWorkers();
            C.log.Info("Best path:");
            if (bestTrail != null)
                ACOExample.Display(bestTrail);
            C.log.Info("Best path length: " + bestLength);
            C.log.Info("Stopping. Waiting for stdin...");
            Console.ReadKey();
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
            if (statuses.Count == 0)
                return;
            ProcessReceivedPheromones(statuses);
            ProcessReceivedTrails(statuses);
            SumUpAndShowNumberOfIterations(statuses);
        }

        private void SumUpAndShowNumberOfIterations(List<WorkerStatus> statuses) {
            int iterations = statuses.Select((s) => s.iterations).Sum();
            C.log.Info("Iterations done so far: " + iterations);
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
            C.log.Info("Pheromones update sent to all workers");
        }

        private void ProcessReceivedTrails(List<WorkerStatus> statuses) {
            var bestWorkerStatus = statuses.OrderBy((status) => status.bestPathLength).First();
            var bestLastLength = bestWorkerStatus.bestPathLength;
            C.log.Info("> best out of last " + bestLastLength.ToString("F1"));

            if (bestLastLength < bestLength) {
                bestLength = bestLastLength;
                bestTrail = bestWorkerStatus.bestTrail;
                C.log.Info("New best length of " + bestLength.ToString("F1"));
            }
        }

        private void KillWorkers() {
            descriptors.ForEach((d) => d.HardRemove());
        }
    }
}
