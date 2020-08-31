using ShortDash.Core.Plugins;
using System;
using System.Reflection;
using System.Runtime.Loader;

namespace ShortDash.Core.Services
{
    internal class PluginLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _resolver;

        public PluginLoadContext(string pluginPath)
        {
            _resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            // Load ShortDash.Plugins.Core from the currently process context
            if (assemblyName.Name == typeof(IShortDashAction).Assembly.GetName().Name) { return null; }
            // Load other assemblies
            var assemblyPath = _resolver.ResolveAssemblyToPath(assemblyName);
            return (assemblyPath != null) ? LoadFromAssemblyPath(assemblyPath) : null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var libraryPath = _resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            return (libraryPath != null) ? LoadUnmanagedDll(unmanagedDllName) : IntPtr.Zero;
        }
    }
}