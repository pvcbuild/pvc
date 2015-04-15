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
using ScriptCs.Contracts;
using ScriptCs;
using ScriptCs.Hosting;
using Newtonsoft.Json;

namespace Pvc.CLI.Commands
{
    public class TaskCommand : CommandBase
    {
        public TaskCommand()
        {
            var logger = new PvcNullLogger();
            var console = new ScriptCs.Hosting.ScriptConsole();
            var servicesBuilder = new PvcScriptServicesBuilder(console, logger)
                .ScriptName("TaskCommand")
                .ScriptHostFactory<PvcScriptHostFactory>();

            if (Type.GetType("Mono.Runtime") != null) this.IsMono = true;
            if (this.IsMono)
                servicesBuilder.ScriptEngine<ScriptCs.Engine.Mono.MonoScriptEngine>();
            else
                servicesBuilder.ScriptEngine<ScriptCs.Engine.Roslyn.RoslynScriptInMemoryEngine>();

            servicesBuilder.InitializationServices.GetFileSystem();

            this.ServicesBuilder = servicesBuilder;
        }

        internal IScriptServicesBuilder ServicesBuilder { get; private set; }

        internal bool IsMono { get; private set; }

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

                var _fileSystem = this.ServicesBuilder.InitializationServices.GetFileSystem();
                if (File.Exists(_fileSystem.PackagesFile) && !Directory.Exists(_fileSystem.PackagesFolder))
                {
                    if (taskName != ExportTasksName) Console.WriteLine("Packages folder missing. Restoring from {0} ...", _fileSystem.PackagesFile);
                    new InstallCommand().Execute(new string[] { }, new Dictionary<string, string>());
                }

                if (taskName == ExportTasksName)
                {
                    BuildScriptHost(pvcfile);
                    Console.Write(JsonConvert.SerializeObject(PvcScriptHost.PVCInstance.LoadedTasks.Select(x => new
                    {
                        Name = x.Name,
                        DependentTasks = x.DependentTasks
                    })));

                    return;
                }

                Console.WriteLine("Preparing to execute task '{0}' and dependencies from {1}", taskName.Magenta(), pvcfile.Cyan());
                ExecuteScript(pvcfile, taskName);
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

                if (stackTrace != null)
                {
                    var stackTraceLines = Regex.Split(stackTrace, "(\n|\r|\r\n)").Select(x => x.Trim());
                    foreach (var stackTraceLine in stackTraceLines)
                    {
                        if (stackTraceLine.Length > 0)
                            Console.WriteLine(stackTraceLine.DarkGrey());
                    }
                }

                var threadTask = PvcConsole.ThreadTask;
                PvcConsole.ThreadTask = null;

                Console.WriteLine("Task {0}failed with an exception", threadTask != null ? "'" + threadTask.Magenta() + "' " : "");
            }
        }

        private void ExecuteScript(string pvcfile, string taskName)
        {
            Console.Write("{0} Loading pvc plugins and runtimes ...".DarkGrey(), PvcConsole.Tag);
            BuildScriptHost(pvcfile);
            
            Console.Write(" [".Grey() + "done".DarkGrey() + "]".Grey() + Environment.NewLine);
            PvcScriptHost.RunTask(taskName);

            if (PvcWatcher.Items.Count > 0)
            {
                Console.WriteLine("");
                Console.WriteLine("Monitoring {0} for changes", "pipeline".Magenta());

                var currentDirectory = Directory.GetCurrentDirectory();
                PvcWatcher.ListenForChanges(currentDirectory);
            }
        }

        private void BuildScriptHost(string pvcfile)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var services = this.ServicesBuilder.Build();

            var assemblies = services.AssemblyResolver.GetAssemblyPaths(currentDirectory).Where(x => !x.EndsWith("Pvc.Core.dll")).ToList();
            var scriptPacks = services.ScriptPackResolver.GetPacks();
            services.Executor.Initialize(assemblies, scriptPacks);

            // Find test assemblies
            var testAssemblyDir = Path.Combine(currentDirectory, services.FileSystem.PackagesFolder, "bin");
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
            var pvcScript = File.ReadAllText(pvcfile);

            PvcScriptHost.PVCInstance = new PvcCore.Pvc();
            var result = services.Executor.ExecuteScript(pvcScript);

            if (result.CompileExceptionInfo != null)
                throw new PvcException(result.CompileExceptionInfo.SourceException);

            if (result.ExecuteExceptionInfo != null)
            {
                if (result.ExecuteExceptionInfo.SourceException.GetType() == typeof(PvcException))
                    throw new PvcException(result.ExecuteExceptionInfo.SourceException.InnerException ?? result.ExecuteExceptionInfo.SourceException);
                else
                    throw new PvcException(result.ExecuteExceptionInfo.SourceException);
            }
        }

        private static string ExportTasksName = "__exportTasks";
    }
}
