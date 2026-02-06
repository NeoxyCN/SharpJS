using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace SharpJS.Core
{
    /// <summary>
    /// Manages loading, unloading, and updating of game mods
    /// </summary>
    public class ModLoader : IDisposable
    {
        private readonly JsRuntime _runtime;
        private readonly List<IMod> _loadedMods;
        private readonly string _modsDirectory;
        private bool _disposed;

        public IReadOnlyList<IMod> LoadedMods => _loadedMods.AsReadOnly();

        public ModLoader(string modsDirectory)
        {
            _modsDirectory = modsDirectory ?? throw new ArgumentNullException(nameof(modsDirectory));
            _runtime = new JsRuntime();
            _loadedMods = new List<IMod>();

            // Create mods directory if it doesn't exist
            if (!Directory.Exists(_modsDirectory))
            {
                Directory.CreateDirectory(_modsDirectory);
            }
        }

        /// <summary>
        /// Exposes a C# object to JavaScript mods
        /// </summary>
        /// <param name="name">Name to expose as</param>
        /// <param name="obj">Object to expose</param>
        public void ExposeApi(string name, object obj)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ModLoader));

            _runtime.RegisterObject(name, obj);
        }

        /// <summary>
        /// Discovers and loads all mods from the mods directory
        /// </summary>
        public void LoadAllMods()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ModLoader));

            var modDirs = Directory.GetDirectories(_modsDirectory);
            
            foreach (var modDir in modDirs)
            {
                try
                {
                    LoadModFromDirectory(modDir);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to load mod from {modDir}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Loads a single mod from a directory
        /// </summary>
        private void LoadModFromDirectory(string modDirectory)
        {
            var manifestPath = Path.Combine(modDirectory, "mod.json");
            
            if (!File.Exists(manifestPath))
            {
                Console.WriteLine($"No mod.json found in {modDirectory}");
                return;
            }

            var manifestJson = File.ReadAllText(manifestPath);
            var manifest = JsonSerializer.Deserialize<ModManifest>(manifestJson);

            if (manifest == null)
            {
                throw new InvalidOperationException("Failed to parse mod.json");
            }

            var scriptPath = Path.Combine(modDirectory, manifest.EntryPoint ?? "main.js");
            
            var mod = new JsMod(
                manifest.Id ?? Path.GetFileName(modDirectory),
                manifest.Name ?? "Unnamed Mod",
                manifest.Version ?? "1.0.0",
                scriptPath,
                _runtime
            );

            mod.OnLoad();
            _loadedMods.Add(mod);
        }

        /// <summary>
        /// Loads a specific mod by its ID
        /// </summary>
        public void LoadMod(string modId)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ModLoader));

            var modDir = Path.Combine(_modsDirectory, modId);
            if (!Directory.Exists(modDir))
            {
                throw new DirectoryNotFoundException($"Mod directory not found: {modDir}");
            }

            LoadModFromDirectory(modDir);
        }

        /// <summary>
        /// Unloads a specific mod by its ID
        /// </summary>
        public void UnloadMod(string modId)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ModLoader));

            var mod = _loadedMods.FirstOrDefault(m => m.Id == modId);
            if (mod != null)
            {
                mod.OnUnload();
                _loadedMods.Remove(mod);
            }
        }

        /// <summary>
        /// Updates all loaded mods
        /// Should be called in the game loop
        /// </summary>
        public void UpdateMods()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(ModLoader));

            // Update JavaScript runtime
            _runtime.Tick();

            // Update all mods
            foreach (var mod in _loadedMods.ToList())
            {
                try
                {
                    mod.OnUpdate();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error updating mod {mod.Name}: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            // Unload all mods
            foreach (var mod in _loadedMods.ToList())
            {
                try
                {
                    mod.OnUnload();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error unloading mod {mod.Name}: {ex.Message}");
                }
            }

            _loadedMods.Clear();
            _runtime?.Dispose();
            _disposed = true;
        }

        private class ModManifest
        {
            public string? Id { get; set; }
            public string? Name { get; set; }
            public string? Version { get; set; }
            public string? EntryPoint { get; set; }
            public string? Description { get; set; }
            public string? Author { get; set; }
        }
    }
}
