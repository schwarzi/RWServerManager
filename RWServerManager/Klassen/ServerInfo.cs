using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace RWServerManager
{
    [XmlRoot("risingworld-server")]
    public class ServerInfo
    {
        [XmlAttribute("version")]
        public string Version { get; set; }

        [XmlElement("servername")]
        public string ServerName { get; set; }

        [XmlElement("serverip")]
        public string ServerIp { get; set; }

        [XmlElement("serverport")]
        public int ServerPort { get; set; }

        [XmlElement("playercount")]
        public int PlayerCount { get; set; }

        [XmlElement("maxplayercount")]
        public int MaxPlayerCount { get; set; }

        [XmlElement("whitelist")]
        public bool WhiteList { get; set; }

        [XmlElement("locked")]
        public bool Locked { get; set; }

        [XmlElement("lua")]
        public bool Lua { get; set; }

        [XmlElement("verification")]
        public bool Verification { get; set; }

        [XmlElement("pvp")]
        public bool PvP { get; set; }

        [XmlElement("hiveprot")]
        public bool HiveProt { get; set; }

        [XmlIgnore]
        public bool IsOnline { get; set; }

        [XmlIgnore]
        public bool IsUpdating { get; set; }
        public static ServerInfo GetServerInfo(string ip, int port)
        {
            ServerInfo result = null;
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ServerInfo));
                var reader = XmlReader.Create(string.Format("http://{0}:{1}", ip, port - 1));
                result = (ServerInfo)serializer.Deserialize(reader);
                reader.Close();
            }
            catch (Exception)
            {
                result = null;
            }
            return result;
        }
    }
}
