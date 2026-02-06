# SharpJS API Documentation

## Core Classes

### JsRuntime

The `JsRuntime` class provides a wrapper around the PuerTS JavaScript engine.

#### Constructor

```csharp
public JsRuntime()
public JsRuntime(ILoader loader)
```

Creates a new JavaScript runtime instance. Optionally accepts a custom loader.

#### Methods

##### Eval
```csharp
public void Eval(string code)
public T Eval<T>(string code)
```

Evaluates JavaScript code and optionally returns a typed result.

**Example:**
```csharp
var runtime = new JsRuntime();
runtime.Eval("console.log('Hello')");
var result = runtime.Eval<int>("2 + 2"); // returns 4
```

##### ExecuteModule
```csharp
public void ExecuteModule(string moduleName)
```

Executes a JavaScript ES module.

##### RegisterObject
```csharp
public void RegisterObject(string name, object obj)
```

Registers a C# object to be accessible from JavaScript.

**Example:**
```csharp
var myObj = new MyClass();
runtime.RegisterObject("myApi", myObj);
// Now accessible in JS as: global.myApi or globalThis.myApi
```

##### Tick
```csharp
public void Tick()
```

Processes pending JavaScript tasks. Should be called regularly.

---

### ModLoader

The `ModLoader` class manages the lifecycle of game mods.

#### Constructor

```csharp
public ModLoader(string modsDirectory)
```

Creates a mod loader that will load mods from the specified directory.

#### Properties

```csharp
public IReadOnlyList<IMod> LoadedMods { get; }
```

Gets the list of currently loaded mods.

#### Methods

##### ExposeApi
```csharp
public void ExposeApi(string name, object obj)
```

Exposes a C# object to all mods with the given name.

**Example:**
```csharp
var gameApi = new GameApi();
modLoader.ExposeApi("game", gameApi);
```

##### LoadAllMods
```csharp
public void LoadAllMods()
```

Discovers and loads all mods from the mods directory. Each mod must have a `mod.json` manifest file.

##### LoadMod
```csharp
public void LoadMod(string modId)
```

Loads a specific mod by its ID.

##### UnloadMod
```csharp
public void UnloadMod(string modId)
```

Unloads a specific mod by its ID.

##### UpdateMods
```csharp
public void UpdateMods()
```

Updates all loaded mods. Should be called in your game loop.

---

### GameApi

The `GameApi` class provides example game functionality that can be exposed to mods.

#### Methods

##### Log
```csharp
public void Log(string message)
```

Logs a message from a mod.

##### SetState / GetState
```csharp
public void SetState(string key, object value)
public object? GetState(string key)
```

Sets or retrieves game state values.

##### On / Emit
```csharp
public void On(string eventName, Action<string> handler)
public void Emit(string eventName, string data = "")
```

Registers event handlers and emits events.

##### GetTime
```csharp
public double GetTime()
```

Returns the current game time in seconds.

##### SpawnEntity
```csharp
public string SpawnEntity(string entityType, double x, double y)
```

Spawns a game entity and returns its ID.

##### RemoveEntity
```csharp
public void RemoveEntity(string entityId)
```

Removes a game entity by ID.

---

## JavaScript Mod Structure

### Mod Manifest (mod.json)

```json
{
  "id": "unique-mod-id",
  "name": "Mod Display Name",
  "version": "1.0.0",
  "description": "Mod description",
  "author": "Author name",
  "entryPoint": "main.js"
}
```

### Mod Script Structure

```javascript
// Access APIs
const api = global.game || globalThis.game;

// Initialize
global.mods['your-mod-id'].onLoad = function() {
    // Called when mod loads
};

// Update
global.mods['your-mod-id'].onUpdate = function() {
    // Called every frame
};

// Cleanup
global.mods['your-mod-id'].onUnload = function() {
    // Called when mod unloads
};
```

For more details, see the full API documentation.
