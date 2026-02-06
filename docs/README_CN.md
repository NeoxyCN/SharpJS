# SharpJS 中文文档

基于 PuerTS 的 .NET 游戏模组系统。

## 功能特性

- ✨ **纯 .NET 实现**: 适用于任何 .NET 8.0+ 环境
- 🚀 **PuerTS 集成**: 由 PuerTS V8 引擎提供支持
- 🔌 **C# ↔ JavaScript 互操作**: 无缝双向通信
- 📦 **模组管理**: 内置模组加载器
- 🛠️ **易用的 API**: 简单的 API 接口
- 📝 **TypeScript 支持**: 完全支持 TypeScript

## 快速开始

```csharp
using SharpJS.Core;

// 创建模组加载器
using var modLoader = new ModLoader("mods");

// 暴露游戏 API
var gameApi = new GameApi();
modLoader.ExposeApi("game", gameApi);

// 加载所有模组
modLoader.LoadAllMods();

// 游戏循环
while (running)
{
    modLoader.UpdateMods();
}
```

## 运行示例

```bash
cd SharpJS.Example
dotnet run
```

更多信息请查看英文文档。
