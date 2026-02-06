using System;
using Puerts;

namespace SharpJS.Core
{
    /// <summary>
    /// JavaScript runtime wrapper for PuerTS
    /// Provides the core JavaScript execution environment for game mods
    /// </summary>
    public class JsRuntime : IDisposable
    {
        private readonly ScriptEnv _jsEnv;
        private readonly ILoader _loader;
        private bool _disposed;

        public JsRuntime() : this(new DefaultLoader())
        {
        }

        public JsRuntime(ILoader loader)
        {
            _loader = loader;
            var backend = new BackendV8(loader);
            _jsEnv = new ScriptEnv(backend);
        }

        /// <summary>
        /// Evaluates JavaScript code in the runtime environment
        /// </summary>
        /// <param name="code">JavaScript code to execute</param>
        public void Eval(string code)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(JsRuntime));

            _jsEnv.Eval(code);
        }

        /// <summary>
        /// Evaluates JavaScript code and returns a typed result
        /// </summary>
        /// <typeparam name="T">Expected return type</typeparam>
        /// <param name="code">JavaScript code to execute</param>
        /// <returns>Typed result of the evaluation</returns>
        public T Eval<T>(string code)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(JsRuntime));

            return _jsEnv.Eval<T>(code);
        }

        /// <summary>
        /// Executes a JavaScript module
        /// </summary>
        /// <param name="moduleName">Name/path of the module to execute</param>
        public void ExecuteModule(string moduleName)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(JsRuntime));

            _jsEnv.ExecuteModule(moduleName);
        }

        /// <summary>
        /// Registers a C# object/class to be accessible from JavaScript
        /// </summary>
        /// <param name="name">Name to expose in JavaScript</param>
        /// <param name="obj">Object to expose</param>
        public void RegisterObject(string name, object obj)
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(JsRuntime));

            // Create a wrapper function that stores the object
            var setFunc = _jsEnv.Eval<Action<object>>($@"
                (function(obj) {{ 
                    if (typeof global === 'undefined') globalThis['{name}'] = obj;
                    else global['{name}'] = obj;
                }})
            ");
            setFunc(obj);
        }

        /// <summary>
        /// Ticks the JavaScript environment to process pending tasks
        /// Should be called regularly to handle async operations
        /// </summary>
        public void Tick()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(JsRuntime));

            _jsEnv.Tick();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _jsEnv?.Dispose();
            _disposed = true;
        }
    }
}
