using System;
using Puerts;

namespace SharpJS.Core
{
    /// <summary>
    /// JavaScript execution environment manager using PuerTS
    /// Handles script compilation and execution with proper lifecycle
    /// </summary>
    public class ScriptEnvironment : IDisposable
    {
        private readonly ScriptEnv _environment;
        private readonly ILoader _scriptLoader;
        private readonly JsEngineType _engineType;
        private bool _isDisposed;

        /// <summary>
        /// Gets the JavaScript engine type used by this environment
        /// </summary>
        public JsEngineType EngineType => _engineType;

        /// <summary>
        /// Creates a new ScriptEnvironment with V8 engine (default)
        /// </summary>
        public ScriptEnvironment() : this(JsEngineType.V8)
        {
        }

        /// <summary>
        /// Creates a new ScriptEnvironment with the specified engine type
        /// </summary>
        /// <param name="engineType">The JavaScript engine to use</param>
        public ScriptEnvironment(JsEngineType engineType) : this(engineType, new ScriptFileLoader())
        {
        }

        /// <summary>
        /// Creates a new ScriptEnvironment with the specified engine type and loader
        /// </summary>
        /// <param name="engineType">The JavaScript engine to use</param>
        /// <param name="scriptLoader">The script loader to use</param>
        public ScriptEnvironment(JsEngineType engineType, ILoader scriptLoader)
        {
            _scriptLoader = scriptLoader;
            _engineType = engineType;
            var backend = BackendFactory.CreateBackend(engineType, scriptLoader);
            _environment = new ScriptEnv(backend);
        }

        /// <summary>
        /// Creates a new ScriptEnvironment with V8 engine and custom loader (for backward compatibility)
        /// </summary>
        /// <param name="scriptLoader">The script loader to use</param>
        public ScriptEnvironment(ILoader scriptLoader) : this(JsEngineType.V8, scriptLoader)
        {
        }

        /// <summary>
        /// Executes JavaScript code without returning a value
        /// </summary>
        public void Execute(string sourceCode)
        {
            ThrowIfDisposed();
            _environment.Eval(sourceCode);
        }

        /// <summary>
        /// Executes JavaScript and returns the result as specified type
        /// </summary>
        public TResult Execute<TResult>(string sourceCode)
        {
            ThrowIfDisposed();
            return _environment.Eval<TResult>(sourceCode);
        }

        /// <summary>
        /// Runs a JavaScript module by name
        /// </summary>
        public void RunModule(string modulePath)
        {
            ThrowIfDisposed();
            _environment.ExecuteModule(modulePath);
        }

        /// <summary>
        /// Binds a .NET object to JavaScript global scope
        /// </summary>
        public void BindGlobalObject(string identifier, object instance)
        {
            ThrowIfDisposed();
            
            var bindingFunction = _environment.Eval<Action<object>>($@"
                (function(obj) {{ 
                    var target = typeof global !== 'undefined' ? global : globalThis;
                    target['{identifier}'] = obj;
                }})
            ");
            bindingFunction(instance);
        }

        /// <summary>
        /// Processes pending JavaScript tasks and promises
        /// </summary>
        public void ProcessTasks()
        {
            ThrowIfDisposed();
            _environment.Tick();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
                throw new ObjectDisposedException(nameof(ScriptEnvironment));
        }

        public void Dispose()
        {
            if (_isDisposed) return;
            
            _environment?.Dispose();
            _isDisposed = true;
        }
    }
}
