using Common.Logging;
using Newtonsoft.Json.Linq;
using NuGet;
using ScriptCs;
using ScriptCs.Contracts;
using ScriptCs.Hosting.Package;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IFileSystem = ScriptCs.Contracts.IFileSystem;

namespace Pvc.CLI
{
    public class PvcPackageContainer : PackageContainer
    {
        public PvcPackageContainer(IFileSystem fileSystem, ILog logger)
            : base(fileSystem, logger)
        {
        }

        public override void CreatePackageFile()
        {
            var packagesFile = Path.Combine(_fileSystem.CurrentDirectory, _fileSystem.PackagesFile);
            var packagesFolder = Path.Combine(_fileSystem.CurrentDirectory, _fileSystem.PackagesFolder);
            var repository = new LocalPackageRepository(packagesFolder);
            var packages = repository.GetPackages().OrderByDescending(x => x.Version);

            var configFile = new JObject();
            var dependencyObject = new JObject();
            configFile.Add("dependencies", dependencyObject);

            foreach (var package in packages)
            {
                if (dependencyObject[package.Id] == null)
                    dependencyObject.Add(new JProperty(package.Id, package.Version.ToString()));
            }

            File.WriteAllText(packagesFile, configFile.ToString());
        }

        public override IEnumerable<IPackageReference> FindPackageReferences(string path)
        {
            // if config file exists, we use those packages only
            if (_fileSystem.FileExists(path))
            {
                var configObject = JObject.Parse(File.ReadAllText(path));
                var packages = configObject["dependencies"];

                foreach (var package in packages)
                {
                    var packageProp = (JProperty)package;
                    yield return new ScriptCs.PackageReference(packageProp.Name, VersionUtility.ParseFrameworkName("net45"), packageProp.Value.ToString());
                }

                yield break;
            }
        }
    }
}
