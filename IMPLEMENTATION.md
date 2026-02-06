# SharpJS Implementation Summary

## Project Overview
SharpJS is a PuerTS-based plugin system for standalone .NET applications (without Unity/Unreal dependencies).

## Architecture Components

### 1. Core Runtime (`ScriptEnvironment`)
- Wraps PuerTS `ScriptEnv` with `BackendV8`
- Manages JavaScript execution lifecycle
- Provides methods: `Execute`, `Execute<T>`, `RunModule`, `BindGlobalObject`, `ProcessTasks`

### 2. File Loading (`ScriptFileLoader`)
- Implements `ILoader` and `IModuleChecker` interfaces
- Loads scripts from:
  - PuerTS NuGet package content files
  - Filesystem (absolute/relative paths)
  - Embedded resources
- Handles .js/.mjs/.cjs extensions
- ES Module detection

### 3. Plugin System
- **IPluginModule**: Interface defining plugin contract
  - Properties: Identifier, DisplayName, VersionNumber
  - Methods: Initialize, Shutdown, PerformUpdate
- **JavaScriptPlugin**: Concrete implementation for JS-based plugins
  - Loads scripts and manages lifecycle
  - Invokes JavaScript callbacks: `initialize()`, `update()`, `shutdown()`

### 4. Plugin Management (`PluginOrchestrator`)
- Discovers plugins from directory
- Loads plugin.json configuration files
- Initializes/shuts down plugins
- Provides update loop integration
- Registers native APIs to JavaScript

### 5. Native Bridge (`HostBridge`)
- Exposes .NET functionality to JavaScript plugins
- Event system: `RegisterCallback`, `TriggerEvent`
- Data store: `StoreData`, `RetrieveData`
- Example game functions: `CreateEntity`, `DestroyEntity`
- Logging: `WriteMessage`

## Key Features
1. **PUERTS_GENERAL macro** configured for standalone .NET
2. Custom loader supporting PuerTS bootstrap files
3. JSON-based plugin configuration
4. Event-driven architecture
5. Bidirectional C#-JavaScript communication
6. Clean lifecycle management

## Usage Example

```csharp
using var orchestrator = new PluginOrchestrator("plugins");
var bridge = new HostBridge();
orchestrator.RegisterNativeApi("host", bridge);
orchestrator.InitializeAllPlugins();

while (running)
{
    orchestrator.UpdateAllPlugins();
}
```

## Plugin Structure

**plugin.json:**
```json
{
  "pluginId": "my-plugin",
  "pluginName": "My Plugin",
  "pluginVersion": "1.0.0",
  "mainScript": "index.js"
}
```

**index.js:**
```javascript
const host = global.host;

global.plugins['my-plugin'].initialize = function() {
    host.WriteMessage('Plugin loaded');
};

global.plugins['my-plugin'].update = function() {
    // Per-frame logic
};

global.plugins['my-plugin'].shutdown = function() {
    // Cleanup
};
```

## Technical Implementation

### PuerTS Integration
- Uses `BackendV8` for V8 JavaScript engine
- Implements custom `ILoader` for script resolution
- Loads PuerTS bootstrap from NuGet package: `~/.nuget/packages/puerts.core/3.0.0/contentFiles/`

### Error Handling
- Try-catch blocks around plugin operations
- Graceful degradation on plugin errors
- Console logging for debugging

### Memory Management
- Proper `IDisposable` implementation
- Cleanup in Dispose methods
- JavaScript environment cleanup

## Testing
- Basic unit tests for HostBridge functionality
- Example application demonstrating all features
- Successful execution confirmed

## Documentation
- Comprehensive README.md
- API documentation in docs/API.md
- Chinese documentation (docs/README_CN.md)
- Inline code comments

## Build & Run
```bash
dotnet build
cd SharpJS.Example
dotnet run
```

Output demonstrates:
- Plugin initialization
- Update loop execution
- Event triggering
- Entity creation/destruction
- Graceful shutdown
