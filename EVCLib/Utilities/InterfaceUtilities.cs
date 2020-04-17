using ChaseLabs.CLLogger;
using ChaseLabs.CLUpdate;
using ChaseLabs.Echo.Video_Converter.Resources;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace ChaseLabs.Echo.Video_Converter.Utilities
{
    public class InterfaceUtilities
    {
        public Dispatcher dis => Dispatcher.CurrentDispatcher;
        private static readonly CLLogger.Interfaces.ILog log = LogManger.Init().SetLogDirectory(Values.Singleton.LogFileLocation).EnableDefaultConsoleLogging().SetMinLogType(Lists.LogTypes.All);
        private static InterfaceUtilities _singleton;
        public static InterfaceUtilities Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new InterfaceUtilities();
                }

                return _singleton;
            }
        }

        private readonly Values values;
        protected InterfaceUtilities()
        {
            values = Values.Singleton;
        }


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

        public void CleanLogs()
        {
            if (File.Exists(Values.Singleton.LogFileLocation))
            {
                foreach (string file in Directory.GetFiles(Directory.GetParent(Values.Singleton.LogFileLocation).FullName))
                {
                    if (!new FileInfo(file).Name.Equals("latest.log"))
                    {
                        try
                        {
                            File.Delete(file);
                        }
                        catch (FileNotFoundException)
                        {
                            log.Error($"\"{file}\" was not found and therefore couldn't be removed");
                        }
                        catch (IOException e)
                        {
                            log.Error($"There was an Error Removing -> \"{file}\"");
                        }
                        catch (Exception e)
                        {
                            log.Error($"{e.InnerException.GetType().Name} Error was Thrown while Removing -> \"{file}\"");
                        }
                    }
                }
            }
        }

        public string SelectWorkingDirectory(string locationText)
        {
            string dir = string.Empty;
            if (locationText != string.Empty || locationText != null)
            {
                try
                {
                    dir = FileUtilities.OpenFolder(locationText, "Select the Root Folder of the Media Files.");
                }
                catch (Exception)
                {
                    dir = FileUtilities.OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Select the Root Folder of the Media Files.");
                }
            }
            else
            {
                dir = FileUtilities.OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Select the Root Folder of the Media Files.");
            }

            if (dir != string.Empty && dir != "" && dir != " ")
            {
                locationText = dir;
                log.Debug(@"File location set to " + dir);
                Values.Singleton.LastUsedMediaDirectory = dir;
            }
            else
            {
                locationText = "Source ";
                log.Debug(@"File location was not found");
            }
            return locationText;
        }

        public string SelectWorkingFile(string text)
        {

            string dir = string.Empty;
            if (text != string.Empty || text != null)
            {
                try
                {
                    dir = FileUtilities.OpenFile(FileUtilities.FileExtensionType.Media, text, "Select the Media file to Be Compressed.");

                }
                catch (Exception)
                {
                    dir = FileUtilities.OpenFile(FileUtilities.FileExtensionType.Media, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Select the Media file to Be Compressed.");
                }
            }
            else
            {
                dir = FileUtilities.OpenFile(FileUtilities.FileExtensionType.Media, Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Select the Media file to Be Compressed.");
            }

            if (dir != string.Empty && dir != "" && dir != " ")
            {
                text = dir;
                log.Debug(@"File location set to " + dir);
                Values.Singleton.LastUsedMediaDirectory = dir;
            }
            else
            {
                text = "Source ";
                log.Debug(@"File location was not found");
            }
            return text;
        }

    }
}
