// Example mod for SharpJS
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
