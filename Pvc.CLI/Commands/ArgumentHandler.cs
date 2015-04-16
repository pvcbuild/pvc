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

        public Action Parse(Tuple<string[], Dictionary<string, string>> parsedArgs)
        {
            var args = parsedArgs.Item1;
            var flags = parsedArgs.Item2;

            if (flags.ContainsKey("?"))
                return () => new UsageCommand(commands).Execute(args, flags);

            if (args.Length > 0)
            {
                var arg = args[0];

                // is command?
                var command = commands.FirstOrDefault(x => x.IsTopLevel && x.Names.Any(y => y.IndexOf(arg) >= 0));
                if (command != null)
                {
                    return () => command.Execute(args, flags);
                }

                // must be a task
                flags.Add("taskname", arg);
                return () => new TaskCommand().Execute(args, flags);
            }
            else
            {
                flags.Add("taskname", "default");
                return () => new TaskCommand().Execute(args, flags);
            }
        }
    }
}
