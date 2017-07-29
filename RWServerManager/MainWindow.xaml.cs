using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using Newtonsoft.Json;
using System.Net;
using System.Globalization;

namespace RWServerManager
{
    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private Config _config = Config.Instance;
        private string _luaPath = "scripts";
        private string _pluginPath = "plugins";
        private string _jdkPath = "";

        private readonly MVVM _viewModel;
        public MainWindow()
        {
            _viewModel = new MVVM();
            this.DataContext = _viewModel;
            InitializeComponent();
        }
        private BitmapImage GetLanguageIcon(string lang)
        {
            BitmapImage img = new BitmapImage();
            img.BeginInit();
            switch (lang.ToLower())
            {
                default:
                case "de": img.UriSource = new Uri("Assets/Icons/DE.png", UriKind.Relative); break;
                case "en": img.UriSource = new Uri("Assets/Icons/EN.png", UriKind.Relative); break;
            }
            img.EndInit();
            return img;
        }

        private void MetroWindow_Loaded(object sender, RoutedEventArgs e)
        {

            _viewModel.LanguageSelector = new List<VMLanguage>()
            {
                new VMLanguage()
                {
                    LangIcon = GetLanguageIcon("DE"),
                    Language = "Deutsch",
                    Short = "DE"
                },
                new VMLanguage()
                {
                    LangIcon = GetLanguageIcon("EN"),
                    Language = "English",
                    Short = "EN"
                }
            };

            _viewModel.WeekDaySelector = new List<RunTimeDay>()
            {
                new RunTimeDay()
                {
                    Name = "täglich",
                    Selector = -1,
                    WeekDay = DayOfWeek.Sunday
                },
                new RunTimeDay()
                {
                    Name = "Sonntag",
                    Selector = 0,
                    WeekDay = DayOfWeek.Sunday
                },
                new RunTimeDay()
                {
                    Name = "Montag",
                    Selector = 1,
                    WeekDay = DayOfWeek.Monday
                },
                new RunTimeDay()
                {
                    Name = "Dienstag",
                    Selector = 2,
                    WeekDay = DayOfWeek.Tuesday
                },
                new RunTimeDay()
                {
                    Name = "Mittwoch",
                    Selector = 3,
                    WeekDay = DayOfWeek.Wednesday
                },
                new RunTimeDay()
                {
                    Name = "Donnerstag",
                    Selector = 4,
                    WeekDay = DayOfWeek.Thursday
                },
                new RunTimeDay()
                {
                    Name = "Freitag",
                    Selector = 5,
                    WeekDay = DayOfWeek.Friday
                },
                new RunTimeDay()
                {
                    Name = "Sonnabend",
                    Selector = 6,
                    WeekDay = DayOfWeek.Saturday
                },
            };

            if (!_config.Load())
            {
                //Programm starten aber im SetupDialog
            }
            else
            {
                
                _viewModel.Language = String.IsNullOrEmpty(_config.MyConfig.Language) ? "DE" : _config.MyConfig.Language;
                _viewModel.MyServer = _config.MyConfig.Server;
                _viewModel.Runtimes = _config.MyConfig.Restarts;

                #region Laden der ServerPlugins(LUA und JAVA)
                #region LuaPlugins
                string serverLuaPath = System.IO.Path.Combine(_viewModel.MyServer.Path, _luaPath);
                var allLuaPlugins = System.IO.Directory.GetDirectories(serverLuaPath);
                if (allLuaPlugins != null && allLuaPlugins.Count() > 0)
                {
                    foreach (var luaFile in allLuaPlugins)
                    {
                        LuaPlugin luaPlugin = new LuaPlugin();
                        luaPlugin.LoadFile(allLuaPlugins[0]);
                        if (luaPlugin != null && luaPlugin.LuaData != null)
                        {
                            if (_viewModel.ServerPlugins == null)
                                _viewModel.ServerPlugins = new List<PluginType>();

                            _viewModel.ServerPlugins.Add(luaPlugin);
                        }
                    }
                }
                #endregion

                #region JavaPlugins
                string serverJavaPath = System.IO.Path.Combine(_viewModel.MyServer.Path, _pluginPath);
                if (System.IO.Directory.Exists(serverJavaPath))
                {
                    var allJavaPlugins = System.IO.Directory.GetDirectories(serverJavaPath);
                    if (allJavaPlugins != null && allJavaPlugins.Count() > 0)
                    {
                        foreach (var jarFile in allJavaPlugins)
                        {
                            JavaPlugin javaPlugin = new JavaPlugin();
                            javaPlugin.LoadFile(jarFile);
                            if (javaPlugin != null && javaPlugin.JavaData != null)
                            {
                                if (_viewModel.ServerPlugins == null)
                                    _viewModel.ServerPlugins = new List<PluginType>();

                                _viewModel.ServerPlugins.Add(javaPlugin);
                            }
                        }
                    }
                }
                #endregion
                #endregion

                #region laden der Server.properties
                var serverPropertyFile = System.IO.Path.Combine(_viewModel.MyServer.Path, "server.properties");
                if (System.IO.File.Exists(serverPropertyFile))
                {
                    var properties = ReadPropertyFile(serverPropertyFile);
                    var json = JsonConvert.SerializeObject(properties, Formatting.Indented);

                    _viewModel.ServerProperties = (ServerProperty)JsonConvert.DeserializeObject<ServerProperty>(json);
                }
                #endregion
            }
        }

        private IDictionary<string,object> ReadPropertyFile(string fileName)
        {
            Dictionary<string, object> result = null;
            foreach(string line in System.IO.File.ReadAllLines(fileName))
            {
                object val = null;
                if((!string.IsNullOrEmpty(line)) && (!line.StartsWith(";")) && (!line.StartsWith("#")) 
                    && (!line.StartsWith("'")) && (line.Contains("=")))
                {
                    int index = line.IndexOf("=");
                    string key = line.Substring(0, index).Trim();
                    string value = line.Substring(index + 1).Trim();

                    if ((value.StartsWith("\"") && value.EndsWith("\""))
                        || (value.StartsWith("'") && value.EndsWith("'")))
                        value = value.Substring(1, value.Length - 2);
                    #region TypenPrüfung
                    if (value.ToLower() == "true" || value.ToLower() == "false")
                        val = bool.Parse(value);
                    else if (int.TryParse(value, out int pInt))
                        val = (int)pInt;
                    else if (double.TryParse(value, out double dbl))
                        val = (double)dbl;
                    else
                        val = value;
                    #endregion
                    if (result == null)
                        result = new Dictionary<string, object>();
                   result.Add(key, val);
                }
            }
            return result;
        }

        private void btnSetup_Click(object sender, RoutedEventArgs e)
        {
            flSetup.IsOpen = !flSetup.IsOpen;
        }
    }

    public class VMLanguage
    {
        public BitmapImage LangIcon { get; set; }

        public string Language { get; set; }

        public string Short { get; set; }
    }

    public class SelectedRuntimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return TimeSpan.FromSeconds(0);
            if (TimeSpan.TryParse(value.ToString(), out TimeSpan ts))
                return ts;
            else
                return TimeSpan.FromSeconds(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null)
                return ((TimeSpan)value).ToString("hh:mm");
            else
                return "00:00";
        }
    }

    public class RestartTaskConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "Restart";
            if ((bool)value)
                return "Update";
            return "Restart";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
