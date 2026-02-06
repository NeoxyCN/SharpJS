namespace SharpJS.Core
{
    /// <summary>
    /// Specifies the JavaScript engine to use for script execution
    /// </summary>
    public enum JsEngineType
    {
        /// <summary>
        /// V8 JavaScript engine (default, high performance)
        /// </summary>
        V8,

        /// <summary>
        /// QuickJS JavaScript engine (lightweight, embedded-friendly)
        /// </summary>
        QuickJS,

        /// <summary>
        /// Node.js JavaScript engine (full Node.js API support)
        /// </summary>
        NodeJS
    }
}
