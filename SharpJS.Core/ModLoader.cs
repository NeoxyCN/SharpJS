using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SharpJS.Core
{
    /// <summary>
    /// Plugin orchestrator that manages JavaScript-based extensions
    /// </summary>
    public class PluginOrchestrator : IDisposable
    {
        private readonly ScriptEnvironment _scriptEnvironment;
        private readonly List<IPluginModule> _activePlugins;
        private readonly string _pluginsRootPath;
        private bool _disposed;

        public IReadOnlyCollection<IPluginModule> ActivePlugins => _activePlugins.AsReadOnly();

        public PluginOrchestrator(string pluginsRootPath)
        {
            _pluginsRootPath = pluginsRootPath ?? throw new ArgumentNullException(nameof(pluginsRootPath));
            _scriptEnvironment = new ScriptEnvironment();
            _activePlugins = new List<IPluginModule>();

            if (!Directory.Exists(_pluginsRootPath))
            {
                Directory.CreateDirectory(_pluginsRootPath);
            }
        }

        /// <summary>
        /// Makes a .NET object accessible from JavaScript plugins
        /// </summary>
        public void RegisterNativeApi(string apiName, object apiInstance)
        {
            EnsureNotDisposed();
            _scriptEnvironment.BindGlobalObject(apiName, apiInstance);
        }

        /// <summary>
        /// Scans and initializes all available plugins
        /// </summary>
        public void InitializeAllPlugins()
        {
            EnsureNotDisposed();

            var pluginDirectories = Directory.GetDirectories(_pluginsRootPath);
            
            foreach (var directory in pluginDirectories)
            {
                try
                {
                    LoadPluginFromDirectory(directory);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load plugin from {directory}: {ex.Message}");
                }
            }
        }

        private void LoadPluginFromDirectory(string directoryPath)
        {
            var configPath = Path.Combine(directoryPath, "plugin.json");
            
            if (!File.Exists(configPath))
            {
                Console.WriteLine($"No plugin.json found in {directoryPath}");
                return;
            }

            var configContent = File.ReadAllText(configPath);
            var config = JsonSerializer.Deserialize<PluginConfiguration>(configContent);

            if (config == null)
            {
                throw new InvalidOperationException("Failed to parse plugin.json");
            }

            var scriptPath = Path.Combine(directoryPath, config.MainScript ?? "index.js");
            
            var plugin = new JavaScriptPlugin(
                config.PluginId ?? Path.GetFileName(directoryPath),
                config.PluginName ?? "Unnamed Plugin",
                config.PluginVersion ?? "1.0.0",
                scriptPath,
                _scriptEnvironment
            );

            plugin.Initialize();
            _activePlugins.Add(plugin);
        }

        /// <summary>
        /// Initializes a specific plugin by identifier
        /// </summary>
        public void InitializePlugin(string pluginId)
        {
            EnsureNotDisposed();

            var pluginPath = Path.Combine(_pluginsRootPath, pluginId);
            if (!Directory.Exists(pluginPath))
            {
                throw new DirectoryNotFoundException($"Plugin directory not found: {pluginPath}");
            }

            LoadPluginFromDirectory(pluginPath);
        }

        /// <summary>
        /// Shuts down a specific plugin by identifier
        /// </summary>
        public void ShutdownPlugin(string pluginId)
        {
            EnsureNotDisposed();

            var plugin = _activePlugins.FirstOrDefault(p => p.Identifier == pluginId);
            if (plugin != null)
            {
                plugin.Shutdown();
                _activePlugins.Remove(plugin);
            }
        }

        /// <summary>
        /// Updates all active plugins - call this in your game loop
        /// </summary>
        public void UpdateAllPlugins()
        {
            EnsureNotDisposed();

            _scriptEnvironment.ProcessTasks();

            foreach (var plugin in _activePlugins.ToList())
            {
                try
                {
                    plugin.PerformUpdate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating plugin {plugin.DisplayName}: {ex.Message}");
                }
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(PluginOrchestrator));
        }

        public void Dispose()
        {
            if (_disposed) return;

            foreach (var plugin in _activePlugins.ToList())
            {
                try
                {
                    plugin.Shutdown();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error shutting down plugin {plugin.DisplayName}: {ex.Message}");
                }
            }

            _activePlugins.Clear();
            _scriptEnvironment?.Dispose();
            _disposed = true;
        }

        private class PluginConfiguration
        {
            public string? PluginId { get; set; }
            public string? PluginName { get; set; }
            public string? PluginVersion { get; set; }
            public string? MainScript { get; set; }
            public string? Description { get; set; }
            public string? Author { get; set; }
        }
    }
}
