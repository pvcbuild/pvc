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
            this.services = this.CreateScriptCsEnv();
        }

        private ScriptServices CreateScriptCsEnv()
        {
            ILog logger = new Common.Logging.Simple.NoOpLogger();
            //logger = new Common.Logging.Simple.ConsoleOutLogger("[PVC]", Common.Logging.LogLevel.All, true, false, true, "hh:mm");
            var console = new ScriptCs.ScriptConsole();
            return new ScriptCs.ScriptServicesBuilder(console, logger)
                .ScriptEngine<ScriptCs.Engine.Roslyn.RoslynScriptInMemoryEngine>()
                .AssemblyResolver<PvcAssemblyResolver>()
                .PackageAssemblyResolver<PvcPackageAssemblyResolver>()
                .PackageContainer<PvcPackageContainer>()
                .InstallationProvider<PvcNugetInstallationProvider>()
                .ScriptName(this.fileName)
                .Build();
        }

        public void Execute(string commandName)
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            Console.Write(PvcConsole.Tag + " Loading pvc plugins and runtimes ...".DarkGrey());
            var packages = this.services.PackageAssemblyResolver.GetPackages(currentDirectory);
            this.services.InstallationProvider.Initialize();
            this.services.PackageInstaller.InstallPackages(packages);

            var assemblies = this.services.AssemblyResolver.GetAssemblyPaths(currentDirectory);
            var scriptPacks = this.services.ScriptPackResolver.GetPacks();
            Console.Write(" [".Grey() + "done".DarkGrey() + "]".Grey() + Environment.NewLine);

            this.services.Executor.Initialize(assemblies, scriptPacks);
            this.services.Executor.AddReferences(assemblies.ToArray());
            this.services.Executor.ImportNamespaces(PvcPlugin.registeredNamespaces.ToArray());

            var script =
                "var pvc = new PvcCore.Pvc();" +
                "{0}" +
                "pvc.Start(\"{1}\");";

            var result = this.services.Executor.ExecuteScript(string.Format(script, File.ReadAllText(this.fileName), commandName));
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
