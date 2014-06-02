using ScriptCs;
using ScriptCs.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvc.CLI
{
    public class PvcScriptHost : ScriptHost
    {
        public PvcScriptHost(IScriptPackManager scriptPackManager, ScriptEnvironment environment)
            : base(scriptPackManager, environment)
        {
        }

        internal static PvcCore.Pvc PVCInstance;

        public PvcCore.Pvc pvc { get { return PVCInstance; } }

        public static void RunTask(string taskName)
        {
            PVCInstance.Start(taskName);
        }
    }

    public class PvcScriptHostFactory : IScriptHostFactory
    {
        public IScriptHost CreateScriptHost(IScriptPackManager scriptPackManager, string[] scriptArgs)
        {
            return new PvcScriptHost(scriptPackManager, new ScriptEnvironment(scriptArgs));
        }
    }
}
