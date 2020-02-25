using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Data;
using System.Diagnostics;
using System.Threading;
using Shapp;

namespace ExampleProject {

    // https://docs.microsoft.com/en-us/archive/msdn-magazine/2012/february/test-run-ant-colony-optimization
    // By James McCaffrey | February 2012
    // Adaptation for multithreading by Tomasz Galecki (@saleph)
    // Demo of Ant Colony Optimization (ACO) solving a Traveling Salesman Problem (TSP).
    // There are many variations of ACO; this is just one approach.
    // The problem to solve has a program defined number of cities. We assume that every
    // city is connected to every other city. The distance between cities is artificially
    // set so that the distance between any two cities is a random value between 1 and 8
    // Cities wrap, so if there are 20 cities then D(0,19) = D(19,0).
    // Free parameters are alpha, beta, rho, and Q. Hard-coded constants limit min and max
    // values of pheromones.

    class ACOExample {

        public static Random random = new Random(0);
        public static List<Random> randoms = new List<Random>();


        private static int numCities = 500;
        private static int numAnts = 5;
        private static int maxTime = 240;
        // left one core for accountability - mostly counting the best distances
        private static int numThreads = 120;
        private static int synchronisationPeriod = 10;
        private static int bestLengthCheckPeriod = 1;
        static ACOExample() {
            for (int i = 0; i < numThreads; i++) {
                randoms.Add(new Random(i + 1));
            }
        }

        // influence of pheromone on direction
        public static int alpha = 3;
        // influence of adjacent node distance
        public static int beta = 2;

        // pheromone decrease factor
        public static double rho = 0.05;
        // pheromone increase factor
        public static double Q = numCities / 6;

        public static void Run() {

            IList<int[][]> ants = new SynchronizedCollection<int[][]>();
            IList<double[][]> pheromones = new SynchronizedCollection<double[][]>();
            IList<object> locks = new SynchronizedCollection<object>();
            IList<int> iterations = new SynchronizedCollection<int>();
            try {
                C.log.Info("Begin Ant Colony Optimization demo");

                C.log.Info("Number cities in problem = " + numCities);

                C.log.Info("Number ants = " + numAnts);
                C.log.Info("Maximum time = " + maxTime);

                C.log.Info("Alpha (pheromone influence) = " + alpha);
                C.log.Info("Beta (local node influence) = " + beta);
                C.log.Info("Rho (pheromone evaporation coefficient) = " + rho.ToString("F2"));
                C.log.Info("Q (pheromone deposit factor) = " + Q.ToString("F2"));

                C.log.Info("Initialing dummy graph distances");
                int[][] dists = MakeGraphDistances(numCities);

                C.log.Info("Initialing ants to random trails");

                for (int i = 0; i < numThreads; ++i) {
                    ants.Add(InitAnts(numAnts, numCities));
                    locks.Add(new object());
                    iterations.Add(0);
                }

                // initialize ants to random trails
                ShowAnts(ants, dists);

                int[] bestTrail = ACOExample.BestTrail(ants, dists, locks);
                // determine the best initial trail
                double bestLength = Length(bestTrail, dists);
                // the length of the best trail

                Console.Write("Best initial trail length: " + bestLength.ToString("F1") + "");
                //Display(bestTrail);

                C.log.Info("Initializing pheromones on trails");
                for (int i = 0; i < numThreads; ++i) {
                    pheromones.Add(InitPheromones(numCities));
                }

                List<Thread> threads = new List<Thread>();
                for (int i = 0; i < numThreads; ++i) {
                    threads.Add(new Thread(new ParameterizedThreadStart((threadIdxParam) => {
                        int threadIdx = (int)threadIdxParam;
                        C.log.Info("Sarting as " + threadIdx);
                        while (true) {
                            UpdateAnts(ants[threadIdx], pheromones[threadIdx], dists, locks[threadIdx], randoms[threadIdx]);
                            UpdatePheromones(pheromones[threadIdx], ants[threadIdx], dists, locks[threadIdx]);
                            lock (locks[threadIdx]) {
                                iterations[threadIdx]++;
                            }
                        }
                    })));
                }
                for (int i = 0; i < numThreads; ++i) {
                    threads[i].Start(i);
                }

                int startTime = GetTime();
                int iteration = 0;
                int lastSynchronisation = GetTime();
                C.log.Info("Entering UpdateAnts - UpdatePheromones loop");
                while (GetTime() - startTime < maxTime) {
                    //for (int i = 0; i < numThreads; ++i)
                    //    UpdateAnts(ants[i], pheromones[i], dists, locks[i]);
                    //for (int i = 0; i < numThreads; ++i)
                    //    UpdatePheromones(pheromones[i], ants[i], dists, locks[i]);

                    if (GetTime() - lastSynchronisation >= synchronisationPeriod) {
                        SynchronizePheromones(pheromones, locks);
                    }

                    if (GetTime() - bestLengthCheckPeriod >= 1) {
                        int[] currBestTrail = ACOExample.BestTrail(ants, dists, locks);
                        double currBestLength = Length(currBestTrail, dists);
                        C.log.Info("length " + currBestLength.ToString("F1"));
                        int sumOfIterations = 0;
                        for (int i = 0; i < numThreads; ++i) {
                            lock (locks[i]) {
                                sumOfIterations += iterations[i];
                            }
                        }
                        C.log.Info("iterations " + sumOfIterations);

                        if (currBestLength < bestLength) {
                            bestLength = currBestLength;
                            bestTrail = currBestTrail;
                            C.log.Info("New best length of " + bestLength.ToString("F1") + " found at time " + iteration);
                        }
                    }
                    Thread.Sleep(500);
                    iteration += 1;
                }

                C.log.Info("Time complete");

                C.log.Info("Best trail found:");
                Display(bestTrail);
                C.log.Info("Length of best trail found: " + bestLength.ToString("F1"));

                C.log.Info("End Ant Colony Optimization demo");
                Console.Read();
            } catch (Exception ex) {
                C.log.Info(ex.Message);
            }

        }

        private static int GetTime() {
            TimeSpan t = (DateTime.UtcNow - new DateTime(1970, 1, 1));
            return (int)t.TotalSeconds;
        }

        // --------------------------------------------------------------------------------------------

        public static int[][] InitAnts(int numAnts, int numCities) {
            random = new Random(0);
            int[][] ants = new int[numAnts][];
            for (int k = 0; k <= numAnts - 1; k++) {
                int start = random.Next(0, numCities);
                ants[k] = RandomTrail(start, numCities);
            }
            return ants;
        }

        private static int[] RandomTrail(int start, int numCities) {
            // helper for InitAnts
            int[] trail = new int[numCities];

            // sequential
            for (int i = 0; i <= numCities - 1; i++) {
                trail[i] = i;
            }

            // Fisher-Yates shuffle
            for (int i = 0; i <= numCities - 1; i++) {
                int r = random.Next(i, numCities);
                int tmp = trail[r];
                trail[r] = trail[i];
                trail[i] = tmp;
            }

            int idx = IndexOfTarget(trail, start);
            // put start at [0]
            int temp = trail[0];
            trail[0] = trail[idx];
            trail[idx] = temp;

            return trail;
        }

        private static int IndexOfTarget(int[] trail, int target) {
            // helper for RandomTrail
            for (int i = 0; i <= trail.Length - 1; i++) {
                if (trail[i] == target) {
                    return i;
                }
            }
            throw new Exception("Target not found in IndexOfTarget");
        }

        public static double Length(int[] trail, int[][] dists) {
            // total length of a trail
            double result = 0.0;
            for (int i = 0; i <= trail.Length - 2; i++) {
                result += Distance(trail[i], trail[i + 1], dists);
            }
            return result;
        }

        // -------------------------------------------------------------------------------------------- 

        public static int[] BestTrail(IList<int[][]> antss, int[][] dists, IList<object> threadLocks) {
            // best trail has shortest total length
            double bestLength = Length(antss[0][0], dists);
            int idxBestLengthForOuter = 0;
            int idxBestLength = 0;
            int j = 0;
            for (int i = 0; i < antss.Count; i++) {
                lock (threadLocks[i]) {
                    var ants = antss[i];
                    for (int k = 0; k <= ants.Length - 1; k++) {
                        double len = Length(ants[k], dists);
                        if (len < bestLength) {
                            bestLength = len;
                            idxBestLength = k;
                            idxBestLengthForOuter = j;
                        }
                    }
                    ++j;
                }
            }
            int numCities = antss[0][0].Length;
            //INSTANT VB NOTE: The local variable bestTrail was renamed since Visual Basic will not allow local variables with the same name as their enclosing function or property:
            int[] bestTrail_Renamed = new int[numCities];
            antss[idxBestLengthForOuter][idxBestLength].CopyTo(bestTrail_Renamed, 0);
            return bestTrail_Renamed;
        }

        // --------------------------------------------------------------------------------------------

        public static double[][] InitPheromones(int numCities, double initialValue = 0.01) {
            double[][] pheromones = new double[numCities][];
            for (int i = 0; i <= numCities - 1; i++) {
                pheromones[i] = new double[numCities];
            }
            for (int i = 0; i <= pheromones.Length - 1; i++) {
                for (int j = 0; j <= pheromones[i].Length - 1; j++) {
                    pheromones[i][j] = initialValue;
                    // otherwise first call to UpdateAnts -> BuiuldTrail -> NextNode -> MoveProbs => all 0.0 => throws
                }
            }
            return pheromones;
        }

        // --------------------------------------------------------------------------------------------

        private static void UpdateAnts(int[][] ants, double[][] pheromones, int[][] dists, object threadLock, Random random) {
            lock (threadLock) {
                int numCities = pheromones.Length;
                for (int k = 0; k < ants.Length; k++) {
                    int start = random.Next(0, numCities);
                    int[] newTrail = BuildTrail(k, start, pheromones, dists, random);
                    ants[k] = newTrail;
                }
            }
        }

        public static int[] BuildTrail(int k, int start, double[][] pheromones, int[][] dists, Random random) {
            int numCities = pheromones.Length;
            int[] trail = new int[numCities];
            bool[] visited = new bool[numCities];
            trail[0] = start;
            visited[start] = true;
            for (int i = 0; i <= numCities - 2; i++) {
                int cityX = trail[i];
                int next = NextCity(k, cityX, visited, pheromones, dists, random);
                trail[i + 1] = next;
                visited[next] = true;
            }
            return trail;
        }

        private static int NextCity(int k, int cityX, bool[] visited, double[][] pheromones, int[][] dists, Random random) {
            // for ant k (with visited[]), at nodeX, what is next node in trail?
            double[] probs = MoveProbs(k, cityX, visited, pheromones, dists);

            double[] cumul = new double[probs.Length + 1];
            for (int i = 0; i <= probs.Length - 1; i++) {
                cumul[i + 1] = cumul[i] + probs[i];
                // consider setting cumul[cuml.Length-1] to 1.00
            }

            double p = random.NextDouble();

            for (int i = 0; i <= cumul.Length - 2; i++) {
                if (p >= cumul[i] && p < cumul[i + 1]) {
                    return i;
                }
            }
            throw new Exception("Failure to return valid city in NextCity");
        }

        private static double[] MoveProbs(int k, int cityX, bool[] visited, double[][] pheromones, int[][] dists) {
            // for ant k, located at nodeX, with visited[], return the prob of moving to each city
            int numCities = pheromones.Length;
            double[] taueta = new double[numCities];
            // inclues cityX and visited cities
            double sum = 0.0;
            // sum of all tauetas
            // i is the adjacent city
            for (int i = 0; i <= taueta.Length - 1; i++) {
                if (i == cityX) {
                    taueta[i] = 0.0;
                    // prob of moving to self is 0
                } else if (visited[i] == true) {
                    taueta[i] = 0.0;
                    // prob of moving to a visited city is 0
                } else {
                    taueta[i] = Math.Pow(pheromones[cityX][i], alpha) * Math.Pow((1.0 / Distance(cityX, i, dists)), beta);
                    // could be huge when pheromone[][] is big
                    if (taueta[i] < 0.0001) {
                        taueta[i] = 0.0001;
                    } else if (taueta[i] > (double.MaxValue / (numCities * 100))) {
                        taueta[i] = double.MaxValue / (numCities * 100);
                    }
                }
                sum += taueta[i];
            }

            double[] probs = new double[numCities];
            for (int i = 0; i <= probs.Length - 1; i++) {
                probs[i] = taueta[i] / sum;
                // big trouble if sum = 0.0
            }
            return probs;
        }

        // --------------------------------------------------------------------------------------------

        private static void UpdatePheromones(double[][] pheromones, int[][] ants, int[][] dists, object threadLock) {
            lock (threadLock) {
                for (int i = 0; i <= pheromones.Length - 1; i++) {
                    for (int j = i + 1; j <= pheromones[i].Length - 1; j++) {
                        for (int k = 0; k <= ants.Length - 1; k++) {
                            // length of ant k trail
                            double decrease = (1.0 - rho) * pheromones[i][j];
                            double increase = 0.0;
                            if (EdgeInTrail(i, j, ants[k]) == true) {
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
        }

        private static void SynchronizePheromones(IList<double[][]> pheromones, IList<object> theadLocks) {
            var sumPheromones = InitPheromones(pheromones[0].Length, 0.0);
            for (int k = 0; k < pheromones.Count; k++) {
                lock (theadLocks[k]) {
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
            }
            for (int k = 0; k < pheromones.Count; k++) {
                lock (theadLocks[k]) {
                    for (int i = 0; i <= pheromones[k].Length - 1; i++) {
                        for (int j = i + 1; j <= pheromones[k][i].Length - 1; j++) {
                            pheromones[k][i][j] = sumPheromones[i][j];
                        }
                    }
                }
            }
        }

        public static bool EdgeInTrail(int cityX, int cityY, int[] trail) {
            // are cityX and cityY adjacent to each other in trail[]?
            int lastIndex = trail.Length - 1;
            int idx = IndexOfTarget(trail, cityX);

            if (idx == 0 && trail[1] == cityY) {
                return true;
            } else if (idx == 0 && trail[lastIndex] == cityY) {
                return true;
            } else if (idx == 0) {
                return false;
            } else if (idx == lastIndex && trail[lastIndex - 1] == cityY) {
                return true;
            } else if (idx == lastIndex && trail[0] == cityY) {
                return true;
            } else if (idx == lastIndex) {
                return false;
            } else if (trail[idx - 1] == cityY) {
                return true;
            } else if (trail[idx + 1] == cityY) {
                return true;
            } else {
                return false;
            }
        }


        // --------------------------------------------------------------------------------------------

        public static int[][] MakeGraphDistances(int numCities) {
            random = new Random(0);
            int[][] dists = new int[numCities][];
            for (int i = 0; i <= dists.Length - 1; i++) {
                dists[i] = new int[numCities];
            }
            for (int i = 0; i <= numCities - 1; i++) {
                for (int j = i + 1; j <= numCities - 1; j++) {
                    int d = random.Next(1, 9);
                    // [1,8]
                    dists[i][j] = d;
                    dists[j][i] = d;
                }
            }
            return dists;
        }

        private static double Distance(int cityX, int cityY, int[][] dists) {
            return dists[cityX][cityY];
        }

        // --------------------------------------------------------------------------------------------

        public static void Display(int[] trail) {
            string output = "";
            for (int i = 0; i <= trail.Length - 1; i++) {
                output += trail[i] + " ";
                if (i > 0 && i % 20 == 0) {
                    output += "\n";
                }
            }
            output += "\n";
            C.log.Info(output);
        }


        private static void ShowAnts(IList<int[][]> antss, int[][] dists) {
            string output = "";
            foreach (var ants in antss)
                for (int i = 0; i <= ants.Length - 1; i++) {
                    output += (i + ": [ ");

                    for (int j = 0; j <= 3; j++) {
                        output += (ants[i][j] + " ");
                    }

                    output += (". . . ");

                    for (int j = ants[i].Length - 4; j <= ants[i].Length - 1; j++) {
                        output += (ants[i][j] + " ");
                    }

                    output += ("] len = ");
                    double len = Length(ants[i], dists);
                    output += (len.ToString("F1"));
                    output += ("\n");
                }
            C.log.Info(output);
        }

        private static void Display(double[][] pheromones) {
            string output = "";
            for (int i = 0; i <= pheromones.Length - 1; i++) {
                output += (i + ": ");
                for (int j = 0; j <= pheromones[i].Length - 1; j++) {
                    output += (pheromones[i][j].ToString("F4").PadLeft(8) + " ");
                }
                output += ("\n");
            }
            C.log.Info(output);
        }

    }
    // class AntColonyProgram
}
