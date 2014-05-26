using PvcCore;
using ScriptCs.Hosting.Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pvc.CLI.Commands
{
    public class InstallCommand : CommandBase
    {
        internal override bool IsTopLevel
        {
            get { return true; }
        }

        internal override string[] Names
        {
            get { return new[] { "install" }; }
        }

        internal override string Description
        {
            get { return "Install a plugin or NuGet package"; }
        }

        internal override void Execute(string[] args, Dictionary<string, string> flags)
        {
            if (!Directory.Exists(ScriptCs.Pvc.Constants.PackagesFolder))
                Directory.CreateDirectory(ScriptCs.Pvc.Constants.PackagesFolder);

            if (args.Length > 1)
            {
                var packageName = args[1];

                var nugetArgs = new List<string>(new[] {
                    "install",
                    packageName,
                    "-o",
                    ScriptCs.Pvc.Constants.PackagesFolder
                });

                var version = flags.ContainsKey("version") ? flags["version"] : null;
                if (version != null)
                    nugetArgs.AddRange(new[] {
                        "-version",
                        version
                    });

                var resultStreams = PvcUtil.StreamProcessExecution(PvcUtil.FindBinaryInPath("NuGet.exe", "NuGet.bat"), Directory.GetCurrentDirectory(), nugetArgs.ToArray());
                
                var outLine = string.Empty;
                var outStreamReader = new StreamReader(resultStreams.Item1);
                while ((outLine = outStreamReader.ReadLine()) != null)
                {
                    Console.WriteLine(outLine);
                }

                var errOutputReader = new StreamReader(resultStreams.Item2);
                while ((outLine = errOutputReader.ReadLine()) != null)
                {
                    Console.WriteLine(outLine);
                }

                // cleanup duplicate packages - we currently can't handle two versions of an unsigned assembly
                var moduleDirectories = Directory.EnumerateDirectories(Path.Combine(ScriptCs.Pvc.Constants.PackagesFolder))
                    .Where(x => Regex.IsMatch(x, @"\d+\.\d+$"))
                    .OrderByDescending(x => new DirectoryInfo(x).LastWriteTime);
                
                var keeperDirs = new List<string>();
                foreach (var dir in moduleDirectories)
                {
                    var dirIdentifier = Regex.Match(dir, @"(.*)(\.\d+){3,4}$").Groups[1].Value;
                    if (Regex.IsMatch(dirIdentifier, @"\.\d+$")) {
                        dirIdentifier = dirIdentifier.Substring(0, dirIdentifier.LastIndexOf('.'));
                    }

                    if (keeperDirs.Contains(dirIdentifier))
                    {
                        Directory.Delete(dir, true);
                        continue;
                    }

                    keeperDirs.Add(dirIdentifier);
                }
            }
        }
    }
}
