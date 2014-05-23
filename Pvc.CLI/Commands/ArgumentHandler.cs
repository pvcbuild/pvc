using Pvc.CLI.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvc.CLI
{
    public class ArgumentHandler
    {
        private CommandBase[] commands = new CommandBase[] {
            new TaskCommand(),
            new InitCommand(),
            new InstallCommand(),
            new UpdateCommand()
        };

        public ArgumentHandler()
        {
            this.commands = commands.Concat(new[] { new UsageCommand(this.commands) }).ToArray();
        }

        public Action Parse(string[] args)
        {
            var parseResults = new FlagsParser().Parse(args);

            var flags = parseResults.Item2;
            args = parseResults.Item1;

            if (flags.ContainsKey("?"))
                return () => new UsageCommand(commands).Execute(args, flags);

            if (args.Length >= 1)
            {
                var arg = args[0];

                // is command?
                var command = commands.FirstOrDefault(x => x.IsTopLevel && x.Names.Any(y => y.IndexOf(arg) >= 0));
                if (command != null)
                {
                    return () => command.Execute(args, flags);
                }

                // must be a task
                flags.Add("taskName", arg);
                return () => new TaskCommand().Execute(args, flags);
            }

            return () => new UsageCommand(commands).Execute(args, flags);
        }
    }
}
