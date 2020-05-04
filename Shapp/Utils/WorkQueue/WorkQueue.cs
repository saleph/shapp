using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Shapp.Communications.Protocol;

namespace Shapp.Utils.WorkQueue {
    public static class WorkQueue {

        private static readonly Queue<QueueTask> tasksQueue = new Queue<QueueTask>();
        private static readonly Dictionary<int, TaskCompletionSource<IData>> promises = new Dictionary<int, TaskCompletionSource<IData>>();
        private static readonly Dictionary<Socket, JobId> socketToJid = new Dictionary<Socket, JobId>();
        private static readonly Dictionary<JobId, WorkerStatus> workers = new Dictionary<JobId, WorkerStatus>();
        private static readonly Queue<JobId> freeWorkersQueue = new Queue<JobId>();

        public static void Initialize() {
            InjectCallbacks();
            CommunicatorWithChildren.InitializeServer();
        }

        public static void StopWorkers() {
            foreach (var worker in workers.Keys) {
                CommunicatorWithChildren.SendToChild(worker, new StopWorker());
                freeWorkersQueue.Clear();
                socketToJid.Clear();
            }
        }

        public static Task<IData> Put(QueueTask task) {
            tasksQueue.Enqueue(task);
            promises[task.Id] = new TaskCompletionSource<IData>();
            if (freeWorkersQueue.Count > 0) {
                DelegateTasksToWorkers();
            }
            return promises[task.Id].Task;
        }

        private static void DelegateTasksToWorkers() {
            while (freeWorkersQueue.Count > 0) {
                if (tasksQueue.Count == 0)
                    return;
                var task = tasksQueue.Dequeue();
                var worker = freeWorkersQueue.Dequeue();
                CommunicatorWithChildren.SendToChild(worker, task);
                workers[worker].IsBusy = true;
                workers[worker].TaskIdLastOrCurrentlyExecuted = task.Id;
                workers[worker].TaskNameLastOrCurrentlyExecuted = task.Name;
            }
        }

        private static void InjectCallbacks() {
            RegisterWorker.OnReceive += (sender, registerWorker) => {
                workers[registerWorker.JobId] = new WorkerStatus();
                freeWorkersQueue.Enqueue(registerWorker.JobId);
                socketToJid[sender] = registerWorker.JobId;
            };
            QueueTaskReturnValue.OnReceive += (sender, returnValue) => {
                promises[returnValue.Id].SetResult(returnValue.OutputData);
                var jid = socketToJid[sender];
                freeWorkersQueue.Enqueue(jid);
                workers[jid].IsBusy = false;
                DelegateTasksToWorkers();
            };
        }
    }

    internal class WorkerStatus {
        public bool IsBusy = false;
        public string TaskNameLastOrCurrentlyExecuted = "";
        public int TaskIdLastOrCurrentlyExecuted = -1;
    }
}
