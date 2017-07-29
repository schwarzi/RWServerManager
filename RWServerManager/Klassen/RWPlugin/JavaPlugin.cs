using ICSharpCode.SharpZipLib.Zip;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace RWServerManager
{
    public class JavaPlugin : PluginType
    {
        string fileName = "plugin.yml";

        public override void LoadFile(string descriptionPath)
        {
            try
            {
                var jarFile = System.IO.Directory.GetFiles(descriptionPath, "*.jar");
                string pluginFile = System.IO.Path.Combine(descriptionPath, jarFile[0]);
                if (!System.IO.File.Exists(pluginFile))
                    throw new System.IO.FileNotFoundException("Fehler beim laden der plugin.yaml");

                System.IO.FileStream fs = System.IO.File.OpenRead(pluginFile);
                var zf = new ZipFile(fs);
                foreach (ZipEntry zipEntry in zf)
                {
                    string EntryFileNAme = zipEntry.Name;
                    if (EntryFileNAme.ToLower().Equals("resources/plugin.yml"))
                    {
                        System.IO.Stream zipstream = zf.GetInputStream(zipEntry);
                        System.IO.TextReader reader = new System.IO.StreamReader(zipstream);
                        var deserializer = new DeserializerBuilder()
                            .WithNamingConvention(new CamelCaseNamingConvention())
                            .Build();


                        JavaData = deserializer.Deserialize<YamlData>(reader);
                        reader.Close();

                    }
                }
            }
            catch (Exception ex)
            {
                Error = ex;
                JavaData = null;
            }
        }

        public YamlData JavaData { get; private set; }

        public Exception Error { get; private set; }
        public class YamlData
        {
            [YamlMember(Alias = "name", ApplyNamingConventions = false)]
            public string Name { get; set; }

            [YamlMember(Alias = "main", ApplyNamingConventions = false)]
            public string Main { get; set; }

            [YamlMember(Alias = "version", ApplyNamingConventions = false)]
            public string Version { get; set; }

            [YamlMember(Alias = "author", ApplyNamingConventions = false)]
            public string Author { get; set; }

            [YamlMember(Alias = "team", ApplyNamingConventions = false)]
            public string Team { get; set; }

            [YamlMember(Alias = "description", ApplyNamingConventions = false)]
            public string Description { get; set; }

            [YamlMember(Alias = "license", ApplyNamingConventions = false)]
            public string License { get; set; }

            [YamlMember(Alias = "website", ApplyNamingConventions = false)]
            public string Website { get; set; }
        }

        public PluginLanguageTypes PluginType { get { return PluginLanguageTypes.JAVA; } }

        public override string Name
        {
            get
            {
                if (JavaData != null)
                    return JavaData.Name;
                else
                    return String.Empty;
            }
        }
    }
}
