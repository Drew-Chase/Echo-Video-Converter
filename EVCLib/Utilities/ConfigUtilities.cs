using ChaseLabs.CLConfiguration.List;
using ChaseLabs.Echo.Video_Converter.Resources;
using System.IO;

namespace ChaseLabs.Echo.Video_Converter.Utilities
{
    public class ConfigUtilities
    {
        private static ConfigUtilities _singleton;
        public static ConfigUtilities Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new ConfigUtilities();
                }

                return _singleton;
            }
        }
        public ConfigManager Manager;

        private ConfigUtilities()
        {
            Manager = new ConfigManager(Values.Singleton.ConfigFileLocation);
            Manager.Add("Last Used Media Directory", "");
            Manager.Add("FFMPEG File Path", Values.Singleton.DefaultFFMPEGFile);
            Manager.Add("Use Enclosed Folder", "True");
            Manager.Add("Temp Folder Location", "");
            Manager.Add("Show Encoder Console", "False");
            Manager.Add("Is Network Path", "False");
            Manager.Add("Use Nvidia NVENC", "False");
            Manager.Add("Use Hardware Encoding", "False");
            Manager.Add("Overwrite Original", "False");
            Manager.Add("Launcher Directory", "");

            //CheckDefaults();

        }

        void CheckDefaults()
        {
            if (Manager.GetConfigByKey("FFMPEG File Path").Value == "")
            {
                Manager.GetConfigByKey("FFMPEG File Path").Value = Values.Singleton.DefaultFFMPEGFile;
            }
        }
    }
}
