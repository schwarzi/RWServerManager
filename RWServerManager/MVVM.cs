using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RWServerManager
{
    public class MVVM : INotifyPropertyChanged, IDataErrorInfo
    {
        private List<PluginType> _serverPlugins;
        private List<RuntimeConfig> _runtimes;
        private ServerConfig _serverConfig;
        private string _language;
        private List<VMLanguage> _vmLanguages;
        private List<RunTimeDay> _weekDaySelector;

        public List<RunTimeDay> WeekDaySelector
        {
            get { return _weekDaySelector; }
            set
            {
                if (Equals(value, _weekDaySelector)) return;
                _weekDaySelector = value;
                RaisePropertyChanged("WeekDaySelector");
            }
        }
        public List<VMLanguage> LanguageSelector
        {
            get { return _vmLanguages; }
            set
            {
                if (Equals(value, _vmLanguages)) return;
                _vmLanguages = value;
                RaisePropertyChanged("LanguageSelector");
            }
        }

        private ServerProperty _serverProperty;

        public List<PluginType> ServerPlugins
        {
            get { return _serverPlugins; }
            set
            {
                if (Equals(value, _serverPlugins)) return;
                _serverPlugins = value;
                RaisePropertyChanged("ServerPlugins");
            }
        }

        public List<RuntimeConfig> Runtimes
        {
            get { return _runtimes; }
            set
            {
                if (Equals(value, _runtimes)) return;
                _runtimes = value;
                RaisePropertyChanged("Runtimes");
            }
        }

        public ServerConfig MyServer
        {
            get { return _serverConfig; }
            set
            {
                if (Equals(value, _serverConfig)) return;
                _serverConfig = value;
                RaisePropertyChanged("MyServer");
            }
        }

        public string Language
        {
            get { return _language; }
            set
            {
                if (Equals(value, _language)) return;
                _language = value;
                RaisePropertyChanged("Language");
            }
        }

        public ServerProperty ServerProperties
        {
            get { return _serverProperty; }
            set
            {
                if (Equals(value, _serverProperty)) return;
                _serverProperty = value;
                RaisePropertyChanged("ServerProperties");
            }
        }

        #region InterfaceMethoden
        public string this[string columnName]
        {
            get
            {
                return String.Empty;
            }
        }

        public string Error
        {
            get
            {
                return String.Empty;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void RaisePropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
