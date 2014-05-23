using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Edokan.KaiZen.Colors;

namespace Pvc.CLI.Commands
{
    public class UsageCommand : CommandBase
    {
        private CommandBase[] commands;

        public UsageCommand(CommandBase[] commands)
        {
            this.commands = commands;
        }

        internal override bool IsTopLevel
        {
            get { return true; }
        }

        internal override string[] Names
        {
            get { return new[] { "help", "?", "usage" }; }
        }

        internal override string Description
        {
            get { return "Prints help/usage information"; }
        }

        internal override void Execute(string[] args, Dictionary<string, string> flags)
        {
            List<Tuple<string, string>> commandDefs = new List<Tuple<string, string>>();

            // usage command needs to be added back in if it was accessed as 'pvc help'
            if (this.commands.Count(x => x.GetType() == typeof(UsageCommand)) == 0)
            {
                this.commands = this.commands.Concat(new[] { new UsageCommand(this.commands) }).ToArray();
            }

            foreach (var command in this.commands.Where(x => x.IsTopLevel))
            {
                commandDefs.Add(new Tuple<string, string>(
                    command.Names.First(),
                    string.Format("{0} {1}", command.Description, command.Names.Length > 1 ? "[" + string.Join(", ", command.Names.Skip(1)) + "]" : "")
                ));
            }

            var consoleWidth = Console.WindowWidth;
            var longestCommandLength = commandDefs.Select(x => x.Item1.Length).Max();
            var padding = 2;

            // Begin output
            Console.WriteLine("");
            Console.WriteLine(@"  _________                    _________  ");
            Console.WriteLine(@" |  ______ \__________________/ ______  |");
            Console.WriteLine(@" | |      \____________________/      | |");
            Console.WriteLine(@" | |                                  | |");
            Console.WriteLine(@" | |    ******   ***  ***   ******    | |");
            Console.WriteLine(@" | |    *******  ***  ***  *******    | |");
            Console.WriteLine(@" | |    ***  *** ***  *** ***         | |");
            Console.WriteLine(@" | |    ***  *** ***  *** ***         | |");
            Console.WriteLine(@" | |    ***  *** ***  *** ***         | |");
            Console.WriteLine(@" | |    *******  ***  *** ***         | |");
            Console.WriteLine(@" | |    ******   ***  *** ***         | |");
            Console.WriteLine(@" | |    ***       **  **  ***         | |");
            Console.WriteLine(@" | |    ***        ****    *******    | |");
            Console.WriteLine(@" | |    ***         **      ******    | |");
            Console.WriteLine(@" | |       ____________________       | |");
            Console.WriteLine(@" | |______/ __________________ \______| |");
            Console.WriteLine(@" |_________/                  \_________|");
            Console.WriteLine("");
            Console.WriteLine("");

            Console.WriteLine(" Usage:");
            Console.WriteLine("   {0} [{1}] [flags,...]", "pvc".Cyan(), "command".Magenta());
            Console.WriteLine("");
            Console.WriteLine(" Commands:");
            Console.WriteLine("");

            foreach (var commandDef in commandDefs)
            {
                var column1 = commandDef.Item1.PadRight(longestCommandLength + padding).ToLower();
                var column2 = commandDef.Item2;

                Console.WriteLine("   {0} {1}", column1.Magenta(), column2);
            }

            Console.WriteLine("");
        }
    }
}
