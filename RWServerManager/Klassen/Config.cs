using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RWServerManager
{
    public class Config
    {
        private static Config m_Instace;
        private readonly string _appPath;
        private Config()
        {
            _appPath = Path.GetDirectoryName(Assembly.GetAssembly(typeof(Config)).Location);
        }

        public static Config Instance
        {
            get
            {
                if (m_Instace == null)
                    m_Instace = new Config();
                return m_Instace;
            }
        }

        public bool Load()
        {
            bool result = false;
            try
            {
                Error = null;
                UpdaterUrl = "http://wdw-community.de/rwupdate.xml";
                string cfgFile = Path.Combine(_appPath, "config.xml");
                if (!File.Exists(cfgFile))
                    throw new FileNotFoundException("Die Datei '" + cfgFile + "' konnte nicht gefunden werden!");

                XmlSerializer serializer = new XmlSerializer(typeof(ConfigData));
                TextReader reader = new StreamReader(cfgFile);
                MyConfig = (ConfigData)serializer.Deserialize(reader);
                reader.Close();

                if (MyConfig == null)
                    throw new Exception("Fehler beim laden der Konfiguration aufgetreten!");

                result = true;

            }
            catch (Exception ex)
            {
                Error = ex;
                result = false;
            }
            return result;
        }

        public Exception Error { get; private set; }

        public ConfigData MyConfig { get; set; }
        public string AppPath { get; set; }

        public string UpdaterUrl { get; private set; }
    }

    [XmlRoot("ManagerConfig")]
    public class ConfigData
    {
        public string Language { get; set; }

        public ServerConfig Server { get; set; }

        [XmlArray("Restarts")]
        [XmlArrayItem("Task")]
        public List<RuntimeConfig> Restarts { get; set; }
    }

    public class ServerConfig
    {
        [XmlElement("IP")]
        public string Ip { get; set; }

        [XmlElement("ServerPath")]
        public string Path { get; set; }
    }

    public class RuntimeConfig
    {
        [XmlAttribute("Runtime")]
        public string Runtime { get; set; }

        [XmlAttribute("Update")]
        public bool Update { get; set; }

        [XmlAttribute("WeekDay")]
        public int WeekDay { get; set; }

        [XmlIgnore]
        public string RuntimeView
        {
            get
            {
                string wday = "";
                switch (WeekDay)
                {
                    default:
                    case -1: wday = "Täglich"; break;
                    case 0: wday = "Sonntag"; break;
                    case 1: wday = "Montag"; break;
                    case 2: wday = "Dienstag"; break;
                    case 3: wday = "Mittwoch"; break;
                    case 4: wday = "Donnerstag"; break;
                    case 5: wday = "Freitag"; break;
                    case 6: wday = "Sonnabend"; break;
                }

                return string.Format("{0} {1} Uhr", wday, Runtime);
            }
        }
    }
}
