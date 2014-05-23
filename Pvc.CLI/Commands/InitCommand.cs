using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvc.CLI.Commands
{
    public class InitCommand : CommandBase
    {
        internal override bool IsTopLevel
        {
            get { return true; }
        }

        internal override string[] Names
        {
            get { return new[] { "init" }; }
        }

        internal override string Description
        {
            get { return "Initialize project in this folder"; }
        }

        internal override void Execute(string[] args, Dictionary<string, string> flags)
        {
            Directory.CreateDirectory(ScriptCs.Pvc.Constants.PackagesFolder);
            Console.WriteLine("Added folder 'pvc-packages'");
        }
    }
}
