using ChaseLabs.CLLogger;
using ChaseLabs.Echo.Video_Converter.Resources;
using System.Diagnostics;
using System.IO;

namespace ChaseLabs.Echo.Video_Converter.Util
{
    public class Utilities
    {
        private static readonly CLLogger.Interfaces.ILog log = LogManger.Init().SetLogDirectory(Values.Singleton.LogFileLocation).EnableDefaultConsoleLogging().SetMinLogType(Lists.LogTypes.All);
        private static Utilities _singleton;
        public static Utilities Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new Utilities();
                }

                return _singleton;
            }
        }

        private readonly Values values;
        protected Utilities()
        {
            values = Values.Singleton;
        }

        public System.Windows.Threading.Dispatcher dis => System.Windows.Threading.Dispatcher.CurrentDispatcher;

        public void OpenFolder(string folderPath)
        {
            if (Directory.Exists(folderPath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    Arguments = folderPath,
                    FileName = "explorer.exe"
                };

                Process.Start(startInfo);
            }
            else
            {
                log.Debug(string.Format("{0} Directory does not exist!", folderPath));
            }
        }

        public void OpenFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    //Arguments = filePath,
                    FileName = filePath//"explorer.exe"
                };

                Process.Start(startInfo);
            }
            else
            {
                log.Debug(string.Format("{0} File does not exist!", filePath));
            }
        }
    }
}
