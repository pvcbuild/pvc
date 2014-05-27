using NuGet;
using ScriptCs.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvc.CLI
{
    class PvcPackageReference : IPackageReference
    {
        public PvcPackageReference(string packageId, string version = null)
        {
            if (version != null) {
                var semanticVersion = new SemanticVersion(version);
                this.Version = semanticVersion.Version;
                this.SpecialVersion = semanticVersion.SpecialVersion;
            }
            else
            {
                this.Version = new Version();
            }

            this.FrameworkName = VersionUtility.ParseFrameworkName("net45");
            this.PackageId = packageId;
        }

        public System.Runtime.Versioning.FrameworkName FrameworkName
        {
            get;
            set;
        }

        public string PackageId
        {
            get;
            set;
        }

        public string SpecialVersion
        {
            get;
            set;
        }

        public Version Version
        {
            get;
            set;
        }
    }
}
