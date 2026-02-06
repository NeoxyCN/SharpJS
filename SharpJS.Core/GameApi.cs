using System;
using System.Collections.Generic;

namespace SharpJS.Core
{
    /// <summary>
    /// Bridge API between native code and JavaScript plugins
    /// Provides safe methods for plugin interaction with the host application
    /// </summary>
    public class HostBridge
    {
        private readonly Dictionary<string, Action<string>> _callbackRegistry;
        private readonly Dictionary<string, object> _sharedDataStore;

        public HostBridge()
        {
            _callbackRegistry = new Dictionary<string, Action<string>>();
            _sharedDataStore = new Dictionary<string, object>();
        }

        /// <summary>
        /// Writes a message to the console from plugin code
        /// </summary>
        public void WriteMessage(string content)
        {
            Console.WriteLine($"[PLUGIN] {content}");
        }

        /// <summary>
        /// Registers a callback for a named event
        /// </summary>
        public void RegisterCallback(string eventIdentifier, Action<string> callback)
        {
            if (!_callbackRegistry.ContainsKey(eventIdentifier))
            {
                _callbackRegistry[eventIdentifier] = callback;
            }
            else
            {
                _callbackRegistry[eventIdentifier] += callback;
            }
        }

        /// <summary>
        /// Triggers a registered event with optional data
        /// </summary>
        public void TriggerEvent(string eventIdentifier, string payload = "")
        {
            if (_callbackRegistry.ContainsKey(eventIdentifier))
            {
                try
                {
                    _callbackRegistry[eventIdentifier]?.Invoke(payload);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in callback for {eventIdentifier}: {ex.Message}");
                }
            }
        }

        /// <summary>
        /// Retrieves data from the shared store
        /// </summary>
        public object? RetrieveData(string dataKey)
        {
            return _sharedDataStore.ContainsKey(dataKey) ? _sharedDataStore[dataKey] : null;
        }

        /// <summary>
        /// Stores data in the shared store
        /// </summary>
        public void StoreData(string dataKey, object dataValue)
        {
            _sharedDataStore[dataKey] = dataValue;
        }

        /// <summary>
        /// Gets the current application timestamp
        /// </summary>
        public double GetCurrentTimestamp()
        {
            return DateTime.Now.TimeOfDay.TotalSeconds;
        }

        /// <summary>
        /// Creates a new entity in the application (example functionality)
        /// </summary>
        public string CreateEntity(string entityClass, double positionX, double positionY)
        {
            var entityIdentifier = Guid.NewGuid().ToString();
            WriteMessage($"Creating {entityClass} at position ({positionX}, {positionY}) with ID {entityIdentifier}");
            return entityIdentifier;
        }

        /// <summary>
        /// Destroys an entity by its identifier (example functionality)
        /// </summary>
        public void DestroyEntity(string entityIdentifier)
        {
            WriteMessage($"Destroying entity {entityIdentifier}");
        }
    }
}
