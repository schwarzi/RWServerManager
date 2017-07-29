using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWServerManager
{
    public abstract class PluginType
    {
        
        public virtual void LoadFile(string descriptionPath)
        {

        }

        public abstract string Name { get;  }
    }

    public enum PluginLanguageTypes
    {
        LUA = 0,
        JAVA = 1
    }
}
