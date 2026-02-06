using System;

namespace SharpJS.Core
{
    /// <summary>
    /// Interface that all game mods must implement
    /// </summary>
    public interface IMod
    {
        /// <summary>
        /// Unique identifier for the mod
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Display name of the mod
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Version of the mod
        /// </summary>
        string Version { get; }

        /// <summary>
        /// Called when the mod is loaded
        /// </summary>
        void OnLoad();

        /// <summary>
        /// Called when the mod is unloaded
        /// </summary>
        void OnUnload();

        /// <summary>
        /// Called on each update tick
        /// </summary>
        void OnUpdate();
    }
}
