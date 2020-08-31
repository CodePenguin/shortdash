using ShortDash.Core.Plugins;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ShortDash.Core.Services
{
    public class PluginService
    {
        private readonly List<Type> pluginActions = new List<Type>();
        private readonly string pluginBasePath;

        public PluginService()
        {
            pluginBasePath = Path.Combine(Path.GetFullPath(Path.GetDirectoryName(typeof(PluginService).Assembly.Location)), "plugins");
            LoadPlugins();
        }

        public IEnumerable<Type> Actions => pluginActions;

        private void FindActions(Assembly plugin)
        {
            foreach (Type type in plugin.GetTypes())
            {
                if (!typeof(IShortDashAction).IsAssignableFrom(type)) { continue; }
                pluginActions.Add(type);
            }
        }

        private Assembly LoadPlugin(string pluginPath)
        {
            var loadContext = new PluginLoadContext(pluginPath);
            return loadContext.LoadFromAssemblyName(new AssemblyName(Path.GetFileNameWithoutExtension(pluginPath)));
        }

        private void LoadPlugins()
        {
            var options = new EnumerationOptions
            {
                IgnoreInaccessible = true,
                RecurseSubdirectories = true,
                MatchCasing = MatchCasing.CaseInsensitive
            };
            var pluginPaths = Directory.GetFiles(pluginBasePath, "ShortDash.Plugins.*.dll", options);
            foreach (var pluginPath in pluginPaths)
            {
                Assembly plugin = LoadPlugin(pluginPath);
                FindActions(plugin);
            }
        }
    }
}