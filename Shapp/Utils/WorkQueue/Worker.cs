using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shapp.Communications.Protocol;

namespace Shapp.Utils.WorkQueue {
    public static class Worker {
        private static readonly ManualResetEvent workerRegistered = new ManualResetEvent(false);
        private static readonly ManualResetEvent killWorkerReceived = new ManualResetEvent(false);

        private static readonly List<Task> tasks = new List<Task>();

        public static void StartProcessingLoop() {
            Initialize();
            // wait for termination, tasks are scheduled asynchronously
            C.log.Info("Waiting for tasks or termination");
            killWorkerReceived.WaitOne();
        }

        private static void Initialize() {
            InjectReceptionCallbacks();
            CommunicatorToParent.Initialize();

            C.log.Info("Sending RegisterWorker");
            CommunicatorToParent.Send(new RegisterWorker() {
                JobId = JobEnvVariables.GetMyJobId()
            });
            workerRegistered.WaitOne();
        }

        private static void InjectReceptionCallbacks() {
            RegisterWorkerConfirm.OnReceive += (sender, cnf) => {
                C.log.Info("RegisterWorkerConfirm received");
                workerRegistered.Set();
            };
            QueueTask.OnReceive += (sender, queueTask) => {
                C.log.Info("QueueTask received");
                Task task = Task.Run(() => RunTask(queueTask));
                task.ContinueWith((t) => tasks.Remove(task));
                tasks.Add(task);
            };
            StopWorker.OnReceive += (sender, stopWorker) => {
                C.log.Info("StopWorker received");
                killWorkerReceived.Set();
            };
        }

        private static void RunTask(QueueTask queueTask)
        {
            C.log.Info("Running the task");
            var returnData = queueTask.functionToRun.Invoke(queueTask.InputData);
            CommunicatorToParent.Send(new QueueTaskReturnValue() {
                Id = queueTask.Id,
                Name = queueTask.Name,
                OutputData = returnData
            });
        }
    }
}
