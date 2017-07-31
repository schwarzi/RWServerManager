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
using Ookii.Dialogs.Wpf;
using System.Threading;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Core;
using System.Data;
using System.Data.SQLite;
using MySql.Data.MySqlClient;
using System.Reflection;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Xml;
using MahApps.Metro.Controls.Dialogs;

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
        private ServerStatusWatcher _myWatcher;
        private System.Diagnostics.Process _serverProcess;
        private System.IO.StreamWriter _processInput;
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
            var thisVersion = Assembly.GetExecutingAssembly().Location;
            FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(thisVersion);
            sbiVersion.Content = fvi.ProductVersion + " ALPHA";

            string xmlUrl = "http://wdw-community.de/RWSWUpdate.xml";
            try
            {
                var reader = new XmlTextReader(xmlUrl);
                var uSerializer = new XmlSerializer(typeof(UpdateData));
                var updData = (UpdateData)uSerializer.Deserialize(reader);

                if(updData != null)
                {
                    Version newVersion = new Version(updData.Version);
                    Version oldVersion = new Version(fvi.ProductVersion);
                    if(oldVersion.CompareTo(newVersion) < 0)
                    {
                        ShowUpdateDialog();
                        return;
                    }
                }
               
            }
            catch (Exception ex)
            {

            }

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
                _viewModel.JAVAPath = Environment.GetEnvironmentVariable("JAVA_HOME");

                if (System.IO.Directory.Exists(_viewModel.MyServer.Path))
                {
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
                        var json = JsonConvert.SerializeObject(properties, Newtonsoft.Json.Formatting.Indented);

                        _viewModel.ServerProperties = (ServerProperty)JsonConvert.DeserializeObject<ServerProperty>(json);
                        if (_viewModel.ServerProperties != null)
                        {
                            DataSet serverDS;
                            switch (_viewModel.ServerProperties.database_type.ToLower())
                            {
                                default:
                                case "sqlite":
                                    {
                                        using (SQLiteConnection conn = new SQLiteConnection(string.Format("Data Source={0}\\Worlds\\{1}\\{1}.db", _viewModel.MyServer.Path, _viewModel.ServerProperties.server_world_name)))
                                        {
                                            using (SQLiteDataAdapter da = new SQLiteDataAdapter("SELECT name FROM sqlite_master WHERE type = 'table' ORDER BY 1", conn))
                                            {
                                                da.MissingSchemaAction = MissingSchemaAction.AddWithKey;
                                                serverDS = new DataSet();
                                                conn.Open();

                                                da.Fill(serverDS);
                                                if (serverDS.Tables != null && serverDS.Tables.Count > 0)
                                                {
                                                    var dt = serverDS.Tables[0];
                                                    foreach (DataRow row in dt.Rows)
                                                    {
                                                        if (!row["name"].ToString().ToLower().Equals("sqlite_sequence"))
                                                        {
                                                            if (_viewModel.ServerTables == null)
                                                                _viewModel.ServerTables = new List<string>();
                                                            _viewModel.ServerTables.Add(row.ItemArray[0].ToString());
                                                        }
                                                    }
                                                }
                                            }
                                        }


                                    }
                                    break;
                                case "mysql":
                                    {
                                        MySqlConnectionStringBuilder ssb = new MySqlConnectionStringBuilder();
                                        ssb.Database = _viewModel.ServerProperties.database_mysql_database;
                                        ssb.Password = _viewModel.ServerProperties.database_mysql_password;
                                        ssb.Port = (uint)_viewModel.ServerProperties.database_mysql_server_port;
                                        ssb.Server = _viewModel.ServerProperties.database_mysql_server_ip;
                                        ssb.UserID = _viewModel.ServerProperties.database_mysql_user;
                                        using (MySqlConnection conn = new MySqlConnection(ssb.ConnectionString))
                                        {
                                            using (MySqlCommand cmd = conn.CreateCommand())
                                            {
                                                cmd.CommandText = "SHOW TABLES";
                                                conn.Open();
                                                MySqlDataReader dr = cmd.ExecuteReader();
                                                if (dr.HasRows)
                                                {
                                                    while (dr.Read())
                                                    {
                                                        for (int i = 0; i < dr.FieldCount; i++)
                                                        {
                                                            if (_viewModel.ServerTables == null)
                                                                _viewModel.ServerTables = new List<string>();
                                                            _viewModel.ServerTables.Add(dr.GetValue(i).ToString());
                                                        }
                                                    }
                                                }
                                            }
                                        }

                                    }
                                    break;
                            }
                        }
                    }
                    #endregion

                    _myWatcher = new ServerStatusWatcher(_viewModel.MyServer.Ip, _viewModel.ServerProperties.server_port);
                    _myWatcher.OnServerStatus += _myWatcher_OnServerStatus;
                    _myWatcher.Start();
                }
                else
                {
                    
                }
            }
        }
        
        private async void ShowUpdateDialog()
        {
            var dlg = await this.ShowMessageAsync("Update verfügbar", " es liegt eine neue Version von Rising Wold Servermanager vor. Möchtest Du die neue version runterladen?", MessageDialogStyle.AffirmativeAndNegative,
                new MetroDialogSettings()
                {
                    AffirmativeButtonText = "aktualisieren",
                    NegativeButtonText = "abbrechen",
                    ColorScheme = MetroDialogColorScheme.Accented
                });

            if(dlg == MessageDialogResult.Affirmative)
            {
                string updaterpath = System.IO.Path.Combine(_config.AppPath, "Updater");
                Process p = new Process();
                p.StartInfo.Arguments = "appPath=" + _config.AppPath;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.FileName = System.IO.Path.Combine(updaterpath, "rwupdater.exe");
                p.Start();
                Environment.Exit(0);
            }
        }

        private Thread _runThread;
        private void RunServer()
        {
            _serverProcess = new System.Diagnostics.Process();
            _serverProcess.StartInfo.FileName = "java.exe";
            if (_viewModel.ServerProperties.server_memory <= 0)
                _viewModel.ServerProperties.server_memory = 1024;
            _serverProcess.StartInfo.Arguments = string.Format("-Xmx{0}m -jar {1}", _viewModel.ServerProperties.server_memory,System.IO.Path.Combine(_viewModel.MyServer.Path,"server.jar"));
            _serverProcess.StartInfo.CreateNoWindow = true;
            _serverProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            _serverProcess.StartInfo.UseShellExecute = false;
            _serverProcess.StartInfo.WorkingDirectory = _viewModel.MyServer.Path;

            _serverProcess.StartInfo.RedirectStandardError = true;
            _serverProcess.StartInfo.RedirectStandardOutput = true;
            _serverProcess.StartInfo.RedirectStandardInput = true;
            var stdOutput = new StringBuilder();
            _serverProcess.OutputDataReceived += _serverProcess_OutputDataReceived; // Use AppendLine rather than Append since args.Data is one line of output, not including the newline character.
             
            string stdError = null;
            try
            {
                _serverProcess.Start();
                _processInput = _serverProcess.StandardInput;
                _serverProcess.BeginOutputReadLine();
                
                stdError = _serverProcess.StandardError.ReadToEnd();
                _serverProcess.WaitForExit();
                
            }
            catch (Exception e)
            {
                
            }

            if (_serverProcess.ExitCode == 0)
            {
                //return stdOutput.ToString();
            }
            else
            {
                var message = new StringBuilder();

                if (!string.IsNullOrEmpty(stdError))
                {
                    message.AppendLine(stdError);
                }

                if (stdOutput.Length != 0)
                {
                    message.AppendLine("Std output:");
                    message.AppendLine(stdOutput.ToString());
                }

                
            }
        }

        private void _serverProcess_OutputDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        {
            _viewModel.ServerLog += "\r\n" + e.Data;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                //txtServerLog.Text += "\r\n"+ e.Data;
            }));
        }

        private void _myWatcher_OnServerStatus(ServerInfo info)
        {
            if(info != null && !info.IsOnline)
            {
                info.MaxPlayerCount = _viewModel.ServerProperties.settings_max_players;
                info.PvP = _viewModel.ServerProperties.settings_pvp_enabled;
                info.HiveProt = _viewModel.ServerProperties.server_hive_verification;
                info.Locked = String.IsNullOrEmpty(_viewModel.ServerProperties.server_password) ? false : true;
                info.Lua = false;
                info.ServerIp = _viewModel.MyServer.Ip;
                info.ServerName = _viewModel.ServerProperties.server_name;
                info.ServerPort = _viewModel.ServerProperties.server_port;
                info.Verification = true;
                info.Version = "unbekannt";
                info.WhiteList = false;
                
            }
            _viewModel.AktualServer = info;
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

        private void btnSetServerPath_Click(object sender, RoutedEventArgs e)
        {
            VistaFolderBrowserDialog vfbd = new VistaFolderBrowserDialog();
            vfbd.Description = "Rising World Serververzeichnis";
            vfbd.UseDescriptionForTitle = true;
            if ((bool)vfbd.ShowDialog(this))
            {
                txtServerPath.Text = vfbd.SelectedPath;
            }
        }

        private void btnStartServer_Click(object sender, RoutedEventArgs e)
        {
            txtServerLog.Text = "";
            _viewModel.ServerLog = "";
            //btnStartServer.IsEnabled = false;
            _runThread = new Thread(new ThreadStart(RunServer));
            _runThread.Start();
        }

        private void btnStopServer_Click(object sender, RoutedEventArgs e)
        {
            //btnStopServer.IsEnabled = false;
            if(_serverProcess != null && _serverProcess.Id > 0 && _processInput != null)
            {
                _processInput.WriteLine("quit");
            }
        }

        private void txtServerLog_TextChanged(object sender, TextChangedEventArgs e)
        {
            txtServerLog.SelectionStart = txtServerLog.Text.Length;
            txtServerLog.ScrollToEnd();
        }

        private void btnUpdateServer_Click(object sender, RoutedEventArgs e)
        {
            //btnStartServer.IsEnabled = false;
            //btnStopServer.IsEnabled = false;
            //btnUpdateServer.IsEnabled = false;
            if(_serverProcess != null && _serverProcess.Id > 0 && _processInput != null)
            {
                _processInput.WriteLine("quit");
                new Thread(new ThreadStart(DoUpdate)).Start();
            }
        }

        private void DoUpdate()
        {
            _viewModel.AktualServer.IsUpdating = true;
            _viewModel.ServerLog += "\r\n" + "warte auf Serverstop...";
            while (_viewModel.AktualServer.IsOnline) Thread.Sleep(100);

            _viewModel.ServerLog += "\r\n" + "hole Serverupdate....";
            var dlPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RWServerAdmin");

            if (!System.IO.Directory.Exists(dlPath))
                System.IO.Directory.CreateDirectory(dlPath);

            var updateFile = System.IO.Path.Combine(dlPath, "serverupdate.zip");
            if (System.IO.File.Exists(updateFile))
                System.IO.File.Delete(updateFile);

            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += WebClient_DownloadFileCompleted;
            webClient.DownloadProgressChanged += WebClient_DownloadProgressChanged;
            webClient.DownloadFileAsync(new Uri("https://download.rising-world.net/download.php?type=server&filetype=zip"), updateFile);
        }

        private void WebClient_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            _viewModel.ServerLog += "\r\n" + "lade " + e.BytesReceived + "/" + e.TotalBytesToReceive + " " + e.ProgressPercentage + "%";
        }

        private void WebClient_DownloadFileCompleted(object sender, System.ComponentModel.AsyncCompletedEventArgs e)
        {
            var dlPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "RWServerAdmin");
            var updateFile = System.IO.Path.Combine(dlPath, "serverupdate.zip");
            _viewModel.ServerLog += "\r\n Serverupdate wurde erfolgreich geladen. Starte Serverbackup...";
            var backupPath = System.IO.Path.Combine(dlPath, "Backups");
            if (!System.IO.Directory.Exists(backupPath))
                System.IO.Directory.CreateDirectory(backupPath);
            string backupFile = System.IO.Path.Combine(backupPath, string.Format("backup_{0:ddMMyyyy_HHmmss}.zip", DateTime.Now));
            System.IO.FileStream fsOut = System.IO.File.Create(backupFile);
            ZipOutputStream zipStream = new ZipOutputStream(fsOut);
            zipStream.SetLevel(3);
            int folderOffset = _viewModel.MyServer.Path.Length + (_viewModel.MyServer.Path.EndsWith("\\") ? 0 : 1);
            CompressBackup(_viewModel.MyServer.Path, zipStream, folderOffset,backupFile);
            zipStream.IsStreamOwner = true;
            zipStream.Close();
            _viewModel.ServerLog += "\r\n wende Update auf '" + _viewModel.MyServer.Path + "' an";

            _viewModel.AktualServer.IsUpdating = false;
            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    btnUpdateServer.IsEnabled = true;
            //    btnStartServer.IsEnabled = true;
            //    btnStopServer.IsEnabled = false;
            //}));
        }

        private void CompressBackup(string path, ZipOutputStream zipStream, int folderOffset, string zipFile)
        {
            string[] files = System.IO.Directory.GetFiles(path);
            System.IO.FileInfo zfi = new System.IO.FileInfo(zipFile);

            foreach (string filename in files)
            {

                System.IO.FileInfo fi = new System.IO.FileInfo(filename);

                string entryName = filename.Substring(folderOffset); // Makes the name in zip based on the folder
                entryName = ZipEntry.CleanName(entryName); // Removes drive from name and fixes slash direction
                ZipEntry newEntry = new ZipEntry(entryName);
                newEntry.DateTime = fi.LastWriteTime; // Note the zip format stores 2 second granularity
                _viewModel.ServerLog += "\r\n packe " + newEntry.Name + " -> " + zfi.Name;
                // Specifying the AESKeySize triggers AES encryption. Allowable values are 0 (off), 128 or 256.
                // A password on the ZipOutputStream is required if using AES.
                //   newEntry.AESKeySize = 256;

                // To permit the zip to be unpacked by built-in extractor in WinXP and Server2003, WinZip 8, Java, and other older code,
                // you need to do one of the following: Specify UseZip64.Off, or set the Size.
                // If the file may be bigger than 4GB, or you do not need WinXP built-in compatibility, you do not need either,
                // but the zip will be in Zip64 format which not all utilities can understand.
                //   zipStream.UseZip64 = UseZip64.Off;
                newEntry.Size = fi.Length;

                zipStream.PutNextEntry(newEntry);

                // Zip the file in buffered chunks
                // the "using" will close the stream even if an exception occurs
                byte[] buffer = new byte[4096];
                using (System.IO.FileStream streamReader = System.IO.File.OpenRead(filename))
                {
                    StreamUtils.Copy(streamReader, zipStream, buffer);
                }
                zipStream.CloseEntry();
            }
            string[] folders = System.IO.Directory.GetDirectories(path);
            foreach (string folder in folders)
            {
                CompressBackup(folder, zipStream, folderOffset, zipFile);
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems != null && e.AddedItems.Count == 1)
            {
                string tablename = e.AddedItems[0].ToString();
                DataTable dt = new DataTable(tablename);
                try
                {
                    switch (_viewModel.ServerProperties.database_type.ToLower())
                    {
                        default:
                        case "sqlite":
                            {
                                using (var c = new SQLiteConnection(string.Format("Data Source={0}\\Worlds\\{1}\\{1}.db", _viewModel.MyServer.Path, _viewModel.ServerProperties.server_world_name)))
                                {
                                    c.Open();
                                    using (SQLiteCommand cmd = c.CreateCommand())
                                    {
                                        cmd.CommandText = "SELECT * FROM " + tablename;
                                        using (SQLiteDataReader dr = cmd.ExecuteReader())
                                        {
                                            dt.Load(dr);
                                            dgServerTable.ItemsSource = dt.DefaultView;
                                        }
                                    }
                                }
                            }break;
                        case "mysql":
                            {
                                MySqlConnectionStringBuilder ssb = new MySqlConnectionStringBuilder();
                                ssb.Database = _viewModel.ServerProperties.database_mysql_database;
                                ssb.Password = _viewModel.ServerProperties.database_mysql_password;
                                ssb.Port = (uint)_viewModel.ServerProperties.database_mysql_server_port;
                                ssb.Server = _viewModel.ServerProperties.database_mysql_server_ip;
                                ssb.UserID = _viewModel.ServerProperties.database_mysql_user;

                                using(MySqlConnection conn = new MySqlConnection(ssb.ConnectionString))
                                {
                                    using(MySqlCommand cmd = conn.CreateCommand())
                                    {
                                        cmd.CommandText = "SELECT * FROM " + tablename;
                                        conn.Open();
                                        using(MySqlDataReader dr = cmd.ExecuteReader())
                                        {
                                            dt.Load(dr);
                                            dgServerTable.ItemsSource = dt.DefaultView;
                                        }
                                    }
                                }
                            }
                            break;
                    }

                }
                catch (Exception)
                {

                    
                }
            }
        }

        private void GetSettingsDescription(object sender, RoutedEventArgs e)
        {
            var html = (string)Properties.Resources.SettingsHtml;
            if (!String.IsNullOrEmpty(html))
                _viewModel.SettingsHtml = html.Replace("{0}", @"<h2>database_mysql_database</h2>Name der MySQL - Datenbank<em>(nur bei der Verwendung von MySQL) </em >");
        }

        private void cmbSelDbTyp_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (grpSetMysql == null) return;
            if (cmbSelDbTyp.SelectedIndex == 0 || cmbSelDbTyp.SelectedIndex == -1)
                grpSetMysql.IsEnabled = false;
            else if (cmbSelDbTyp.SelectedIndex == 1)
                grpSetMysql.IsEnabled = true;
        }

        private void MetroWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if(_myWatcher != null)
            {
                _myWatcher.Stop();
                while (_myWatcher.IsRunning) Thread.Sleep(100);
            }
        }
    }

    public static class BrowserBehavior
    {
        public static readonly DependencyProperty HtmlProperty = DependencyProperty.RegisterAttached(
            "Html",
            typeof(string),
            typeof(BrowserBehavior),
            new FrameworkPropertyMetadata(OnHtmlChanged));

        [AttachedPropertyBrowsableForType(typeof(WebBrowser))]
        public static string GetHtml(WebBrowser d)
        {
            return (string)d.GetValue(HtmlProperty);
        }

        public static void SetHtml(WebBrowser d, string value)
        {
            d.SetValue(HtmlProperty, value);
        }

        static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WebBrowser wb = d as WebBrowser;
            if (wb != null)
                wb.NavigateToString(e.NewValue as string);
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

    public class ServerStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (parameter.ToString().ToLower())
            {
                default: return null;
                case "status":
                    {
                        BitmapImage img = new BitmapImage();
                        img.BeginInit();
                        img.UriSource = (value != null && ((ServerInfo)value).IsOnline) ? new Uri("Assets/Icons/online.png", UriKind.Relative) : new Uri("Assets/Icons/offline.png", UriKind.Relative);
                        img.EndInit();
                        return img;
                    }
                case "hive":
                    {
                        BitmapImage img = new BitmapImage();
                        img.BeginInit();
                        img.UriSource = (value != null && ((ServerInfo)value).HiveProt) ? new Uri("Assets/Icons/hive_on.png", UriKind.Relative) : new Uri("Assets/Icons/hive_off.png", UriKind.Relative);
                        img.EndInit();
                        return img;
                    }
                case "locked":
                    {
                        BitmapImage img = new BitmapImage();
                        img.BeginInit();
                        img.UriSource = (value != null && ((ServerInfo)value).Locked) ? new Uri("Assets/Icons/locked.png", UriKind.Relative) : new Uri("Assets/Icons/locked_off.png", UriKind.Relative);
                        img.EndInit();
                        return img;
                    }
                case "lua":
                    {
                        BitmapImage img = new BitmapImage();
                        img.BeginInit();
                        img.UriSource = (value != null && ((ServerInfo)value).Lua) ? new Uri("Assets/Icons/lua_on.png", UriKind.Relative) : new Uri("Assets/Icons/lua_off.png", UriKind.Relative);
                        img.EndInit();
                        return img;
                    }
                case "sname":
                    {
                        return (value != null) ? ((ServerInfo)value).ServerName : "-";
                    }
                case "ip":
                    {
                        return (value != null) ? ((ServerInfo)value).ServerIp : "?";
                    }
                case "port":
                    {
                        return (value != null) ? ((ServerInfo)value).ServerPort : 4255;
                    }
                case "pcount":
                    {
                        return (value != null) ? ((ServerInfo)value).PlayerCount : 0;
                    }
                case "pmax":
                    {
                        return (value != null) ? ((ServerInfo)value).MaxPlayerCount : 0;
                    }
                case "wlist":
                    {
                        BitmapImage img = new BitmapImage();
                        img.BeginInit();
                        img.UriSource = (value != null && ((ServerInfo)value).WhiteList) ? new Uri("Assets/Icons/whitelist_on.png", UriKind.Relative) : new Uri("Assets/Icons/whitelist_off.png", UriKind.Relative);
                        img.EndInit();
                        return img;
                    }
                case "veri":
                    {
                        BitmapImage img = new BitmapImage();
                        img.BeginInit();
                        img.UriSource = (value != null && ((ServerInfo)value).IsOnline) ? new Uri("Assets/Icons/veri_on.png", UriKind.Relative) : new Uri("Assets/Icons/veri_off.png", UriKind.Relative);
                        img.EndInit();
                        return img;
                    }
                case "pvp":
                    {
                        BitmapImage img = new BitmapImage();
                        img.BeginInit();
                        img.UriSource = (value != null && ((ServerInfo)value).IsOnline) ? new Uri("Assets/Icons/pvp.png", UriKind.Relative) : new Uri("Assets/Icons/pve.png", UriKind.Relative);
                        img.EndInit();
                        return img;
                    }
                case "version":
                    {
                        return (value != null) ? ((ServerInfo)value).Version : "?";
                    }
                case "start":
                    {
                        
                        if (value == null) return false;
                        if(((ServerInfo)value).IsUpdating)
                        {
                            return false;
                        }
                        return ((ServerInfo)value).IsOnline ? false : true;
                    }
                case "stop":
                    {
                        if (value == null) return false;
                        if (((ServerInfo)value).IsUpdating)
                        {
                            return false;
                        }
                        return ((ServerInfo)value).IsOnline ? true : false;
                    }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class DBTypeSelectorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return 0;
            switch (value.ToString().ToLower())
            {
                default:
                case "sqlite": return 0;
                case "mysql": return 1;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return "sqlite";
            switch ((int)value)
            {
                default:
                case 0: return "sqlite";
                case 1: return "mysql";
            }
        }
    }
}
