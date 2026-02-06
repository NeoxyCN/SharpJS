using System;
using System.IO;

namespace SharpJS.Core
{
    /// <summary>
    /// JavaScript-based mod implementation
    /// Loads and executes mods written in JavaScript/TypeScript
    /// </summary>
    public class JsMod : IMod
    {
        private readonly JsRuntime _runtime;
        private readonly string _scriptPath;
        private bool _isLoaded;

        public string Id { get; }
        public string Name { get; }
        public string Version { get; }

        public JsMod(string id, string name, string version, string scriptPath, JsRuntime runtime)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Version = version ?? throw new ArgumentNullException(nameof(version));
            _scriptPath = scriptPath ?? throw new ArgumentNullException(nameof(scriptPath));
            _runtime = runtime ?? throw new ArgumentNullException(nameof(runtime));
        }

        public void OnLoad()
        {
            if (_isLoaded)
                return;

            if (!File.Exists(_scriptPath))
                throw new FileNotFoundException($"Mod script not found: {_scriptPath}");

            try
            {
                var script = File.ReadAllText(_scriptPath);
                
                // Set up mod context
                _runtime.Eval($@"
                    if (typeof global.mods === 'undefined') {{
                        global.mods = {{}};
                    }}
                    global.mods['{Id}'] = {{
                        id: '{Id}',
                        name: '{Name}',
                        version: '{Version}'
                    }};
                ");

                // Execute the mod script
                _runtime.Eval(script);

                // Call mod's initialization if it exists
                try
                {
                    _runtime.Eval($@"
                        if (typeof global.mods['{Id}'].onLoad === 'function') {{
                            global.mods['{Id}'].onLoad();
                        }}
                    ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Mod {Id} onLoad failed: {ex.Message}");
                }

                _isLoaded = true;
                Console.WriteLine($"Mod loaded: {Name} v{Version}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to load mod {Name}: {ex.Message}", ex);
            }
        }

        public void OnUnload()
        {
            if (!_isLoaded)
                return;

            try
            {
                // Call mod's cleanup if it exists
                _runtime.Eval($@"
                    if (global.mods['{Id}'] && typeof global.mods['{Id}'].onUnload === 'function') {{
                        global.mods['{Id}'].onUnload();
                    }}
                    delete global.mods['{Id}'];
                ");

                _isLoaded = false;
                Console.WriteLine($"Mod unloaded: {Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error unloading mod {Name}: {ex.Message}");
            }
        }

        public void OnUpdate()
        {
            if (!_isLoaded)
                return;

            try
            {
                _runtime.Eval($@"
                    if (global.mods['{Id}'] && typeof global.mods['{Id}'].onUpdate === 'function') {{
                        global.mods['{Id}'].onUpdate();
                    }}
                ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in mod {Name} update: {ex.Message}");
            }
        }
    }
}
