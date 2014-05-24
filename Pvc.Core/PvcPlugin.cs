using PvcCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PvcPlugins
{
    public abstract class PvcPlugin
    {
        public static List<string> registeredNamespaces = new List<string>(new[] {
            "PvcCore",
            "PvcPlugins"
        });
        
        public virtual void RegisterNamespaces(params string[] namespaces)
        {
            namespaces = namespaces.Except(new[] { "Pvc" }.Concat(registeredNamespaces)).ToArray();
            PvcPlugin.registeredNamespaces.AddRange(namespaces);
        }

        public virtual string[] SupportedTags { get { return new[] { "*" }; } }

        public abstract IEnumerable<PvcStream> Execute(IEnumerable<PvcStream> inputStreams);
    }
}
