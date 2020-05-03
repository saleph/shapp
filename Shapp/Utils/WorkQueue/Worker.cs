using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shapp.Communications.Protocol;

namespace Shapp.Utils.WorkQueue {
    public class Worker {
        private readonly ManualResetEvent workerRegistered = new ManualResetEvent(false);
        private readonly ManualResetEvent killWorkerReceived = new ManualResetEvent(false);

        private readonly List<Task> tasks = new List<Task>();

        public void StartProcessingLoop() {
            Initialize();
            // wait for termination, tasks are scheduled asynchronously
            killWorkerReceived.WaitOne();
        }

        private void Initialize() {
            InjectReceptionCallbacks();
            CommunicatorToParent.Initialize();
            CommunicatorToParent.Send(new RegisterWorker() {
                JobId = JobEnvVariables.GetMyJobId()
            });
            workerRegistered.WaitOne();
        }

        private void InjectReceptionCallbacks() {
            RegisterWorkerConfirm.OnReceive += (sender, cnf) => {
                workerRegistered.Set();
            };
            QueueTask.OnReceive += (sender, queueTask) => {
                Task task = Task.Run(() => RunTask(queueTask));
                task.ContinueWith((t) => tasks.Remove(task));
                tasks.Add(task);
            };
            StopWorker.OnReceive += (sender, stopWorker) => {
                killWorkerReceived.Set();
            };
        }

        private void RunTask(QueueTask queueTask) {
            var returnData = queueTask.functionToRun.Invoke(queueTask.InputData);
            CommunicatorToParent.Send(new QueueTaskReturnValue() {
                Id = queueTask.Id,
                Name = queueTask.Name,
                OutputData = returnData
            });
        }
    }
}
