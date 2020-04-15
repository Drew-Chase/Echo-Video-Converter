using ChaseLabs.CLLogger;
using ChaseLabs.CLUpdate.Lists;
using ChaseLabs.Echo.Video_Converter.Util;
using System;
using System.IO;
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
        public string LauncherVersionKey => "Launcher";
        public string RemoteVersionURL => "https://www.dropbox.com/s/nsxijbp9hkrbr1p/version?dl=1";
        public string LauncherURL => "https://www.dropbox.com/s/kqvc6nw10nht0m1/launcher.zip?dl=1";
        public string ApplicationURL => "https://www.dropbox.com/s/5p5qbhl97emxn4k/application.zip?dl=1";

        public string LastUsedMediaDirectory { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Last Used Media Directory").Value = value; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Last Used Media Directory").Value; }
        public bool UseEnclosedFolder { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Use Enclosed Folder").Value = value + ""; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Use Enclosed Folder").ParseBoolean(); }
        public string TempFolderLocation { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Temp Folder Location").Value = value; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Temp Folder Location").Value; }
        public bool ShowEncoderConsole { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Show Encoder Console").Value = value + ""; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Show Encoder Console").ParseBoolean(); }
        public bool IsNetworkPath { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Is Network Path").Value = value + ""; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Is Network Path").ParseBoolean(); }
        public bool UseNvidiaNVENC { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Use Nvidia NVENC").Value = value + ""; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Use Nvidia NVENC").ParseBoolean(); }
        public bool OverwriteOriginal { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("Overwrite Original").Value = value + ""; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("Overwrite Original").ParseBoolean(); }
        public string FFMPEGFile { set => ConfigUtilities.Singleton.Manager.GetConfigByKey("FFMPEG File Path").Value = value; get => ConfigUtilities.Singleton.Manager.GetConfigByKey("FFMPEG File Path").Value; }

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
                var version = new Versions(VersionPath);
                if (version.GetVersion(ApplicationVersionKey) != null)
                    return $"app-v.{version.GetVersion(ApplicationVersionKey).Value}";
                return "";
            }
        }

        public string LauncherBuildVersion
        {
            get
            {
                var version = new Versions(VersionPath);
                if (version.GetVersion(LauncherVersionKey) != null)
                    return $"launcher-v.{version.GetVersion(LauncherVersionKey).Value}";
                return "";
            }
        }

        public string InstallationFolder => Environment.CurrentDirectory;

        private string configlocation = "";
        public string ConfigLocation
        {
            get => configlocation;
            set
            {
                string dir = Path.Combine(value, "Configurations");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                configlocation = dir;
            }
        }

        private string loglocation = "";
        public string LogLocation
        {
            get => loglocation;
            set
            {
                string dir = Path.Combine(value, "Logs");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                loglocation = dir;
            }
        }

        private string rootlocation = "";
        public string RootLocation
        {
            get => rootlocation;
            set
            {
                string dir = Path.Combine(value, CompanyName, ProductLine, ApplicationName);
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                LogLocation = dir;
                ConfigLocation = dir;
                rootlocation = dir;
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

        public string FFMPEGDirectory => Directory.GetParent(FFMPEGFile).FullName;


        public string LogFileLocation => Path.Combine(LogLocation, "latest.log");

        public string ConfigFileLocation => Path.Combine(ConfigLocation, "default.config");


        public ScrollViewer getScrollView()
        {
            return sv;
        }
        public void setScrollView(ScrollViewer block)
        {
            sv = block;
        }

    }

}
