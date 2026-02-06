using System;
using System.IO;
using System.Reflection;
using Puerts;

namespace SharpJS.Core
{
    /// <summary>
    /// File loader implementation for PuerTS in standalone .NET applications
    /// Loads scripts from filesystem and NuGet package content
    /// </summary>
    public class ScriptFileLoader : ILoader, IModuleChecker
    {
        private readonly string _baseDirectory;
        private readonly string _puertsPackageContentPath;

        public ScriptFileLoader(string? baseDirectory = null)
        {
            _baseDirectory = baseDirectory ?? AppDomain.CurrentDomain.BaseDirectory;
            
            var homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _puertsPackageContentPath = Path.Combine(homeDirectory, ".nuget", "packages", 
                "puerts.core", "3.0.0", "contentFiles");
        }

        public bool IsESM(string scriptPath)
        {
            return scriptPath.EndsWith(".mjs", StringComparison.OrdinalIgnoreCase);
        }

        public string ReadFile(string scriptPath, out string debugInfo)
        {
            debugInfo = scriptPath;

            // Priority 1: Check PuerTS built-in files
            if (scriptPath.StartsWith("puerts/"))
            {
                var packageFilePath = Path.Combine(_puertsPackageContentPath, scriptPath);
                if (File.Exists(packageFilePath))
                {
                    debugInfo = packageFilePath;
                    return File.ReadAllText(packageFilePath);
                }
            }

            // Priority 2: Check absolute or relative paths
            var absolutePath = Path.IsPathRooted(scriptPath) 
                ? scriptPath 
                : Path.Combine(_baseDirectory, scriptPath);

            if (File.Exists(absolutePath))
            {
                debugInfo = absolutePath;
                return File.ReadAllText(absolutePath);
            }

            // Priority 3: Try with .js extension
            if (!HasScriptExtension(scriptPath))
            {
                var withExtension = absolutePath + ".js";
                if (File.Exists(withExtension))
                {
                    debugInfo = withExtension;
                    return File.ReadAllText(withExtension);
                }
            }

            // Priority 4: Check embedded resources
            var resourceContent = TryLoadFromEmbeddedResources(scriptPath, out var resourcePath);
            if (resourceContent != null)
            {
                debugInfo = resourcePath;
                return resourceContent;
            }

            throw new FileNotFoundException($"Script file not found: {scriptPath}");
        }

        public bool FileExists(string scriptPath)
        {
            if (scriptPath.StartsWith("puerts/"))
            {
                var packageFilePath = Path.Combine(_puertsPackageContentPath, scriptPath);
                if (File.Exists(packageFilePath))
                    return true;
            }

            var absolutePath = Path.IsPathRooted(scriptPath) 
                ? scriptPath 
                : Path.Combine(_baseDirectory, scriptPath);

            if (File.Exists(absolutePath))
                return true;

            if (!HasScriptExtension(scriptPath) && File.Exists(absolutePath + ".js"))
                return true;

            return TryLoadFromEmbeddedResources(scriptPath, out _) != null;
        }

        private bool HasScriptExtension(string path)
        {
            return path.EndsWith(".js") || path.EndsWith(".mjs") || path.EndsWith(".cjs");
        }

        private string? TryLoadFromEmbeddedResources(string scriptPath, out string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var normalizedPath = scriptPath.Replace("/", ".").Replace("\\", ".");
            var resourceName = $"{assembly.GetName().Name}.Scripts.{normalizedPath}";
            
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream != null)
            {
                using var reader = new StreamReader(stream);
                resourcePath = $"resource://{resourceName}";
                return reader.ReadToEnd();
            }

            resourcePath = string.Empty;
            return null;
        }
    }
}
