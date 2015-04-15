using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcCore
{
    public class PvcTask
    {
        internal readonly bool isAsync = false;
        internal readonly string taskName = null;
        internal readonly Action taskAction = null;
        internal readonly Action<Action> asyncTaskAction = null;
        internal IEnumerable<string> dependentTaskNames = null;

        public string Name { get { return taskName; } }

        public IEnumerable<string> DependentTasks { get { return dependentTaskNames; } }

        public PvcTask(string taskName, Action taskAction)
        {
            this.taskName = taskName;
            this.taskAction = taskAction;
            this.dependentTaskNames = new string[] {};
        }

        public PvcTask(string taskName, Action<Action> asyncTaskAction)
        {
            this.taskName = taskName;
            this.dependentTaskNames = new string[] { };
            this.asyncTaskAction = asyncTaskAction;
            this.isAsync = true;
        }

        public void ExecuteAsync(Action callback)
        {
            PvcConsole.ThreadTask = this.taskName;
            this.asyncTaskAction(() =>
            {
                PvcConsole.ThreadTask = null;
                callback();
            });
        }

        public void Execute()
        {
            PvcConsole.ThreadTask = this.taskName;
            taskAction();
            PvcConsole.ThreadTask = null;
        }

        public PvcTask Requires(params string[] taskNames)
        {
            this.dependentTaskNames = taskNames;
            return this;
        }
    }
}
