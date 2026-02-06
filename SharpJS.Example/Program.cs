using System;
using System.IO;
using System.Threading;
using SharpJS.Core;

namespace SharpJS.Example
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== SharpJS Plugin System Demo ===");
            Console.WriteLine("PuerTS-powered extensibility for .NET applications");
            Console.WriteLine();

            var pluginsDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "plugins");
            
            PrepareExamplePlugin(pluginsDirectory);

            using (var orchestrator = new PluginOrchestrator(pluginsDirectory))
            {
                var hostBridge = new HostBridge();
                orchestrator.RegisterNativeApi("host", hostBridge);

                Console.WriteLine("Initializing plugins...");
                orchestrator.InitializeAllPlugins();
                Console.WriteLine($"Active plugins: {orchestrator.ActivePlugins.Count}");
                Console.WriteLine();

                Console.WriteLine("Running update loop (10 iterations)...");
                Console.WriteLine();

                bool continueRunning = true;
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    continueRunning = false;
                };

                int iteration = 0;
                while (continueRunning && iteration < 10)
                {
                    hostBridge.TriggerEvent("frame_update", iteration.ToString());
                    orchestrator.UpdateAllPlugins();

                    iteration++;
                    Thread.Sleep(1000);

                    if (iteration == 5)
                    {
                        Console.WriteLine("\n--- Triggering special event ---");
                        hostBridge.TriggerEvent("milestone_reached", "Halfway complete!");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Shutting down gracefully...");
            }

            Console.WriteLine("Application terminated.");
        }

        static void PrepareExamplePlugin(string pluginsDirectory)
        {
            var demoPluginPath = Path.Combine(pluginsDirectory, "demo-plugin");
            
            if (!Directory.Exists(demoPluginPath))
            {
                Directory.CreateDirectory(demoPluginPath);

                File.WriteAllText(Path.Combine(demoPluginPath, "plugin.json"), @"{
  ""pluginId"": ""demo-plugin"",
  ""pluginName"": ""Demo Plugin"",
  ""pluginVersion"": ""1.0.0"",
  ""description"": ""Demonstration of SharpJS plugin capabilities"",
  ""author"": ""SharpJS Team"",
  ""mainScript"": ""index.js""
}");

                File.WriteAllText(Path.Combine(demoPluginPath, "index.js"), @"// Demo Plugin for SharpJS
const host = global.host || globalThis.host;

let updateCounter = 0;

global.plugins['demo-plugin'].initialize = function() {
    host.WriteMessage('Demo plugin initialized successfully!');
    
    host.RegisterCallback('frame_update', function(data) {
        // Frame update callback
    });
    
    host.RegisterCallback('milestone_reached', function(data) {
        host.WriteMessage('Milestone event received: ' + data);
    });
};

global.plugins['demo-plugin'].update = function() {
    updateCounter++;
    
    if (updateCounter === 1) {
        host.WriteMessage('First update cycle executed');
        host.StoreData('plugin_status', true);
    }
    
    if (updateCounter === 3) {
        host.WriteMessage('Creating test entity...');
        const entityId = host.CreateEntity('demo_object', 150, 250);
        host.StoreData('created_entity_id', entityId);
    }
    
    if (updateCounter === 7) {
        const storedId = host.RetrieveData('created_entity_id');
        if (storedId) {
            host.WriteMessage('Destroying previously created entity...');
            host.DestroyEntity(storedId);
        }
    }
};

global.plugins['demo-plugin'].shutdown = function() {
    host.WriteMessage('Demo plugin shutting down gracefully');
};

host.WriteMessage('Demo plugin script loaded');
");

                Console.WriteLine($"Created demo plugin in: {demoPluginPath}");
                Console.WriteLine();
            }
        }
    }
}

