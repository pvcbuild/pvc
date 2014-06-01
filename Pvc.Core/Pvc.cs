using Minimatch;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Humanizer;
using System.Text.RegularExpressions;
using Edokan.KaiZen.Colors;

namespace PvcCore
{
    public class Pvc
    {
        public readonly List<PvcTask> LoadedTasks;
        public readonly List<string> CompletedTasks;
        private readonly ConcurrentDictionary<string, object> locks;

        public Pvc()
        {
            this.LoadedTasks = new List<PvcTask>();
            this.CompletedTasks = new List<string>();

            this.locks = new ConcurrentDictionary<string, object>();
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

        public PvcPipe Source(params string[] inputs)
        {
            return new PvcPipe().Source(inputs);
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
                    locks.AddOrUpdate(runTask.taskName, new { }, (s, o) => o);
                }
            }

            try
            {
                foreach (var executionPath in executionPaths.AsParallel())
                {
                    this.RunTasks(executionPath.ToArray());
                }
            }
            catch (AggregateException ex)
            {
                throw new PvcException(ex.InnerException);
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
                    try
                    {
                        if (CompletedTasks.Contains(task.taskName))
                            continue;

                        if (task.isAsync)
                        {
                            // Start callback chain for async methods, lock on task
                            Monitor.Enter(locks[task.taskName]);
                            var callbackCalled = false;

                            var stopwatch = this.StartTaskStatus(task.taskName);
                            task.ExecuteAsync(() =>
                            {
                                this.FinishTaskStatus(task.taskName, stopwatch);
                                CompletedTasks.Add(task.taskName);

                                if (i != tasks.Length - 1)
                                    this.RunTasks(tasks.Skip(i + 1).ToArray());

                                callbackCalled = true;
                            });

                            // Keep app running while async task completes
                            while (callbackCalled == false) { }
                            break;
                        }
                        else
                        {
                            var stopwatch = this.StartTaskStatus(task.taskName);

                            task.Execute();
                            CompletedTasks.Add(task.taskName);

                            this.FinishTaskStatus(task.taskName, stopwatch);
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new PvcException(ex);
                    }
                }
            }
        }

        private Stopwatch StartTaskStatus(string taskName)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            Console.WriteLine("Starting '{0}' ...", taskName.Magenta());

            return stopwatch;
        }

        private void FinishTaskStatus(string taskName, Stopwatch stopwatch)
        {
            stopwatch.Stop();
            Console.WriteLine("Finished '{0}' in {1}", taskName.Magenta(), stopwatch.Elapsed.Humanize().White());
        }
    }
}
