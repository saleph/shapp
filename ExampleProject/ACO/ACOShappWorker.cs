using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleProject.ACO {
    internal class ACOShappWorker {
        private static readonly int numCities = ACOWithShappExample.numCities;
        private static readonly int numAntsPerWorker = ACOWithShappExample.numAntsPerWorker;
        private static readonly int reportingPeriod = ACOWithShappExample.workerStatusReportingPeriod;
        // influence of pheromone on direction
        private static int alpha = 3;
        // influence of adjacent node distance
        private static int beta = 2;
        // pheromone decrease factor
        private static double rho = 0.01;
        // pheromone increase factor
        private static double Q = numCities / 6;


        private int[][] dists = ACOExample.MakeGraphDistances(numCities);
        private int[][] ants = ACOExample.InitAnts(numAntsPerWorker, numCities);
        private double[][] pheromones = ACOExample.InitPheromones(numCities);
        private object pheromonesLock = new object();

        int[] bestTrail = null;
        double bestLength = double.MaxValue;

        private AutoResetEvent processingCanBeStarted = new AutoResetEvent(false);

        public void Run() {
            Shapp.CommunicatorToParent.Initialize();
            SetupParametersForACO();
            InjectDelegateForPheromonesUpdate();
            InjectDelegateForStartProcessing();
            InitializeBestTrailAndLength();

            processingCanBeStarted.WaitOne();
            DoMainLoop();
        }

        private void InjectDelegateForStartProcessing() {
            StartProcessing.OnReceive += (socket, startProcessing) => {
                processingCanBeStarted.Set();
            };
        }

        private void SetupParametersForACO() {
            // I know. This is just PoC
            ACOExample.alpha = alpha;
            ACOExample.beta = beta;
        }

        private void InjectDelegateForPheromonesUpdate() {
            PheromonesUpdate.OnReceive += (socket, update) => {
                Console.WriteLine("Received PheromonesUpdate from " + socket.ToString());
                lock (pheromonesLock) {
                    pheromones = update.pheromones;
                }
            };
        }

        private void DoMainLoop() {
            int iteration = 1;
            Console.WriteLine("\nEntering UpdateAnts - UpdatePheromones loop\n");
            while (true) {
                lock (pheromonesLock) {
                    UpdateAnts();
                    UpdatePheromenes();
                }
                CheckForPossibleNewBestTrail(iteration);
                if (iteration % reportingPeriod == 0) {
                    ReportWorkerStatusToParent();
                }
                iteration++;
            }
        }

        private void ReportWorkerStatusToParent() {
            var status = new WorkerStatus() {
                bestPathLength = bestLength,
                bestTrail = bestTrail,
                pheromones = pheromones
            };
            Shapp.CommunicatorToParent.Send(status);
        }

        private void CheckForPossibleNewBestTrail(int iteration) {
            int[] currBestTrail = BestTrail();
            double currBestLength = Length(currBestTrail);
            Console.WriteLine("> length + " + currBestLength.ToString("F1"));

            if (currBestLength < bestLength) {
                bestLength = currBestLength;
                bestTrail = currBestTrail;
                Console.WriteLine("New best length of " + bestLength.ToString("F1") + " found at time " + iteration);
            }
        }

        private void UpdateAnts() {
            for (int k = 0; k < ants.Length; k++) {
                int start = ACOExample.random.Next(0, numCities);
                int[] newTrail = ACOExample.BuildTrail(k, start, pheromones, dists);
                ants[k] = newTrail;
            }
        }

        private void UpdatePheromenes() {
            for (int i = 0; i <= pheromones.Length - 1; i++) {
                for (int j = i + 1; j <= pheromones[i].Length - 1; j++) {
                    for (int k = 0; k <= ants.Length - 1; k++) {
                        // length of ant k trail
                        double decrease = (1.0 - rho) * pheromones[i][j];
                        double increase = 0.0;
                        if (ACOExample.EdgeInTrail(i, j, ants[k]) == true) {
                            double length = ACOExample.Length(ants[k], dists);
                            increase = (Q / length);
                        }

                        pheromones[i][j] = decrease + increase;

                        if (pheromones[i][j] < 0.0001) {
                            pheromones[i][j] = 0.0001;
                        } else if (pheromones[i][j] > 100000.0) {
                            pheromones[i][j] = 100000.0;
                        }

                        pheromones[j][i] = pheromones[i][j];
                    }
                }
            }
        }

        private void InitializeBestTrailAndLength() {
            bestTrail = BestTrail();
            bestLength = Length(bestTrail);
        }

        public int[] BestTrail() {
            double bestLength = Length(ants[0]);
            int idxBestLength = 0;
            for (int k = 1; k < ants.Length; k++) {
                double len = Length(ants[k]);
                if (len < bestLength) {
                    bestLength = len;
                    idxBestLength = k;
                }
            }
            int numCitiesInTrail = ants[0].Length;
            int[] currentBestTrail = new int[numCitiesInTrail];
            ants[idxBestLength].CopyTo(currentBestTrail, 0);
            return currentBestTrail;
        }

        private double Length(int[] path) {
            return ACOExample.Length(path, dists);
        }
    }
}
