# SharpJS

[English](#english) | [中文](#chinese)

<a name="english"></a>
## English

A PuerTS-based plugin system for .NET environments. SharpJS enables JavaScript/TypeScript plugins in any .NET application without Unity or Unreal Engine dependencies.

### Features

- ✨ **Pure .NET Implementation**: Works in any .NET 8.0+ environment
- 🚀 **Multiple JavaScript Engines**: Choose from V8, QuickJS, or Node.js based on your needs
- 🔌 **C# ↔ JavaScript Interop**: Seamless bidirectional communication between C# and JavaScript
- 📦 **Plugin Management**: Built-in plugin orchestrator with lifecycle management (initialize/update/shutdown)
- 🛠️ **Easy to Use API**: Simple API for exposing C# functionality to plugins
- 📝 **TypeScript Support**: Full TypeScript support through PuerTS
- 🎯 **Event-Driven Architecture**: Robust event system for plugin communication
- 💾 **Data Persistence**: Shared data store for plugin state management

### Quick Start

#### Installation

1. Clone or reference SharpJS.Core in your project:

```bash
# Clone the repository
git clone https://github.com/NeoxyCN/SharpJS.git

# Or add as a project reference
dotnet add reference path/to/SharpJS.Core/SharpJS.Core.csproj
```

2. Install via NuGet (if published):

```bash
dotnet add package SharpJS.Core
```

#### Basic Usage

```csharp
using SharpJS.Core;
using System;

// Create plugin orchestrator with default V8 engine
using var orchestrator = new PluginOrchestrator("./plugins");

// Create and expose host API
var hostBridge = new HostBridge();
orchestrator.RegisterNativeApi("host", hostBridge);

// Initialize all plugins
orchestrator.InitializeAllPlugins();

// Main loop
bool running = true;
while (running)
{
    hostBridge.TriggerEvent("frame_update", frameCount.ToString());
    orchestrator.UpdateAllPlugins();
    System.Threading.Thread.Sleep(16); // ~60 FPS
}
```

#### JavaScript Engine Selection

SharpJS supports multiple JavaScript engines. You can choose the engine when creating the `PluginOrchestrator` or `ScriptEnvironment`:

```csharp
// Use V8 (default, high performance)
using var orchestratorV8 = new PluginOrchestrator("./plugins", JsEngineType.V8);

// Use QuickJS (lightweight, embedded-friendly)
using var orchestratorQjs = new PluginOrchestrator("./plugins", JsEngineType.QuickJS);

// Use Node.js (full Node.js API support - requires libnode installed)
using var orchestratorNode = new PluginOrchestrator("./plugins", JsEngineType.NodeJS);
```

**Available Engines:**

| Engine | Type | Description | Requirements |
|--------|------|-------------|--------------|
| **V8** | `JsEngineType.V8` | High-performance V8 engine (default) | libc++ on Linux |
| **QuickJS** | `JsEngineType.QuickJS` | Lightweight, embedded-friendly | None |
| **Node.js** | `JsEngineType.NodeJS` | Full Node.js API support | libnode.so installed |

#### Creating a Plugin

1. Create a plugin directory (e.g., `plugins/my-plugin/`)
2. Add `plugin.json`:

```json
{
  "pluginId": "my-plugin",
  "pluginName": "My Awesome Plugin",
  "pluginVersion": "1.0.0",
  "description": "Description of my plugin",
  "author": "Your Name",
  "mainScript": "index.js"
}
```

3. Create `index.js`:

```javascript
// Access the host API
const host = global.host || globalThis.host;

// Plugin state
let counter = 0;

// Initialize plugin
global.plugins['my-plugin'].initialize = function() {
    host.WriteMessage('My plugin loaded!');
    
    // Register event handlers
    host.RegisterCallback('frame_update', function(data) {
        // Handle frame updates
    });
    
    host.RegisterCallback('custom_event', function(data) {
        host.WriteMessage('Received: ' + data);
    });
};

// Update function (called every frame)
global.plugins['my-plugin'].update = function() {
    counter++;
    
    if (counter === 10) {
        host.WriteMessage('10 frames passed!');
    }
};

// Cleanup
global.plugins['my-plugin'].shutdown = function() {
    host.WriteMessage('My plugin shutting down...');
};
```

### Architecture

#### Core Components

- **ScriptEnvironment**: JavaScript runtime wrapper using PuerTS V8
  - Manages JavaScript execution lifecycle
  - Methods: `Execute()`, `Execute<T>()`, `RunModule()`, `BindGlobalObject()`, `ProcessTasks()`

- **PluginOrchestrator**: Manages plugin lifecycle and JavaScript environment
  - Discovers and loads plugins from directories
  - Methods: `InitializeAllPlugins()`, `UpdateAllPlugins()`, `RegisterNativeApi()`

- **HostBridge**: Bridge API exposing .NET functionality to plugins
  - Event system: `RegisterCallback()`, `TriggerEvent()`
  - Data store: `StoreData()`, `RetrieveData()`
  - Logging: `WriteMessage()`
  - Example functions: `CreateEntity()`, `DestroyEntity()`, `GetCurrentTimestamp()`

- **ScriptFileLoader**: Custom file loader for JavaScript modules
  - Loads from filesystem, PuerTS packages, and embedded resources
  - Supports .js/.mjs/.cjs extensions

- **IPluginModule / JavaScriptPlugin**: Plugin interface and JavaScript implementation
  - Properties: `Identifier`, `DisplayName`, `VersionNumber`
  - Methods: `Initialize()`, `Shutdown()`, `PerformUpdate()`

#### Project Structure

```
SharpJS/
├── SharpJS.Core/              # Core plugin framework
│   ├── JsRuntime.cs           # ScriptEnvironment class
│   ├── ModLoader.cs           # PluginOrchestrator class
│   ├── GameApi.cs             # HostBridge class
│   ├── DefaultLoader.cs       # ScriptFileLoader class
│   ├── IMod.cs                # IPluginModule interface
│   └── JsMod.cs               # JavaScriptPlugin class
├── SharpJS.Example/           # Example console application
│   └── Program.cs             # Demo usage with sample plugin
├── SharpJS.Tests/             # Unit tests
│   └── CoreTests.cs           # Basic functionality tests
└── docs/                      # Documentation
    ├── API.md                 # API reference
    └── README_CN.md           # Chinese documentation
```

### API Reference

#### HostBridge

The `HostBridge` class provides methods that plugins can call:

```javascript
// Logging
host.WriteMessage(message)

// Event system
host.RegisterCallback(eventName, callback)
host.TriggerEvent(eventName, data)  // Called from C#

// Data persistence
host.StoreData(key, value)
host.RetrieveData(key)

// Utility functions
host.GetCurrentTimestamp()

// Example game functions
host.CreateEntity(entityType, x, y)
host.DestroyEntity(entityId)
```

#### Creating Custom APIs

You can create your own API classes to expose to plugins:

```csharp
public class MyCustomApi
{
    public void CustomMethod(string param)
    {
        Console.WriteLine($"Called from plugin: {param}");
    }
    
    public int Calculate(int a, int b)
    {
        return a + b;
    }
    
    public string GetVersion()
    {
        return "1.0.0";
    }
}

// Expose to plugins
var customApi = new MyCustomApi();
orchestrator.RegisterNativeApi("myApi", customApi);
```

Then in JavaScript:

```javascript
const myApi = global.myApi || globalThis.myApi;

global.plugins['my-plugin'].initialize = function() {
    myApi.CustomMethod("Hello from JavaScript!");
    const result = myApi.Calculate(5, 3);  // result = 8
    const version = myApi.GetVersion();    // version = "1.0.0"
};
```

### Requirements

- .NET 8.0 or higher
- PuerTS.V8.Complete NuGet package (automatically included)
- Platform support: Windows, Linux, macOS

### Implementation Details

This project implements PuerTS in a pure .NET environment following the official guidelines:

1. **PUERTS_GENERAL macro**: Defined in the project to enable general .NET usage
2. **Custom Loader**: Implements `ILoader` and `IModuleChecker` to load JavaScript files
3. **Bootstrap Files**: Automatically loads PuerTS bootstrap files from NuGet packages (`~/.nuget/packages/puerts.core/3.0.0/contentFiles/`)
4. **V8 Backend**: Uses `BackendV8` for high-performance JavaScript execution

### Running the Example

```bash
cd SharpJS.Example
dotnet run
```

This will:
1. Create a demo plugin if it doesn't exist
2. Initialize the plugin
3. Run an update loop for 10 iterations
4. Demonstrate event system and API calls
5. Cleanly shutdown the plugin

Example output:
```
=== SharpJS Plugin System Demo ===
PuerTS-powered extensibility for .NET applications

Initializing plugins...
[PLUGIN] Demo plugin script loaded
[PLUGIN] Demo plugin initialized successfully!
Plugin initialized: Demo Plugin v1.0.0
Active plugins: 1

Running update loop (10 iterations)...

[PLUGIN] First update cycle executed
[PLUGIN] Creating test entity...
[PLUGIN] Creating demo_object at position (150, 250) with ID ac7d4374-...

--- Triggering special event ---
[PLUGIN] Milestone event received: Halfway complete!
[PLUGIN] Destroying previously created entity...

Shutting down gracefully...
[PLUGIN] Demo plugin shutting down gracefully
Application terminated.
```

### Running Tests

```bash
cd SharpJS.Tests
dotnet test
```

Note: Basic unit tests are included for core functionality. The example application provides comprehensive integration testing.

### Building

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build SharpJS.Core
```

### Troubleshooting

#### Plugin not loading
- Check that `plugin.json` is properly formatted
- Ensure `mainScript` field points to the correct JavaScript file
- Verify the plugin directory structure

#### V8 initialization errors
- Make sure libc++ is installed on Linux systems: `sudo apt-get install libc++1`
- Check that PuerTS NuGet packages are properly restored

#### JavaScript errors
- Use `host.WriteMessage()` for debugging
- Check the console output for error messages
- Verify JavaScript syntax is correct

---

<a name="chinese"></a>
## 中文

一个基于 PuerTS 的 .NET 插件系统。SharpJS 使任何 .NET 应用程序都能支持 JavaScript/TypeScript 插件，无需 Unity 或 Unreal Engine 依赖。

### 特性

- ✨ **纯 .NET 实现**：适用于任何 .NET 8.0+ 环境
- 🚀 **多 JavaScript 引擎支持**：可选择 V8、QuickJS 或 Node.js 引擎
- 🔌 **C# ↔ JavaScript 互操作**：C# 和 JavaScript 之间的无缝双向通信
- 📦 **插件管理**：内置插件编排器，具有完整的生命周期管理（初始化/更新/关闭）
- 🛠️ **易用的 API**：简单的 API 用于向插件公开 C# 功能
- 📝 **TypeScript 支持**：通过 PuerTS 完全支持 TypeScript
- 🎯 **事件驱动架构**：强大的事件系统用于插件通信
- 💾 **数据持久化**：插件状态管理的共享数据存储

### 快速开始

#### 安装

1. 克隆或引用 SharpJS.Core 项目：

```bash
# 克隆仓库
git clone https://github.com/NeoxyCN/SharpJS.git

# 或添加项目引用
dotnet add reference path/to/SharpJS.Core/SharpJS.Core.csproj
```

2. 通过 NuGet 安装（如果已发布）：

```bash
dotnet add package SharpJS.Core
```

#### 基本用法

```csharp
using SharpJS.Core;
using System;

// 创建插件编排器（默认使用 V8 引擎）
using var orchestrator = new PluginOrchestrator("./plugins");

// 创建并公开宿主 API
var hostBridge = new HostBridge();
orchestrator.RegisterNativeApi("host", hostBridge);

// 初始化所有插件
orchestrator.InitializeAllPlugins();

// 主循环
bool running = true;
while (running)
{
    hostBridge.TriggerEvent("frame_update", frameCount.ToString());
    orchestrator.UpdateAllPlugins();
    System.Threading.Thread.Sleep(16); // ~60 FPS
}
```

#### JavaScript 引擎选择

SharpJS 支持多种 JavaScript 引擎。您可以在创建 `PluginOrchestrator` 或 `ScriptEnvironment` 时选择引擎：

```csharp
// 使用 V8（默认，高性能）
using var orchestratorV8 = new PluginOrchestrator("./plugins", JsEngineType.V8);

// 使用 QuickJS（轻量级，适合嵌入式）
using var orchestratorQjs = new PluginOrchestrator("./plugins", JsEngineType.QuickJS);

// 使用 Node.js（完整 Node.js API 支持 - 需要安装 libnode）
using var orchestratorNode = new PluginOrchestrator("./plugins", JsEngineType.NodeJS);
```

**可用引擎：**

| 引擎 | 类型 | 描述 | 依赖 |
|------|------|------|------|
| **V8** | `JsEngineType.V8` | 高性能 V8 引擎（默认） | Linux 上需要 libc++ |
| **QuickJS** | `JsEngineType.QuickJS` | 轻量级，适合嵌入式 | 无 |
| **Node.js** | `JsEngineType.NodeJS` | 完整 Node.js API 支持 | 需要安装 libnode.so |

#### 创建插件

1. 创建插件目录（例如 `plugins/my-plugin/`）
2. 添加 `plugin.json`：

```json
{
  "pluginId": "my-plugin",
  "pluginName": "我的超棒插件",
  "pluginVersion": "1.0.0",
  "description": "插件描述",
  "author": "您的名字",
  "mainScript": "index.js"
}
```

3. 创建 `index.js`：

```javascript
// 访问宿主 API
const host = global.host || globalThis.host;

// 插件状态
let counter = 0;

// 初始化插件
global.plugins['my-plugin'].initialize = function() {
    host.WriteMessage('我的插件已加载！');
    
    // 注册事件处理器
    host.RegisterCallback('frame_update', function(data) {
        // 处理帧更新
    });
    
    host.RegisterCallback('custom_event', function(data) {
        host.WriteMessage('收到：' + data);
    });
};

// 更新函数（每帧调用）
global.plugins['my-plugin'].update = function() {
    counter++;
    
    if (counter === 10) {
        host.WriteMessage('已过 10 帧！');
    }
};

// 清理
global.plugins['my-plugin'].shutdown = function() {
    host.WriteMessage('我的插件正在关闭...');
};
```

### API 参考

#### HostBridge

`HostBridge` 类提供插件可以调用的方法：

```javascript
// 日志记录
host.WriteMessage(message)

// 事件系统
host.RegisterCallback(eventName, callback)
host.TriggerEvent(eventName, data)  // 从 C# 调用

// 数据持久化
host.StoreData(key, value)
host.RetrieveData(key)

// 实用函数
host.GetCurrentTimestamp()

// 示例游戏函数
host.CreateEntity(entityType, x, y)
host.DestroyEntity(entityId)
```

#### 创建自定义 API

您可以创建自己的 API 类来公开给插件：

```csharp
public class MyCustomApi
{
    public void CustomMethod(string param)
    {
        Console.WriteLine($"从插件调用：{param}");
    }
    
    public int Calculate(int a, int b)
    {
        return a + b;
    }
    
    public string GetVersion()
    {
        return "1.0.0";
    }
}

// 公开给插件
var customApi = new MyCustomApi();
orchestrator.RegisterNativeApi("myApi", customApi);
```

然后在 JavaScript 中：

```javascript
const myApi = global.myApi || globalThis.myApi;

global.plugins['my-plugin'].initialize = function() {
    myApi.CustomMethod("来自 JavaScript 的你好！");
    const result = myApi.Calculate(5, 3);  // result = 8
    const version = myApi.GetVersion();    // version = "1.0.0"
};
```

### 系统要求

- .NET 8.0 或更高版本
- PuerTS.V8.Complete NuGet 包（自动包含）
- 平台支持：Windows、Linux、macOS

### 实现细节

本项目遵循官方指南在纯 .NET 环境中实现 PuerTS：

1. **PUERTS_GENERAL 宏**：在项目中定义以启用通用 .NET 使用
2. **自定义加载器**：实现 `ILoader` 和 `IModuleChecker` 以加载 JavaScript 文件
3. **引导文件**：自动从 NuGet 包加载 PuerTS 引导文件（`~/.nuget/packages/puerts.core/3.0.0/contentFiles/`）
4. **V8 后端**：使用 `BackendV8` 实现高性能 JavaScript 执行

### 运行示例

```bash
cd SharpJS.Example
dotnet run
```

这将：
1. 如果不存在则创建演示插件
2. 初始化插件
3. 运行 10 次迭代的更新循环
4. 演示事件系统和 API 调用
5. 干净地关闭插件

示例输出：
```
=== SharpJS Plugin System Demo ===
PuerTS-powered extensibility for .NET applications

Initializing plugins...
[PLUGIN] Demo plugin script loaded
[PLUGIN] Demo plugin initialized successfully!
Plugin initialized: Demo Plugin v1.0.0
Active plugins: 1

Running update loop (10 iterations)...

[PLUGIN] First update cycle executed
[PLUGIN] Creating test entity...
[PLUGIN] Creating demo_object at position (150, 250) with ID ac7d4374-...

--- Triggering special event ---
[PLUGIN] Milestone event received: Halfway complete!
[PLUGIN] Destroying previously created entity...

Shutting down gracefully...
[PLUGIN] Demo plugin shutting down gracefully
Application terminated.
```

### 运行测试

```bash
cd SharpJS.Tests
dotnet test
```

注意：包含核心功能的基本单元测试。示例应用程序提供全面的集成测试。

### 构建

```bash
# 构建整个解决方案
dotnet build

# 构建特定项目
dotnet build SharpJS.Core
```

### 故障排除

#### 插件无法加载
- 检查 `plugin.json` 格式是否正确
- 确保 `mainScript` 字段指向正确的 JavaScript 文件
- 验证插件目录结构

#### V8 初始化错误
- 确保在 Linux 系统上安装了 libc++：`sudo apt-get install libc++1`
- 检查 PuerTS NuGet 包是否正确恢复

#### JavaScript 错误
- 使用 `host.WriteMessage()` 进行调试
- 检查控制台输出的错误消息
- 验证 JavaScript 语法是否正确

---

## License

See LICENSE file for details.

## Contributing / 贡献

Contributions are welcome! Please feel free to submit pull requests.

欢迎贡献！请随时提交拉取请求。

## Acknowledgments / 致谢

- Built on [PuerTS](https://github.com/Tencent/puerts) by Tencent / 基于腾讯的 PuerTS 构建
- Inspired by Unity and Unreal Engine plugin systems / 受 Unity 和 Unreal Engine 插件系统启发

## Support / 支持

For issues and questions, please use the GitHub issue tracker.

如有问题，请使用 GitHub issue tracker。

## Documentation / 文档

- [API Documentation](docs/API.md) - Detailed API reference
- [Implementation Details](IMPLEMENTATION.md) - Technical implementation details
- [Chinese Documentation](docs/README_CN.md) - 中文详细文档
