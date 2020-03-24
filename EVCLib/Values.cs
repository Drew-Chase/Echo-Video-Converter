using ChaseLabs.Echo.Video_Converter.Util;
using System;
using System.IO;
using System.Windows.Controls;

namespace ChaseLabs.Echo.Video_Converter.Resources
{
    public class Values
    {
        static log4net.ILog log => Logging.LogHelper.GetLogger();
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

        public static string[] MediaExtensions => new string[] { "done", "mpegg", "mpeg", "mp4", "mkv", "m4a", "m4v", "f4v", "f4a", "m4b", "m4r", "f4b", "mov", "3gp", "3gp2", "3g2", "3gpp", "3gpp2", "ogg", "oga", "ogv", "ogx", "wmv", "wma", "flv", "avi" };


        protected Values()
        {
        }

        public string CurrentSizeString { get; set; }
        public string OriginalSizeString { get; set; }

        public string FormattedTime => DateTime.Now.ToString().Replace(":", "-").Replace("/", "-");
        public string UnformattedTime => DateTime.Now.ToString();

        public MediaFiles mediaFiles => new MediaFiles();

        private TextBlock LogBlock = null;
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

        private string _buildVersion = "N/A";
        public string BuildVersion
        {
            get => _buildVersion;
            set => _buildVersion = "build v." + value;
        }

        public string InstallationFolder => Environment.CurrentDirectory;


        private ConfigUtilities configutil;
        public ConfigUtilities ConfigUtil
        {
            get => configutil;
            set => configutil = value;
        }


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
                string dir = Path.Combine(value, "Chase Labs", "Echo Video Converter");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }

                LogLocation = dir;
                ConfigLocation = dir;
                rootlocation = dir;
            }
        }

        public string LogFileLocation => Path.Combine(LogLocation, "latest.log");

        public string ConfigFileLocation => Path.Combine(ConfigLocation, "default.config");


        public void setLogBlock(TextBlock block)
        {
            LogBlock = block;
            Log<string>.Singleton.setLogBlock(getLogBlock());
        }

        public ScrollViewer getScrollView()
        {
            return sv;
        }
        public void setScrollView(ScrollViewer block)
        {
            sv = block;
            Log<string>.Singleton.setScrollView(getScrollView());
        }

        public TextBlock getLogBlock()
        {
            return LogBlock;
        }

    }

}
