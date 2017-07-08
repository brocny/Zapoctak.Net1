using System.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace ZapoctakProg2
{
    public class PowerUpPluginLoader
    {
        private const string DefaultPluginPath = @"..\..\plugin\";
        /// <summary>
        /// Singleton design pattern
        /// </summary>
        public static PowerUpPluginLoader Instance { get; } = new PowerUpPluginLoader();

        private readonly Dictionary<string, IPowerUpParser> parsers = new Dictionary<string, IPowerUpParser>();
        public IReadOnlyDictionary<string, IPowerUpParser> Parsers => parsers;

        public void Register(string key, IPowerUpParser value)
        {
            if (!parsers.ContainsKey(key))
            {
                parsers.Add(key, value);
            }
        }

        public PowerUpPluginLoader()
        {
            // Register hardcoded powerUps
            var reduceTimeParser = new ReduceTimeParser();
            Register(reduceTimeParser.PowerUpId, reduceTimeParser);
        }


        /// <summary>
        /// Load plugins from all files in the directory with names matching regex <code>.*[p|P]ower[-|_]?[u|U]p.*\.dll$</code>
        /// </summary>
        /// <param name="path">Plugin directory</param>
        public void LoadPlugins(string path = DefaultPluginPath)
        {
            if(!Directory.Exists(path)) return;
            var files = from file in Directory.EnumerateFiles(path)
                where Regex.IsMatch(file, @".*[p|P]ower[-|_]?[u|U]p.*\.dll$")
                select Path.GetFullPath(file);

            foreach (var file in files)
            {
                var assembly = Assembly.LoadFile(file);
                var definedParsers = from parser in assembly.DefinedTypes
                    where parser.ImplementedInterfaces.Contains(typeof(IPowerUpParser))
                    select parser;
                if(!definedParsers.Any())
                    throw new MissingMemberException($"{Path.GetFileName(file)} contains no type implementing {nameof(IPowerUpParser)}");

                foreach (var parser in definedParsers)
                {
                    // parameterless contructor
                    if (parser.GetConstructor(new Type[0]) == null)
                        throw new MissingMemberException($"{parser} in {file} has no parameterless constructor!");

                    var instance = (IPowerUpParser)Activator.CreateInstance(parser.AsType());
                    if (instance != null)
                    {
                        Register(instance.PowerUpId, instance);
                    }
                }
                
            }
        }
    }
}
