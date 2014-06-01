using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edokan.KaiZen.Colors;
using PvcCore;
using System.Text.RegularExpressions;

namespace Pvc.CLI.Commands
{
    public class TaskCommand : CommandBase
    {
        internal override bool IsTopLevel
        {
            get { return false; }
        }

        internal override string[] Names
        {
            get { return new string[] { "Task" }; }
        }

        internal override string Description
        {
            get { return "Execute a task defined in the pvcfile"; }
        }

        internal override void Execute(string[] args, Dictionary<string, string> flags)
        {
            try
            {
                var taskName = flags.ContainsKey("taskname") ? flags["taskname"] : "default";
                var pvcfile = flags.ContainsKey("pvcfile") ? flags["pvcfile"] : "pvcfile";
                
                pvcfile = Path.GetFileNameWithoutExtension(pvcfile) + ".csx";
                if (!File.Exists(pvcfile))
                {
                    Console.WriteLine("Cannot find " + pvcfile.Cyan() + " in current directory.");
                    return;
                }

                if (File.Exists(ScriptCs.Pvc.Constants.PackagesFile) && !Directory.Exists(ScriptCs.Pvc.Constants.PackagesFolder))
                {
                    Console.WriteLine("Packages folder missing. Restoring from " + ScriptCs.Pvc.Constants.PackagesFile + " ...");
                    new InstallCommand().Execute(new string[] { }, new Dictionary<string, string>());
                }

                Console.WriteLine("Preparing to execute task '{0}' and dependencies from {1}", taskName.Magenta(), pvcfile.Cyan());

                var currentDirectory = Directory.GetCurrentDirectory();
                var executor = new Executor(Path.Combine(currentDirectory, pvcfile));
                executor.Execute(taskName);

                if (PvcWatcher.Items.Count > 0)
                {
                    Console.WriteLine("");
                    Console.WriteLine("Monitoring {0} for changes", "pipeline".Magenta());

                    PvcWatcher.ListenForChanges(currentDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("{0} thrown:", ex.InnerException != null ? ex.InnerException.GetType().ToString() : "Exception");
                Console.WriteLine("--------------------------");
                Console.WriteLine(ex.Message.Red());

                var stackTrace = ex.StackTrace;
                if (ex.InnerException != null)
                {
                    Console.WriteLine("--------------------------");
                    Console.WriteLine(ex.InnerException.Message.DarkRed());
                    stackTrace = ex.InnerException.StackTrace;
                }

                var stackTraceLines = Regex.Split(stackTrace, "(\n|\r|\r\n)").Select(x => x.Trim());
                foreach (var stackTraceLine in stackTraceLines)
                {
                    if (stackTraceLine.Length > 0)
                        Console.WriteLine(stackTraceLine.DarkGrey());
                }

                var threadTask = PvcConsole.ThreadTask;
                PvcConsole.ThreadTask = null;

                Console.WriteLine("Task {0}failed with an exception", threadTask != null ? "'" + threadTask.Magenta() + "' " : "");
            }
        }
    }
}
