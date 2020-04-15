using ChaseLabs.CLConfiguration.List;
using ChaseLabs.CLConfiguration.Object;
using ChaseLabs.CLLogger;
using ChaseLabs.Echo.Video_Converter.Resources;
using System;
using System.Collections.Generic;
using System.IO;

namespace ChaseLabs.Echo.Video_Converter.Util
{
    public class ConfigUtilities
    {
        private static ConfigUtilities _singleton;
        public static ConfigUtilities Singleton
        {
            get
            {
                if (_singleton == null) _singleton = new ConfigUtilities();
                return _singleton;
            }
        }
        public ConfigManager Manager;
        ConfigUtilities()
        {
            Manager = new ConfigManager(Values.Singleton.ConfigFileLocation);
            Manager.Add("Last Used Media Directory", "");
            Manager.Add("FFMPEG File Path", Path.Combine(Values.Singleton.ApplicationDirectory, "FFMPEG", "bin", "x64", "ffmpeg.exe"));
            Manager.Add("Use Enclosed Folder", "True");
            Manager.Add("Temp Folder Location", "");
            Manager.Add("Show Encoder Console", "False");
            Manager.Add("Is Network Path", "False");
            Manager.Add("Use Nvidia NVENC", "False");
            Manager.Add("Overwrite Original", "False");
        }
    }
}
