using PvcCore;
using System;
using System.IO;
using Edokan.KaiZen.Colors;
using System.Text.RegularExpressions;
using System.Linq;
using Pvc.CLI.Commands;

namespace Pvc.CLI
{
    class Program
    {
        static void Main(string[] args)
        {
            var parseResults = new FlagsParser().Parse(args);
            PvcConsole.Configure(!parseResults.Item2.ContainsKey("ansi"));

            var task = new ArgumentHandler().Parse(parseResults);
            task();
        }
    }
}
