using Puerts;

namespace SharpJS.Core
{
    /// <summary>
    /// Factory for creating PuerTS backends based on engine type
    /// </summary>
    internal static class BackendFactory
    {
        /// <summary>
        /// Creates a backend instance for the specified engine type
        /// </summary>
        /// <param name="engineType">The JavaScript engine type to use</param>
        /// <param name="loader">The script loader to use</param>
        /// <returns>A backend instance</returns>
        /// <exception cref="NotSupportedException">Thrown when the specified engine type is not supported</exception>
        public static Backend CreateBackend(JsEngineType engineType, ILoader loader)
        {
            return engineType switch
            {
                JsEngineType.V8 => new BackendV8(loader),
                JsEngineType.QuickJS => new BackendQuickJS(loader),
                JsEngineType.NodeJS => new BackendNodeJS(loader),
                _ => throw new NotSupportedException($"Unsupported JavaScript engine type: {engineType}")
            };
        }

        /// <summary>
        /// Checks if the specified engine type is available
        /// </summary>
        /// <param name="engineType">The engine type to check</param>
        /// <returns>True if the engine is available, false otherwise</returns>
        public static bool IsEngineAvailable(JsEngineType engineType)
        {
            try
            {
                var typeName = engineType switch
                {
                    JsEngineType.V8 => "Puerts.BackendV8",
                    JsEngineType.QuickJS => "Puerts.BackendQuickJS",
                    JsEngineType.NodeJS => "Puerts.BackendNodeJS",
                    _ => null
                };

                if (typeName == null) return false;

                // Check if the type is available in loaded assemblies
                foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (assembly.GetType(typeName) != null)
                        return true;
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Gets all available engine types
        /// </summary>
        /// <returns>An array of available engine types</returns>
        public static JsEngineType[] GetAvailableEngines()
        {
            var available = new List<JsEngineType>();
            foreach (JsEngineType engineType in Enum.GetValues(typeof(JsEngineType)))
            {
                if (IsEngineAvailable(engineType))
                    available.Add(engineType);
            }
            return available.ToArray();
        }
    }
}
