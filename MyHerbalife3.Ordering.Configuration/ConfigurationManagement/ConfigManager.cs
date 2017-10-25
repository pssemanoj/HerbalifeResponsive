using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Web.Hosting;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.ConfigurationObjects;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.FileWatcher;
using MyHerbalife3.Ordering.Configuration.ConfigurationManagement.Providers;

namespace MyHerbalife3.Ordering.Configuration.ConfigurationManagement
{
    public sealed class ConfigManager
    {
        #region Constants

        private const string ProgressStatusOk = "Configs loaded for locale {0}.\r\n";
        private const string ProgressStatusFailed = "Failed to load config file {0}.\r\n";
        private const string ProgressStatusHadErrors = "Configs loaded for locale {0}, but the file has errors.\r\n{1}";

        #endregion

        #region Fields

        /// <summary>Locale dictionary of Configuration dictionaries</summary>
        private readonly Dictionary<string, Dictionary<string, ConfigurationSet>> _Configs =
            new Dictionary<string, Dictionary<string, ConfigurationSet>>();

        private StringBuilder _ConfigStatusMessage = new StringBuilder();
        private string _currentPlatform = "MyHL";
        private bool _errorLoading;

        #endregion

        #region Singleton Implementation

        private static readonly ConfigManager _ConfigManager = new ConfigManager();

        static ConfigManager()
        {
        }

        public static ConfigManager Instance
        {
            get { return _ConfigManager; }
        }

        #endregion

        #region Construction

        /// <summary>
        ///     Initializes a new instance of the <see cref="ConfigManager" /> class.
        /// </summary>
        private ConfigManager()
        {
            var platforms = GetPlatforms();
            var defaultPlatform = platforms.Platforms.Find(p => p.Active & p.Default);
            if (null == defaultPlatform)
            {
                string errorMessage =
                    "Could not initialize HLConfigManager. There were no Active and Default Platforms in MyHLPlatforms.xml";
                Trace.TraceError(errorMessage, "ConfigManager");
                throw new ApplicationException(errorMessage);
            }
            CurrentPlatform = defaultPlatform.Name;
            LoadConfigs(platforms.Platforms);
        }

        #endregion

        #region Properties

        public ConfigurationSet Configurations
        {
            get
            {
                ConfigurationSet configs = null;
                string locale = string.Empty;
                string platform = string.Empty;
                try
                {
                    locale = Thread.CurrentThread.CurrentCulture.Name;
                    platform = GetCurrentPlatformName();
                    if (_Configs[platform].ContainsKey(locale))
                    {
                        configs = _Configs[platform][locale];
                    }
                    else
                    {
                        try
                        {
                            configs = _Configs[platform]["Default"];
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError(
                                string.Format(
                                    "Unable to drop back to Default configs for Platform {0}. Web_Default.config is either corrupt or missing and was not loaded. {1}",
                                    platform, ex.Message), "ConfigManager");
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(
                        string.Format(
                            "Error acquiring ConfigurationSet for Platform {0}, locale: {1}. Error Message: {2}",
                            platform, locale, ex.Message), "ConfigManager");
                }

                return configs;
            }
        }

        public Dictionary<string, ConfigurationSet> CurrentPlatformConfigs
        {
            get { return _Configs[CurrentPlatform]; }
        }

        public Dictionary<string, Dictionary<string, ConfigurationSet>> AllPlatformConfigs
        {
            get { return _Configs; }
        }

        public string CurrentPlatform
        {
            get { return GetCurrentPlatformName(); }
            private set { _currentPlatform = value; }
        }

        /// <summary>
        ///     List of locations that are configured for the current platform.
        /// </summary>
        public Dictionary<string, string> PlatformLocations { get; set; }

        #endregion

        #region Private methods

        private HLPlatforms GetPlatforms()
        {
            var platforms = new HLPlatforms();
            try
            {
                platforms = HLPlatforms.GetPlatforms();
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.Message, "ConfigManager");
            }
            finally
            {
                if (platforms.Platforms.Count == 0 || platforms.Platforms.Count(p => p.Active) == 0)
                {
                    Trace.TraceError(
                        "No Platforms marked Active were found in MyHLPlatforms.xml - or the file could not ge found. Creating default MyHL Platform",
                        "ConfigManager");
                    platforms.Platforms.Clear();
                    var myHL = new Platform();
                    myHL.Name = "MyHL";
                    myHL.Active = true;
                    myHL.Default = true;
                    platforms.Platforms.Add(myHL);
                }
            }

            return platforms;
        }

        private string GetCurrentPlatformName()
        {
            string platform = _currentPlatform;
            if (!string.IsNullOrEmpty(Thread.CurrentThread.Name))
            {
                if (_Configs.ContainsKey(Thread.CurrentThread.Name))
                {
                    platform = Thread.CurrentThread.Name;
                }
            }

            return platform;
        }

        private string GetConfigFilesList(string[] configFiles)
        {
            var sb = new StringBuilder();
            foreach (string file in configFiles)
            {
                sb.Append(Path.GetFileName(file));
                sb.Append("\r\n");
            }

            return sb.ToString();
        }

        private void LoadConfigs(IEnumerable<Platform> platforms)
        {
            foreach (var platform in platforms.Where(p => p.Active).ToList())
            {
                var configPath = HostingEnvironment.ApplicationPhysicalPath != null
                    // Normal flow (IIS)
                                     ? Path.Combine(HostingEnvironment.ApplicationPhysicalPath,
                                                    "Configuration\\" + platform.Name)
                    // Determinig the not normal execution channel.
                                     : AppDomain.CurrentDomain
                                                .BaseDirectory.IndexOf
                                           ("MyHerbalife3.Ordering.Web.Test",
                                            System.StringComparison
                                                  .Ordinal) > 0
                    // For local unit tests.
                                           ? Path.Combine(
                                               AppDomain.CurrentDomain.BaseDirectory.Substring(0,
                                                                                               AppDomain.CurrentDomain
                                                                                                        .BaseDirectory
                                                                                                        .IndexOf
                                                                                                   ("MyHerbalife3.Ordering.Web.Test",
                                                                                                    System
                                                                                                        .StringComparison
                                                                                                        .Ordinal)),
                                               "MyHerbalife3.Ordering.Web\\Configuration\\" + platform.Name)
                    // For automated test when building a check-in.
                                           : string.Concat(
                                               AppDomain.CurrentDomain.BaseDirectory,
                                               "\\_PublishedWebsites\\MyHerbalife3.Ordering.Web\\Configuration\\", platform.Name);

                FileSystemWatcherManager.FileChanged += new EventHandler<FileNotifyEventArgs>(ConfigFileChanged);
                if (!Directory.Exists(configPath))
                    continue;
                var configFiles = GetConfigFiles(configPath);

                Trace.TraceInformation(
                    string.Format(
                        "Begin loading configs for MyHerbalife website using Platform: {0}. Config Directory: {1}.\r\nThe following Config files will be loaded:\r\n{2}",
                        platform.Name, configPath, GetConfigFilesList(configFiles)), "ConfigManager");
                try
                {
                    foreach (string configFile in configFiles)
                    {
                        if (ProcessConfigFile(platform.Name, configFile, false))
                        {
                            FileSystemWatcherManager.AddWatchedObject(configFile);
                        }
                        else
                        {
                            _errorLoading = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(
                        string.Format(
                            "An exception occurred while initializing MyHerbalife configurations for Platform: {0}!\r\n{1}\r\n{2}",
                            platform.Name, ex.Message, ex.StackTrace), "ConfigManager");
                    _errorLoading = true;
                }
                finally
                {
                    if (_errorLoading)
                    {
                        Trace.TraceError(
                            string.Format(
                                "Config loading for Platform: {0} completed with errors! Review previous error messages for the details.\r\n{1}",
                                platform.Name, _ConfigStatusMessage), "ConfigManager");
                    }
                    else
                    {
                        Trace.TraceInformation(
                            string.Format(
                                "All Configs loaded successfully and without errors for Platform: {0}!\r\n{1}",
                                platform.Name, _ConfigStatusMessage), "ConfigManager");
                    }
                    _ConfigStatusMessage = new StringBuilder();
                }
            }
        }

        private bool ProcessConfigFile(string platform, string configFile, bool replaceOriginal)
        {
            var processedOk = true;
            System.Configuration.Configuration config = null;
            var configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = configFile;
            try
            {
                config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    string.Format("Could not load configurations from the file: {0}:\r\n{1}", configFile, ex.Message),
                    "ConfigManager");
                _ConfigStatusMessage.Append(string.Format(ProgressStatusFailed,
                                                          Path.GetFileNameWithoutExtension(configFile)));
                return false;
            }

            var locale = config.AppSettings.Settings["Locale"];
            if (null == locale || string.IsNullOrEmpty(locale.Value))
            {
                Trace.TraceError(
                    string.Format("Missing Locale in configuration file: {0}. File skipped.",
                                  Path.GetFileName(configFile)), "ConfigManager");
                processedOk = false;
            }
            else
            {
                // Configuration given by Web_countryLocale.config local file.
                processedOk = ProcessLocalConfigFile(config, platform, configFile, locale.Value, replaceOriginal);

                // Review if it should read service configuraiton.
                //var useServiceConfiguration = config.AppSettings.Settings["UseServiceConfiguration"];
                //if (platform == "MyHL" && null != useServiceConfiguration &&
                //    !string.IsNullOrEmpty(useServiceConfiguration.Value) && bool.Parse(useServiceConfiguration.Value))
                //{
                //    HlCountryConfigurationProvider.ProcessServiceConfigurationSet(locale.Value,
                //                                                                  _Configs[platform][locale.Value]);
                //}
            }

            return processedOk;
        }

        /// <summary>
        /// Process a local configuratino file.
        /// </summary>
        /// <param name="config">Configuration</param>
        /// <param name="platform">Platform</param>
        /// <param name="configFile">Configuration file path.</param>
        /// <param name="locale"></param>
        /// <param name="replaceOriginal">Replace original or not.</param>
        /// <returns>True if imported</returns>
        private bool ProcessLocalConfigFile(System.Configuration.Configuration config, string platform,
            string configFile, string locale, bool replaceOriginal)
        {
            var processedOk = true;
            try
                {
                    var newSet = new ConfigurationSet(config);
                    if (!_Configs.ContainsKey(platform))
                    {
                        _Configs.Add(platform, new Dictionary<string, ConfigurationSet>());
                    }
                    if (!_Configs[platform].ContainsKey(newSet.Locale))
                    {
                        _Configs[platform].Add(newSet.Locale, newSet);
                        bool hasError = newSet.HasErrors;
                        _errorLoading |= hasError;
                        string message = (hasError)
                                             ? string.Format(ProgressStatusHadErrors, newSet.Locale, newSet.ErrorMessage)
                                             : string.Format(ProgressStatusOk, newSet.Locale);
                        _ConfigStatusMessage.Append(string.Format(message, locale));
                    }
                    else
                    {
                        if (replaceOriginal)
                        {
                            var locker = new ReaderWriterLockSlim();
                            locker.EnterReadLock();
                            try
                            {
                                _Configs[platform].Remove(newSet.Locale);
                                _Configs[platform].Add(newSet.Locale, newSet);
                            }
                            catch (Exception lockException)
                            {
                                Trace.TraceError(
                                    string.Format(
                                        "An exception occurred while refreshing the configurations {0}... {1}",
                                        Path.GetFileNameWithoutExtension(configFile), lockException.Message),
                                    "ConfigManager");
                                processedOk = false;
                            }
                            finally
                            {
                                locker.ExitReadLock();
                            }
                        }
                        else
                        {
                            Trace.TraceError(
                                string.Format(
                                    "A duplicate locale was encountered in the config file: {0}. Configs for Locale {1} has already been loaded.",
                                    configFile, newSet.Locale), "ConfigManager");
                            processedOk = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.TraceError(
                        string.Format(
                            "An exception occurred while initializing MyHerbalife configurations!\r\n{0}\r\nContinuing with load...",
                            ex.Message), "ConfigManager");
                    processedOk = false;
                }

            return processedOk;
        }

        private string[] GetConfigFiles(string configPath)
        {
            var results = new string[] {};
            try
            {
                if (Directory.Exists(configPath))
                {
                    results = Directory.GetFiles(configPath, "*.config");
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    string.Format("Could not load config files from {0}. The error is\r\n{1}.", configPath, ex.Message),
                    "ConfigManager");
            }

            return results;
        }

        private void ConfigFileChanged(object sender, FileNotifyEventArgs e)
        {
            string configFile = e.FilePath;
            Trace.TraceInformation(
                string.Format("Config file change detected. Reloading config file: {0}.", Path.GetFileName(configFile)),
                "ConfigManager");
            Thread.Sleep(1000); //TODO - get this onto a seperate thread.
            var directories = Path.GetDirectoryName(e.FilePath)
                                  .Split(new[] {Path.DirectorySeparatorChar}, StringSplitOptions.RemoveEmptyEntries);
            string platform = directories[directories.Length - 1];
            if (ProcessConfigFile(platform, configFile, true))
            {
                Trace.TraceInformation(
                    string.Format("Configuration {0} refreshed from disk.", Path.GetFileNameWithoutExtension(configFile)),
                    "ConfigManager");
            }
        }

        #endregion
    }
}