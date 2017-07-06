using System.Reflection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ZapoctakProg2
{
    class PowerUpPluginLoader
    {
        private const string PluginPath = @"..\..\plugin\";
        public static PowerUpPluginLoader Instance { get; } = new PowerUpPluginLoader();

        private Dictionary<string, IPowerUpParser> parsers = new Dictionary<string, IPowerUpParser>();
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
            var reduceTimeParser = new ReduceTimeParser();
            Register(reduceTimeParser.PowerUpId, reduceTimeParser);
        }



        public void LoadPlugins(string path = PluginPath)
        {
            if(!Directory.Exists(path)) return;
            var files = from file in Directory.EnumerateFiles(path)
                where Regex.IsMatch(file, @".*Powerup.*\.dll$")
                select Path.GetFullPath(file);

            foreach (var file in files)
            {
                var assembly = Assembly.LoadFile(file);
                var parser =
                    assembly.DefinedTypes.FirstOrDefault(t => t.ImplementedInterfaces.Contains(typeof(IPowerUpParser)));
                if(parser == null)
                    throw new MissingMemberException($"{file} contains no type implementing {nameof(IPowerUpParser)}");
                
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
