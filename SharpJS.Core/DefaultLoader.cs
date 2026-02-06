using System;
using System.IO;
using System.Reflection;
using Puerts;

namespace SharpJS.Core
{
    /// <summary>
    /// Custom loader for PuerTS in .NET environment
    /// Implements ILoader to load JavaScript files from filesystem and embedded resources
    /// </summary>
    public class DefaultLoader : ILoader, IModuleChecker
    {
        private readonly string _rootPath;
        private readonly string _puertsContentPath;

        public DefaultLoader(string? rootPath = null)
        {
            _rootPath = rootPath ?? AppDomain.CurrentDomain.BaseDirectory;
            
            // Find PuerTS content files path
            var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            _puertsContentPath = Path.Combine(userProfile, ".nuget", "packages", "puerts.core", "3.0.0", "contentFiles");
        }

        /// <summary>
        /// Checks if a file path is an ES6 module
        /// </summary>
        public bool IsESM(string filepath)
        {
            // Consider .mjs files as ES modules
            return filepath.EndsWith(".mjs");
        }

        /// <summary>
        /// Reads a file and returns its content along with debug path
        /// </summary>
        public string ReadFile(string filepath, out string debugpath)
        {
            debugpath = filepath;

            // Try to load from PuerTS content files first (for built-in files)
            if (filepath.StartsWith("puerts/"))
            {
                var puertsFilePath = Path.Combine(_puertsContentPath, filepath);
                if (File.Exists(puertsFilePath))
                {
                    debugpath = puertsFilePath;
                    return File.ReadAllText(puertsFilePath);
                }
            }

            // Try to load from filesystem
            var fullPath = Path.IsPathRooted(filepath) 
                ? filepath 
                : Path.Combine(_rootPath, filepath);

            if (File.Exists(fullPath))
            {
                debugpath = fullPath;
                return File.ReadAllText(fullPath);
            }

            // Try with .js extension if not present
            if (!filepath.EndsWith(".js") && !filepath.EndsWith(".mjs") && !filepath.EndsWith(".cjs"))
            {
                var jsPath = fullPath + ".js";
                if (File.Exists(jsPath))
                {
                    debugpath = jsPath;
                    return File.ReadAllText(jsPath);
                }
            }

            // Try to load from embedded resources
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"SharpJS.Core.Scripts.{filepath.Replace("/", ".").Replace("\\", ".")}";
            
            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                if (stream != null)
                {
                    using (var reader = new StreamReader(stream))
                    {
                        debugpath = $"embedded://{resourceName}";
                        return reader.ReadToEnd();
                    }
                }
            }

            throw new FileNotFoundException($"File not found: {filepath}");
        }

        /// <summary>
        /// Checks if a file exists
        /// </summary>
        public bool FileExists(string filepath)
        {
            // Check PuerTS content files
            if (filepath.StartsWith("puerts/"))
            {
                var puertsFilePath = Path.Combine(_puertsContentPath, filepath);
                if (File.Exists(puertsFilePath))
                    return true;
            }

            var fullPath = Path.IsPathRooted(filepath) 
                ? filepath 
                : Path.Combine(_rootPath, filepath);

            if (File.Exists(fullPath))
                return true;

            // Check with .js extension
            if (!filepath.EndsWith(".js") && !filepath.EndsWith(".mjs") && !filepath.EndsWith(".cjs"))
            {
                if (File.Exists(fullPath + ".js"))
                    return true;
            }

            // Check embedded resources
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"SharpJS.Core.Scripts.{filepath.Replace("/", ".").Replace("\\", ".")}";
            return assembly.GetManifestResourceStream(resourceName) != null;
        }
    }
}
