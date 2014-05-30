using Autofac;
using Common.Logging;
using ScriptCs.Contracts;
using ScriptCs.Hosting.Package;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScriptCs
{
    public class PvcInitializationServices : ScriptCs.InitializationServices
    {
        public PvcInitializationServices(ILog logger, IDictionary<Type, object> overrides = null)
            : base(logger, overrides)
        {
        }

        protected override Autofac.IContainer CreateContainer()
        {
            var builder = new ContainerBuilder();
            this.Logger.Debug("Registering initialization services");
            builder.RegisterInstance<ILog>(this.Logger);
            builder.RegisterType<PvcScriptServicesBuilder>().As<IScriptServicesBuilder>();
            RegisterOverrideOrDefault<IFileSystem>(builder, b => b.RegisterType<FileSystem>().As<IFileSystem>().SingleInstance());
            RegisterOverrideOrDefault<IAssemblyUtility>(builder, b => b.RegisterType<AssemblyUtility>().As<IAssemblyUtility>().SingleInstance());
            RegisterOverrideOrDefault<IPackageContainer>(builder, b => b.RegisterType<PvcPackageContainer>().As<IPackageContainer>().SingleInstance());
            RegisterOverrideOrDefault<IPackageAssemblyResolver>(builder, b => b.RegisterType<PvcInitPackageAssemblyResolver>().As<IPackageAssemblyResolver>().SingleInstance());
            RegisterOverrideOrDefault<IAssemblyResolver>(builder, b => b.RegisterType<PvcAssemblyResolver>().As<IAssemblyResolver>().SingleInstance());
            RegisterOverrideOrDefault<IModuleLoader>(builder, b => b.RegisterType<ModuleLoader>().As<IModuleLoader>().SingleInstance());
            return builder.Build();
        }
    }
}
