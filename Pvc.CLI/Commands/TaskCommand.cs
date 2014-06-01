using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edokan.KaiZen.Colors;
using PvcCore;
using System.Text.RegularExpressions;
using Common.Logging;
using PvcPlugins;

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
                    Console.WriteLine("Cannot find {0} in current directory.", pvcfile.Cyan());
                    return;
                }

                if (File.Exists(ScriptCs.Constants.PackagesFile) && !Directory.Exists(ScriptCs.Constants.PackagesFolder))
                {
                    Console.WriteLine("Packages folder missing. Restoring from {0} ...", ScriptCs.Constants.PackagesFile);
                    new InstallCommand().Execute(new string[] { }, new Dictionary<string, string>());
                }

                Console.WriteLine("Preparing to execute task '{0}' and dependencies from {1}", taskName.Magenta(), pvcfile.Cyan());
                this.ExecuteScript(pvcfile, taskName);
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

        private void ExecuteScript(string pvcfile, string taskName)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var logger = new Common.Logging.Simple.NoOpLogger();
            var console = new ScriptCs.Hosting.ScriptConsole();
            var servicesBuilder = new ScriptCs.Hosting.ScriptServicesBuilder(console, logger);
            if (Type.GetType ("Mono.Runtime") != null)
                servicesBuilder.ScriptEngine<ScriptCs.Engine.Mono.MonoScriptEngine>();
            else
                servicesBuilder.ScriptEngine<ScriptCs.Engine.Roslyn.RoslynScriptInMemoryEngine>();

            var services = servicesBuilder.ScriptName(pvcfile).Build();
            Console.Write("{0} Loading pvc plugins and runtimes ...".DarkGrey(), PvcConsole.Tag);

            var assemblies = services.AssemblyResolver.GetAssemblyPaths(currentDirectory).Where(x => !x.EndsWith("Pvc.Core.dll")).ToList();
            var scriptPacks = services.ScriptPackResolver.GetPacks();
            Console.Write(" [".Grey() + "done".DarkGrey() + "]".Grey() + Environment.NewLine);

            services.Executor.Initialize(assemblies, scriptPacks);

            // Find test assemblies
            var testAssemblyDir = Path.Combine(currentDirectory, ScriptCs.Constants.PackagesFolder, "bin");
            if (Directory.Exists(testAssemblyDir))
            {
                assemblies.AddRange(Directory.EnumerateFiles(testAssemblyDir, "*.dll", SearchOption.AllDirectories).Where(x => !x.EndsWith("Pvc.Core.dll")));
            }

            services.Executor.AddReferences(assemblies.ToArray());

            // In case we haven't yet installed plugins but are running a simple task, inject this
            // assemblies reference to Pvc.Core
            if (assemblies.Count(x => x.EndsWith("Pvc.Core.dll")) == 0)
                services.Executor.AddReferences(new[] { typeof(PvcCore.Pvc).Assembly.Location });

            services.Executor.ImportNamespaces(PvcPlugin.registeredNamespaces.ToArray());

            var script =
                "{0}" + Environment.NewLine +
                "{1}" + Environment.NewLine +
                "var pvc = new PvcCore.Pvc();" + Environment.NewLine +
                "{2}" + Environment.NewLine +
                "pvc.Start(\"{3}\");";

            var pvcScriptLines = File.ReadAllLines(pvcfile);

            var loadScriptLines = new StringBuilder();
            var requireScriptLines = new StringBuilder();
            var baseScriptLines = new StringBuilder();
            foreach (var line in pvcScriptLines)
            {
                if (line.StartsWith("#load"))
                {
                    loadScriptLines.AppendLine(line);
                }
                else if (line.StartsWith("#r"))
                {
                    requireScriptLines.AppendLine(line);
                }
                else
                {
                    baseScriptLines.AppendLine(line);
                }
            }

            var compiledScript = string.Format(script, loadScriptLines, requireScriptLines, baseScriptLines, taskName);
            var result = services.Executor.ExecuteScript(compiledScript);
            if (result.CompileExceptionInfo != null)
                throw result.CompileExceptionInfo.SourceException;

            if (result.ExecuteExceptionInfo != null)
            {
                if (result.ExecuteExceptionInfo.SourceException.GetType() == typeof(PvcException))
                    throw result.ExecuteExceptionInfo.SourceException.InnerException ?? result.ExecuteExceptionInfo.SourceException;
                else
                    throw result.ExecuteExceptionInfo.SourceException;
            }

            if (PvcWatcher.Items.Count > 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Monitoring {0} for changes", "pipeline".Magenta());

                PvcWatcher.ListenForChanges(currentDirectory);
            }
        }
    }
}
