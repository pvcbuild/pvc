using ScriptCs.Contracts;
using ScriptCs.Hosting;
using ScriptCs.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pvc.CLI
{
    public class PvcScriptServicesBuilder : ScriptServicesBuilder
    {
        public PvcScriptServicesBuilder(IConsole console, ILog logger, IRuntimeServices runtimeServices = null, ITypeResolver typeResolver = null, IInitializationServices initializationServices = null)
            : base(
                console: console,
                logger: logger,
                runtimeServices: runtimeServices,
                typeResolver: typeResolver,
                initializationServices: initializationServices ?? BuildInitializationServices(logger)
            )
        {
            this.FileSystem<PvcFileSystem>()
                .PackageContainer<PvcPackageContainer>();
        }

        private static InitializationServices BuildInitializationServices(ILog logger)
        {
            return new InitializationServices(logger, new Dictionary<Type, object>
            {
                { typeof(IPackageContainer), typeof(PvcPackageContainer) },
                { typeof(IFileSystem), typeof(PvcFileSystem) }
            });
        }
    }
}
