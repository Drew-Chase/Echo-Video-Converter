using ChaseLabs.CLLogger;
using ChaseLabs.CLUpdate.Lists;
using ChaseLabs.Echo.Video_Converter.Utilities;
using System;
using System.IO;
using System.Reflection;
using System.Windows.Controls;

namespace ChaseLabs.Echo.Video_Converter.Resources
{
    public class Values
    {
        private static Values _singleton;
        public static Values Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new Values();
                }

                return _singleton;
            }
        }

        //public LogManger Log = LogManger.Init().SetLogDirectory(Values.Singleton.LogFileLocation).EnableDefaultConsoleLogging().SetMinLogType(Lists.LogTypes.All);
        public static string[] MediaExtensions => new string[] { "done", "mpegg", "mpeg", "mp4", "mkv", "m4a", "m4v", "f4v", "f4a", "m4b", "m4r", "f4b", "mov", "3gp", "3gp2", "3g2", "3gpp", "3gpp2", "ogg", "oga", "ogv", "ogx", "wmv", "wma", "flv", "avi" };


        private static readonly CLLogger.Interfaces.ILog log = LogManger.Init().SetLogDirectory(Values.Singleton.LogFileLocation).EnableDefaultConsoleLogging().SetMinLogType(Lists.LogTypes.All);

        protected Values()
        {
        }

        public string CompanyName => "Chase Labs";
        public string ApplicationName => "Echo Video Converter";
        public string ProductLine => "Echo";
        public string VersionFileName => "Version";
        public string VersionPath => Path.Combine(ConfigLocation, VersionFileName);
        public string ApplicationVersionKey => "Application";
        public string LauncherVersionKey => "LAUNCHER";
        public string RemoteVersionURL => "https://www.dropbox.com/s/nsxijbp9hkrbr1p/version?dl=1";
        public string LastUsedMediaDirectory { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Last Used Media Directory").Value = value; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Last Used Media Directory").Value; }
        public bool UseEnclosedFolder { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Use Enclosed Folder").Value = value + ""; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Use Enclosed Folder").ParseBoolean(); }
        public string TempFolderLocation { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Temp Folder Location").Value = value; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Temp Folder Location").Value; }
        public bool ShowEncoderConsole { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Show Encoder Console").Value = value + ""; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Show Encoder Console").ParseBoolean(); }
        public bool IsNetworkPath { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Is Network Path").Value = value + ""; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Is Network Path").ParseBoolean(); }
        public bool UseNvidiaNVENC { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Use Nvidia NVENC").Value = value + ""; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Use Nvidia NVENC").ParseBoolean(); }
        public bool UseHardwareEncoding { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Use Hardware Encoding").Value = value + ""; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Use Hardware Encoding").ParseBoolean(); }
        public bool OverwriteOriginal { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Overwrite Original").Value = value + ""; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Overwrite Original").ParseBoolean(); }
        public string FFMPEGFile { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("FFMPEG File Path").Value = value; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("FFMPEG File Path").Value; }
        public string LauncherDirectory { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Launcher Directory").Value = value; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Launcher Directory").Value; }

        public string DefaultFFMPEGFile => Path.Combine(Values.Singleton.ApplicationDirectory, "FFMPEG", "bin", "x64", "ffmpeg.exe");
        public string FFMPEGDirectory => Directory.GetParent(FFMPEGFile).FullName;
        public string LogFileLocation => Path.Combine(LogLocation, "latest.log");
        public string ConfigFileLocation => Path.Combine(ConfigLocation, "default.config");

        public string CurrentSizeString { get; set; }
        public string OriginalSizeString { get; set; }

        public string FormattedTime => DateTime.Now.ToString().Replace(":", "-").Replace("/", "-");
        public string UnformattedTime => DateTime.Now.ToString();

        public MediaFiles mediaFiles => new MediaFiles();

        private ScrollViewer sv = null;
        private readonly TextBlock currentSize, originalSize;

        public MediaFiles MediaFiles { get; } = new MediaFiles();

        public TextBlock CurrentSize
        {
            get;
            set;
        }

        public TextBlock OriginalSize
        {
            get;
            set;
        }

        //private string _buildVersion = "N/A";
        //public string BuildVersion
        //{
        //    get => _buildVersion;
        //    set => _buildVersion = "build v." + value;
        //}

        public string ApplicationBuildVersion
        {
            get
            {
                Versions version = new Versions(VersionPath);
                if (version.GetVersion(ApplicationVersionKey) != null)
                {
                    return $"app-v.{version.GetVersion(ApplicationVersionKey).Value}";
                }

                return "";
            }
        }

        public string LauncherBuildVersion
        {
            get
            {
                Versions version = new Versions(VersionPath);
                if (version.GetVersion(LauncherVersionKey) != null)
                {
                    return $"launcher-v.{version.GetVersion(LauncherVersionKey).Value}";
                }

                return "";
            }
        }

        public string InstallationFolder => Directory.GetParent(Assembly.GetExecutingAssembly().Location).FullName;

        private string rootlocation = "";
        public string RootLocation
        {
            get
            {
                if (rootlocation == "")
                    RootLocation = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

                return rootlocation;
            }
            set
            {
                string dir = Path.Combine(value, CompanyName, ProductLine, ApplicationName);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                rootlocation = dir;
            }
        }


        public string ConfigLocation
        {
            get
            {
                string dir = Path.Combine(RootLocation, "Configurations");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return dir;
            }
        }

        public string LogLocation
        {
            get
            {
                string dir = Path.Combine(RootLocation, "Logs");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                return dir;
            }
        }

        public string ApplicationDirectory
        {
            get
            {
                string dir = Path.Combine(RootLocation, "Bin");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                return dir;
            }
        }

    }

}
