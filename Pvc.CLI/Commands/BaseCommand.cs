using ScriptCs.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvc.CLI.Commands
{
    public abstract class CommandBase
    {
        internal abstract bool IsTopLevel { get; }

        internal abstract string[] Names { get; }

        internal abstract string Description { get; }

        internal abstract void Execute(string[] args, Dictionary<string, string> flags);
    }
}
