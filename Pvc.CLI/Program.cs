using PvcCore;
using System;
using System.IO;
using Edokan.KaiZen.Colors;
using System.Text.RegularExpressions;
using System.Linq;

namespace Pvc.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            PvcConsole.Configure();

            try
            {
                var pvcfile = "pvcfile.csx";
                var taskName = args.Length >= 1 ? args[0] : "default";

                if (!File.Exists(pvcfile))
                {
                    Console.WriteLine("Cannot find " + "pvcfile.csx".Cyan() + " in current directory.");
                    return;
                }

                Console.WriteLine("Executing task '{0}' and dependencies from {1}", taskName.Magenta(), pvcfile.Cyan());

                var executor = new Executor(Path.Combine(Directory.GetCurrentDirectory(), pvcfile));
                executor.Execute(taskName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("{0} thrown:", ex.InnerException != null ? ex.InnerException.GetType().ToString() : "Exception");
                Console.WriteLine("--------------------------");
                Console.WriteLine(ex.Message.Red());
                var stackTraceLines = Regex.Split(ex.StackTrace, "(\n|\r|\r\n)").Select(x => x.Trim());
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
