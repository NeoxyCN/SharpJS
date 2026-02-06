using System;
using System.Collections.Generic;

namespace SharpJS.Core
{
    /// <summary>
    /// Game API that can be exposed to JavaScript mods
    /// This provides a safe interface for mods to interact with the game
    /// </summary>
    public class GameApi
    {
        private readonly Dictionary<string, Action<string>> _eventHandlers;
        private readonly Dictionary<string, object> _gameState;

        public GameApi()
        {
            _eventHandlers = new Dictionary<string, Action<string>>();
            _gameState = new Dictionary<string, object>();
        }

        /// <summary>
        /// Logs a message from a mod
        /// </summary>
        public void Log(string message)
        {
            Console.WriteLine($"[MOD] {message}");
        }

        /// <summary>
        /// Registers an event handler
        /// </summary>
        public void On(string eventName, Action<string> handler)
        {
            if (!_eventHandlers.ContainsKey(eventName))
            {
                _eventHandlers[eventName] = handler;
            }
            else
            {
                _eventHandlers[eventName] += handler;
            }
        }

        /// <summary>
        /// Triggers an event
        /// </summary>
        public void Emit(string eventName, string data = "")
        {
            if (_eventHandlers.ContainsKey(eventName))
            {
                try
                {
                    _eventHandlers[eventName]?.Invoke(data);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in event handler for {eventName}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Gets a value from game state
        /// </summary>
        public object? GetState(string key)
        {
            return _gameState.ContainsKey(key) ? _gameState[key] : null;
        }

        /// <summary>
        /// Sets a value in game state
        /// </summary>
        public void SetState(string key, object value)
        {
            _gameState[key] = value;
        }

        /// <summary>
        /// Gets the current game time (example)
        /// </summary>
        public double GetTime()
        {
            return DateTime.Now.TimeOfDay.TotalSeconds;
        }

        /// <summary>
        /// Spawns an entity (example game function)
        /// </summary>
        public string SpawnEntity(string entityType, double x, double y)
        {
            var entityId = Guid.NewGuid().ToString();
            Log($"Spawning {entityType} at ({x}, {y}) with ID {entityId}");
            return entityId;
        }

        /// <summary>
        /// Removes an entity (example game function)
        /// </summary>
        public void RemoveEntity(string entityId)
        {
            Log($"Removing entity {entityId}");
        }
    }
}
