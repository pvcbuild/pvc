using Common.Logging;
using ScriptCs;
using ScriptCs.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            logger = new Common.Logging.Simple.ConsoleOutLogger("[PVC]", Common.Logging.LogLevel.All, true, false, true, "hh:mm");
            var console = new ScriptCs.ScriptConsole();
            return new ScriptCs.ScriptServicesBuilder(console, logger)
                .ScriptEngine<ScriptCs.Engine.Roslyn.RoslynScriptInMemoryEngine>()
                .ScriptName(this.fileName)
                .Repl(true)
                .Build();
        }

        public void Execute(string commandName)
        {
            this.services.Executor.Initialize(new string[] { }, new IScriptPack[] { });
            this.services.Executor.AddReferenceAndImportNamespaces(new Type[] {
                typeof(PvcCore.Pvc),
                typeof(PvcPlugins.PvcMSBuild),
                typeof(PvcPlugins.PvcLess)
            });

            var script =
                "var pvc = new Pvc();" +
                "{0}" +
                "pvc.Start(\"{1}\");";

            var result = this.services.Executor.ExecuteScript(string.Format(script, File.ReadAllText(this.fileName), commandName));
            if (result.ExecuteExceptionInfo != null)
            {
                throw result.ExecuteExceptionInfo.SourceException;
            }
        }
    }
}
