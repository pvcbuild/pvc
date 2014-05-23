using PvcCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pvc.CLI.Commands
{
    public class UpdateCommand : CommandBase
    {
        internal override bool IsTopLevel
        {
            get { return true; }
        }

        internal override string[] Names
        {
            get { return new[] { "update" }; }
        }

        internal override string Description
        {
            get { return "Update an installed plugin or NuGet package"; }
        }

        internal override void Execute(string[] args, Dictionary<string, string> flags)
        {
            if (args.Length > 1)
            {
                new InstallCommand().Execute(args, flags);
            }
        }
    }
}
