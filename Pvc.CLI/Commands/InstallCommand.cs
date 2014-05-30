using Newtonsoft.Json.Linq;
using NuGet;
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

            var services = Executor.CreateScriptCsEnv("");
            services.InstallationProvider.Initialize();

            if (args.Length > 1)
            {
                var packageName = args[1];
                var packageRef = new PvcPackageReference(packageName);

                services.PackageInstaller.InstallPackages(new[] { packageRef }, true);
                this.GenerateConfigFile();
            }
            else
            {
                var packages = services.PackageAssemblyResolver.GetPackages(Directory.GetCurrentDirectory());
                services.PackageInstaller.InstallPackages(packages);
            }
        }

        internal void GenerateConfigFile()
        {
            var repository = new LocalPackageRepository(Path.Combine(Directory.GetCurrentDirectory(), ScriptCs.Pvc.Constants.PackagesFolder));
            var packages = repository.GetPackages().OrderByDescending(x => x.Version);

            var configFile = new JObject();
            var dependencyObject = new JObject();
            configFile.Add("dependencies", dependencyObject);

            foreach (var package in packages)
            {
                if (dependencyObject[package.Id] == null)
                    dependencyObject.Add(new JProperty(package.Id, package.Version.ToString()));
            }

            File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), ScriptCs.Pvc.Constants.PackagesFile), configFile.ToString());
        }
    }
}
