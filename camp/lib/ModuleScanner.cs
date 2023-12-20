using System.IO;
using YamlDotNet.Serialization;

namespace camp.lib
{

    public delegate void OnLoadedModule(Module module);


    public class tempModule
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public tempCommand Command { get; set; }

    }

    public class tempCommand
    {
        public string Start { get; set; }
        public string Stop { get; set; }
    }

    public partial class ModuleScanner
    {

        public ModuleScanner(OnLoadedModule onLoadedModule)
        {
            string baseDirectory = "E:\\Camp\\"; // AppDomain.CurrentDomain.BaseDirectory;
            string searchPattern = "CAMP_MODULE.yml";

            IEnumerable<string> yamlFiles = Directory.GetFiles(baseDirectory, searchPattern, SearchOption.AllDirectories);


            foreach (string yamlFile in yamlFiles)
            {
                try
                {

                    string yamlContent = File.ReadAllText(yamlFile);
                    var deserializer = new DeserializerBuilder().Build();
                    Module module = deserializer.Deserialize<Module>(yamlContent);
                    module.Path = Path.GetDirectoryName(yamlFile);

                    onLoadedModule.Invoke(Decode(module));

                    Log.WriteLine("ModuleLoader", $"Module Loaded : {module.Name}, Version {module.Version}");

                }
                catch (YamlDotNet.Core.YamlException ex)
                {
                    Log.WriteLine("ModuleLoader", $"YAML Exception in {yamlFile}: {ex.Message}");
                }
                catch (Exception ex)
                {
                    Log.WriteLine("ModuleLoader", $"Error processing {yamlFile}: {ex.Message}");
                }
            }
        }


        public static Module Decode(Module module)
        {

            Dictionary<string, string> SET_ARGS = new()
            {
                { "CURRENT_PATH", module.Path },
                { "CAMP_INSTALL", AppDomain.CurrentDomain.BaseDirectory }
            };


            foreach (var set in SET_ARGS)
            {
                string key = set.Key;
                string value = set.Value;

                module.Command.Start = module.Command.Start.Replace($"${{{key}}}", value);
                module.Command.Stop = module.Command.Stop.Replace($"${{{key}}}", value);
            }

            return module;
        }
    }
}
