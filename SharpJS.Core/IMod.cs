using System;

namespace SharpJS.Core
{
    /// <summary>
    /// Contract for plugin modules in the modding system
    /// </summary>
    public interface IPluginModule
    {
        string Identifier { get; }
        string DisplayName { get; }
        string VersionNumber { get; }
        
        void Initialize();
        void Shutdown();
        void PerformUpdate();
    }
}
