using System;
using System.IO;

namespace SharpJS.Core
{
    /// <summary>
    /// JavaScript-powered plugin module implementation
    /// Executes scripts and manages their lifecycle
    /// </summary>
    public class JavaScriptPlugin : IPluginModule
    {
        private readonly ScriptEnvironment _scriptEnv;
        private readonly string _scriptFilePath;
        private bool _initialized;

        public string Identifier { get; }
        public string DisplayName { get; }
        public string VersionNumber { get; }

        public JavaScriptPlugin(
            string identifier, 
            string displayName, 
            string versionNumber, 
            string scriptFilePath, 
            ScriptEnvironment scriptEnv)
        {
            Identifier = identifier ?? throw new ArgumentNullException(nameof(identifier));
            DisplayName = displayName ?? throw new ArgumentNullException(nameof(displayName));
            VersionNumber = versionNumber ?? throw new ArgumentNullException(nameof(versionNumber));
            _scriptFilePath = scriptFilePath ?? throw new ArgumentNullException(nameof(scriptFilePath));
            _scriptEnv = scriptEnv ?? throw new ArgumentNullException(nameof(scriptEnv));
        }

        public void Initialize()
        {
            if (_initialized) return;

            if (!File.Exists(_scriptFilePath))
                throw new FileNotFoundException($"Plugin script missing: {_scriptFilePath}");

            try
            {
                var scriptContent = File.ReadAllText(_scriptFilePath);
                
                // Setup plugin context in JavaScript
                _scriptEnv.Execute($@"
                    if (typeof global.plugins === 'undefined') {{
                        global.plugins = {{}};
                    }}
                    global.plugins['{Identifier}'] = {{
                        id: '{Identifier}',
                        name: '{DisplayName}',
                        version: '{VersionNumber}'
                    }};
                ");

                // Execute the plugin script
                _scriptEnv.Execute(scriptContent);

                // Invoke initialization callback if defined
                try
                {
                    _scriptEnv.Execute($@"
                        if (global.plugins['{Identifier}'].initialize && 
                            typeof global.plugins['{Identifier}'].initialize === 'function') {{
                            global.plugins['{Identifier}'].initialize();
                        }}
                    ");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Plugin {Identifier} initialization callback error: {ex.Message}");
                }

                _initialized = true;
                Console.WriteLine($"Plugin initialized: {DisplayName} v{VersionNumber}");
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to initialize plugin {DisplayName}: {ex.Message}", ex);
            }
        }

        public void Shutdown()
        {
            if (!_initialized) return;

            try
            {
                _scriptEnv.Execute($@"
                    if (global.plugins['{Identifier}'] && 
                        typeof global.plugins['{Identifier}'].shutdown === 'function') {{
                        global.plugins['{Identifier}'].shutdown();
                    }}
                    delete global.plugins['{Identifier}'];
                ");

                _initialized = false;
                Console.WriteLine($"Plugin shutdown: {DisplayName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during plugin {DisplayName} shutdown: {ex.Message}");
            }
        }

        public void PerformUpdate()
        {
            if (!_initialized) return;

            try
            {
                _scriptEnv.Execute($@"
                    if (global.plugins['{Identifier}'] && 
                        typeof global.plugins['{Identifier}'].update === 'function') {{
                        global.plugins['{Identifier}'].update();
                    }}
                ");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in plugin {DisplayName} update: {ex.Message}");
            }
        }
    }
}
