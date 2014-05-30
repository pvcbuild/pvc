using Common.Logging;
using PvcCore;
using PvcPlugins;
using ScriptCs;
using ScriptCs.Contracts;
using ScriptCs.Hosting.Package;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Edokan.KaiZen.Colors;

namespace Pvc.CLI
{
    public class Executor
    {
        private readonly string fileName = null;
        private readonly ScriptServices services = null;

        public Executor(string fileName)
        {
            this.fileName = fileName;
            this.services = CreateScriptCsEnv(this.fileName);
        }

        internal static ScriptServices CreateScriptCsEnv(string scriptName)
        {
            ILog logger = new Common.Logging.Simple.NoOpLogger();
            //logger = new Common.Logging.Simple.ConsoleOutLogger("[PVC]", Common.Logging.LogLevel.All, true, false, true, "hh:mm");
            var console = new ScriptCs.ScriptConsole();

            return new ScriptCs.PvcScriptServicesBuilder(console, logger)
                .ScriptEngine<ScriptCs.Engine.Roslyn.RoslynScriptInMemoryEngine>()
                .AssemblyResolver<PvcAssemblyResolver>()
                .PackageAssemblyResolver<PvcPackageAssemblyResolver>()
                .PackageContainer<PvcPackageContainer>()
                .InstallationProvider<PvcNugetInstallationProvider>()
                .ScriptName(scriptName)
                .Build();
        }

        public void Execute(string commandName)
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            Console.Write(PvcConsole.Tag + " Loading pvc plugins and runtimes ...".DarkGrey());

            var assemblies = this.services.AssemblyResolver.GetAssemblyPaths(currentDirectory).Where(x => !x.EndsWith("Pvc.Core.dll")).ToList();
            var scriptPacks = this.services.ScriptPackResolver.GetPacks();
            Console.Write(" [".Grey() + "done".DarkGrey() + "]".Grey() + Environment.NewLine);

            this.services.Executor.Initialize(assemblies, scriptPacks);

            // Find test assemblies
            var testAssemblyDir = Path.Combine(currentDirectory, ScriptCs.Pvc.Constants.PackagesFolder, "bin");
            if (Directory.Exists(testAssemblyDir))
            {
                assemblies.AddRange(Directory.EnumerateFiles(testAssemblyDir, "*.dll", SearchOption.AllDirectories).Where(x => !x.EndsWith("Pvc.Core.dll")));
            }

            this.services.Executor.AddReferences(assemblies.ToArray());
            this.services.Executor.ImportNamespaces(PvcPlugin.registeredNamespaces.ToArray());

            // In case we haven't yet installed plugins but are running a simple task, inject this
            // assemblies reference to Pvc.Core
            if (assemblies.Count(x => x.EndsWith("Pvc.Core.dll")) == 0)
                this.services.Executor.AddReferenceAndImportNamespaces(new[] { typeof(PvcCore.Pvc) });

            var script =
                "{0}" + Environment.NewLine +
                "{1}" + Environment.NewLine +
                "var pvc = new PvcCore.Pvc();" + Environment.NewLine +
                "{2}" + Environment.NewLine +
                "pvc.Start(\"{3}\");";

            var pvcScriptLines = File.ReadAllLines(this.fileName);

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

            var compiledScript = string.Format(script, loadScriptLines, requireScriptLines, baseScriptLines, commandName);
            var result = this.services.Executor.ExecuteScript(compiledScript);
            if (result.CompileExceptionInfo != null)
                throw result.CompileExceptionInfo.SourceException;

            if (result.ExecuteExceptionInfo != null)
            {
                if (result.ExecuteExceptionInfo.SourceException.GetType() == typeof(PvcException))
                    throw result.ExecuteExceptionInfo.SourceException.InnerException ?? result.ExecuteExceptionInfo.SourceException;
                else
                    throw result.ExecuteExceptionInfo.SourceException;
            }
        }
    }
}
