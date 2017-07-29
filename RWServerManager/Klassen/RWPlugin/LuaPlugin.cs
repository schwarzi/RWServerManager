using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RWServerManager
{
    public class LuaPlugin : PluginType
    {
        string fileName = "definition.xml";
        public override void LoadFile(string descriptionPath)
        {
            try
            {
                Error = null;
                XmlSerializer serializer = new XmlSerializer(typeof(LuaConfigData));
                var luaDefininition = System.IO.Path.Combine(descriptionPath, fileName);
                if (!System.IO.File.Exists(luaDefininition))
                    throw new System.IO.FileNotFoundException("Die Datei '" + luaDefininition + "' konnte nicht gefunden werden!");
                var luaXmlStream = new System.IO.StreamReader(luaDefininition);
                var luaXmlString = System.IO.File.ReadAllText(luaDefininition);
                if (!luaXmlString.StartsWith("<?xml"))
                    luaXmlString = "<?xml version=\"1.0\" encoding=\"utf - 8\" ?>" + luaXmlString;

                System.IO.TextReader reader = new System.IO.StringReader(luaXmlString);
                LuaData = (LuaConfigData)serializer.Deserialize(reader);reader.Close();

            }
            catch (Exception ex)
            {
                Error = ex;
                LuaData = null;
            }
        }

        public Exception Error { get; private set; }

        public LuaConfigData LuaData { get; private set; }

        public override string Name
        {
            get
            {
                if (LuaData != null)
                    return LuaData.Name;
                else return String.Empty;
            }
        }

        public PluginLanguageTypes PluginType { get { return PluginLanguageTypes.LUA; } }

//        [Obsolete("Achtung Lua-Scripte werden bald von Rising World nichtmehr unterstützt!", false)]
        [XmlRoot("Lua-Script")]
        public  class LuaConfigData
        {
            public string Name { get; set; }

            public string Description { get; set; }

            public string File { get; set; }

            public string Author { get; set; }

            public string Website { get; set; }

            public string License { get; set; }
        }
    }
}
