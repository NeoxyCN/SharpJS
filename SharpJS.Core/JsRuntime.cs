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
        private bool _isDisposed;

        public ScriptEnvironment() : this(new ScriptFileLoader())
        {
        }

        public ScriptEnvironment(ILoader scriptLoader)
        {
            _scriptLoader = scriptLoader;
            var v8Backend = new BackendV8(scriptLoader);
            _environment = new ScriptEnv(v8Backend);
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
