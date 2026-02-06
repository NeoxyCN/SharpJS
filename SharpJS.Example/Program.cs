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
            Console.WriteLine("=== SharpJS Game Modding Example ===");
            Console.WriteLine("PuerTS-based game modding system for .NET");
            Console.WriteLine();

            // Create mods directory
            var modsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mods");
            
            // Create example mod if it doesn't exist
            CreateExampleMod(modsPath);

            // Initialize mod loader
            using (var modLoader = new ModLoader(modsPath))
            {
                // Create and expose game API to mods
                var gameApi = new GameApi();
                modLoader.ExposeApi("game", gameApi);

                Console.WriteLine("Loading mods...");
                modLoader.LoadAllMods();
                Console.WriteLine($"Loaded {modLoader.LoadedMods.Count} mod(s)");
                Console.WriteLine();

                // Simulate game loop
                Console.WriteLine("Starting game loop (press Ctrl+C to exit)...");
                Console.WriteLine();

                var running = true;
                Console.CancelKeyPress += (sender, e) =>
                {
                    e.Cancel = true;
                    running = false;
                };

                int frameCount = 0;
                while (running && frameCount < 10) // Run for 10 frames for demonstration
                {
                    // Emit game tick event
                    gameApi.Emit("tick", frameCount.ToString());

                    // Update all mods
                    modLoader.UpdateMods();

                    frameCount++;
                    Thread.Sleep(1000); // 1 second per frame for demonstration

                    if (frameCount == 5)
                    {
                        Console.WriteLine("\n--- Triggering custom event ---");
                        gameApi.Emit("custom_event", "Hello from game!");
                    }
                }

                Console.WriteLine();
                Console.WriteLine("Shutting down...");
            }

            Console.WriteLine("Program ended.");
        }

        static void CreateExampleMod(string modsPath)
        {
            var exampleModPath = Path.Combine(modsPath, "example-mod");
            
            if (!Directory.Exists(exampleModPath))
            {
                Directory.CreateDirectory(exampleModPath);

                // Create mod.json
                var manifestPath = Path.Combine(exampleModPath, "mod.json");
                File.WriteAllText(manifestPath, @"{
  ""id"": ""example-mod"",
  ""name"": ""Example Mod"",
  ""version"": ""1.0.0"",
  ""description"": ""A simple example mod demonstrating SharpJS functionality"",
  ""author"": ""SharpJS"",
  ""entryPoint"": ""main.js""
}");

                // Create main.js
                var scriptPath = Path.Combine(exampleModPath, "main.js");
                File.WriteAllText(scriptPath, @"// Example mod for SharpJS
// This demonstrates how to create a mod using JavaScript/TypeScript

// Access the game API
const api = game || (typeof global !== 'undefined' ? global.game : globalThis.game);

// Store mod state
let tickCount = 0;

// Initialize the mod
global.mods['example-mod'].onLoad = function() {
    api.Log('Example mod loaded! Hello from JavaScript!');
    
    // Register event handler
    api.On('tick', function(data) {
        // This will be called every game tick
    });
    
    api.On('custom_event', function(data) {
        api.Log('Received custom event: ' + data);
    });
};

// Update function called every frame
global.mods['example-mod'].onUpdate = function() {
    tickCount++;
    
    if (tickCount === 1) {
        api.Log('First update tick!');
        api.SetState('mod_active', true);
    }
    
    if (tickCount === 3) {
        api.Log('Spawning an entity...');
        const entityId = api.SpawnEntity('test_entity', 100, 200);
        api.SetState('spawned_entity', entityId);
    }
    
    if (tickCount === 7) {
        const entityId = api.GetState('spawned_entity');
        if (entityId) {
            api.Log('Removing spawned entity...');
            api.RemoveEntity(entityId);
        }
    }
};

// Cleanup function
global.mods['example-mod'].onUnload = function() {
    api.Log('Example mod unloading...');
};

api.Log('Example mod script loaded');
");

                Console.WriteLine($"Created example mod at: {exampleModPath}");
                Console.WriteLine();
            }
        }
    }
}

