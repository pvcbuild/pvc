using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PvcCore
{
    public class Pvc
    {
        internal readonly ConcurrentDictionary<string, object> locks = new ConcurrentDictionary<string, object>();

        public readonly List<PvcTask> LoadedTasks = null;
        public readonly List<string> CompletedTasks = null;

        public Pvc()
        {
            this.LoadedTasks = new List<PvcTask>();
            this.CompletedTasks = new List<string>();
        }

        public PvcTask Task(string taskName, Action taskAction)
        {
            var task = new PvcTask(taskName, taskAction);
            this.LoadedTasks.Add(task);

            return task;
        }

        public PvcTask Task(string taskName, Action<Action> asyncTaskAction)
        {
            var task = new PvcTask(taskName, asyncTaskAction);
            this.LoadedTasks.Add(task);

            return task;
        }

        public PvcTask Task(string taskName)
        {
            return this.Task(taskName, () => { });
        }

        public PvcPipe Source(string directory, string pattern)
        {
            var filePaths = Directory.GetFiles(directory, pattern, SearchOption.AllDirectories);
            return Source(filePaths);
        }

        public PvcPipe Source(params string[] filePaths)
        {
            var streams = filePaths.Select(x => new PvcStream(new FileStream(x, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)).As(x));
            return new PvcPipe(streams);
        }

        public void Start(string taskName)
        {
            if (this.LoadedTasks.FirstOrDefault(x => x.taskName == taskName) == null)
                throw new PvcException("Task {0} not defined.", taskName);

            var dependencyGraph = new PvcDependencyGraph();
            foreach (var task in this.LoadedTasks)
            {
                dependencyGraph.AddDependencies(task.taskName, task.dependentTaskNames);
            }

            var executionPaths = new List<IEnumerable<PvcTask>>();
            var runPaths = dependencyGraph.GetPaths(taskName);
            foreach (var runPath in runPaths)
            {
                var runTasks = runPath.Select(x => this.LoadedTasks.First(y => y.taskName == x));
                executionPaths.Add(runTasks);
                foreach (var runTask in runTasks)
                {
                    // create locks
                    locks.AddOrUpdate(runTask.taskName, new { }, (s, o) => null);
                }
            }

            foreach (var executionPath in executionPaths.AsParallel())
            {
                this.RunTasks(executionPath.ToArray());
            }
        }

        private void RunTasks(PvcTask[] tasks)
        {
            for (int i = 0; i < tasks.Length; i++)
            {
                var task = tasks[i];

                // lock on task name to avoid multiple threads doing the work
                lock (locks[task.taskName])
                {
                    if (CompletedTasks.Contains(task.taskName))
                        continue;

                    if (task.isAsync)
                    {
                        // Start callback chain for async methods, lock on task
                        Monitor.Enter(locks[task.taskName]);
                        var callbackCalled = false;
                        task.ExecuteAsync(() =>
                        {
                            CompletedTasks.Add(task.taskName);
                            this.RunTasks(tasks.Skip(i + 1).ToArray());
                            callbackCalled = true;
                        });

                        // Keep app running
                        while (callbackCalled == false)
                        {
                            Thread.Sleep(50);
                        }

                        break;
                    }
                    else
                    {
                        task.Execute();
                        CompletedTasks.Add(task.taskName);
                    }
                }
            }
        }
    }
}
