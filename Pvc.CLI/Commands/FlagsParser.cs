using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pvc.CLI.Commands
{
    public class FlagsParser
    {
        public Tuple<string[], Dictionary<string, string>> Parse(string[] args)
        {
            var leftOverArgs = new List<string>();
            var flags = new Dictionary<string, string>();

            for (int i = 0; i < args.Length; i++)
            {
                var arg = args[i];
                var argRegex = Regex.Match(arg, @"^([\-\/]{1,2})([a-zA-Z0-9\?\+]+)([\=\:])*(.*)");
                if (argRegex.Success)
                {
                    var argName = argRegex.Groups[2].Value;
                    var argValue = argRegex.Groups[4].Value;

                    if (argValue == string.Empty && (args.Length - 1) >= (i + 1))
                    {
                        var nextArg = args[i + 1];
                        if (!Regex.IsMatch(nextArg, @"^[\-\/]{1,2}"))
                        {
                            // next 'arg' is actually value for this arg, skip it
                            argValue = nextArg;
                            i++;
                        }
                    }

                    flags.Add(argName.ToLower(), argValue);
                }
                else
                {
                    leftOverArgs.Add(arg);
                }
            }

            return new Tuple<string[], Dictionary<string, string>>(leftOverArgs.ToArray(), flags);
        }
    }
}
