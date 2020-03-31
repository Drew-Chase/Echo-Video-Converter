using ChaseLabs.CLLogger;
using ChaseLabs.Echo.Video_Converter.Resources;
using System;
using System.Collections.Generic;
using System.IO;

namespace ChaseLabs.Echo.Video_Converter.Util
{
    public class ConfigUtilities
    {
        private static readonly CLLogger.Interfaces.ILog log = LogManger.Init().SetLogDirectory(Values.Singleton.LogFileLocation).EnableDefaultConsoleLogging().SetMinLogType(Lists.LogTypes.All);

        public string root_path, settings_path, log_path, last_used_media_directory, default_config_file = "default.config";
        public SortType sortType;
        public SortOrder sortOrder;
        private readonly Values reference;
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

        protected ConfigUtilities()
        {
            reference = Values.Singleton;
            //if (utilities.RootLocation == null || utilities.RootLocation.Equals(string.Empty))
            reference.RootLocation = Environment.GetEnvironmentVariable("appdata");
            root_path = reference.RootLocation;
            settings_path = reference.ConfigLocation;
            default_config_file = "default.config";
            Read(default_config_file, settings_path);
        }

        public List<string> config()
        {
            return new List<string>
            {
                $"Last Used Media Directory:{LastUsedMediaDirectory}",
                $"Overwrite Original:{OverWriteOriginal}",
                $"Use Enclosed Folder as the Temporary encode path:{UseEnclosedFolder}",
                $"TempFolderLocation:{TempFolderDirectory}",
                $"Show Encoder Console:{ShowEncodeConsole}",
                $"Is Network Path:{IsNetworkPath}",
                $"Sort Type:{MediaFiles.SortTypeToString(sortType)}",
                $"Sort Order:{MediaFiles.SortOrderToString(sortOrder)}"
            };
        }


        private bool _isnetworkpath = false;
        public bool IsNetworkPath
        {
            get => _isnetworkpath;
            set
            {
                _isnetworkpath = value;
                Write(config(), default_config_file, settings_path, true);
            }
        }

        private bool _useenclosedfolder = true;
        public bool UseEnclosedFolder
        {
            get => _useenclosedfolder;
            set
            {
                _useenclosedfolder = value;
                Write(config(), default_config_file, settings_path, true);
            }
        }

        public string LastUsedMediaDirectory
        {
            get => last_used_media_directory;
            set
            {
                last_used_media_directory = value;
                Write(config(), default_config_file, settings_path, true);
            }
        }

        private bool _showencodeconsole = false;
        public bool ShowEncodeConsole
        {
            get => _showencodeconsole;
            set
            {
                _showencodeconsole = value;
                Write(config(), default_config_file, settings_path, true);
            }
        }

        private string _TempFolderDirectory;
        public string TempFolderDirectory
        {
            get => _TempFolderDirectory;
            set
            {
                _TempFolderDirectory = value;
                Write(config(), default_config_file, settings_path, true);
            }
        }

        private bool overwriteoriginal = false;
        public bool OverWriteOriginal
        {
            get => overwriteoriginal;
            set
            {
                overwriteoriginal = value;
                Write(config(), default_config_file, settings_path, true);
            }

        }

        public void Export()
        {
            Write(config(), default_config_file, settings_path, true);
        }

        public void Write(List<string> list, string file, string path, bool resync)
        {
            try
            {
                Directory.CreateDirectory(path);
                using (StreamWriter writer = File.CreateText(Path.Combine(path, file)))
                {
                    foreach (string line in list)
                    {
                        writer.WriteLine(line);
                    }
                    writer.Flush();
                    writer.Close();
                }
                if (resync)
                {
                    Read(file, path);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("" + e.StackTrace);
                return;
            }
        }

        public void Write(string content, string file, string path, bool resync)
        {
            try
            {
                Directory.CreateDirectory(path);
                using (StreamWriter writer = File.AppendText(Path.Combine(path, file)))
                {
                    writer.WriteLine(content);
                    writer.Flush();
                    writer.Close();
                }
                if (resync)
                {
                    Read(file, path);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("" + e.StackTrace);
                return;
            }
        }


        public void Read(string file, string path)
        {
            try
            {
                Directory.CreateDirectory(path);
                using (StreamReader reader = File.OpenText(Path.Combine(path, file)))
                {
                    string text = "";
                    while (!reader.EndOfStream)
                    {
                        string line = reader.ReadLine();

                        if (line.StartsWith("Last Used Media Directory:"))
                        {
                            text = line.Replace("Last Used Media Directory:", "");
                            last_used_media_directory = text;
                        }
                        if (line.StartsWith("Overwrite Original:"))
                        {
                            text = line.Replace("Overwrite Original:", "");
                            try
                            {
                                overwriteoriginal = bool.Parse(text.ToLower());
                            }
                            catch (Exception e)
                            {
                                log.Debug(e.StackTrace);
                                overwriteoriginal = false;
                            }
                        }
                        if (line.StartsWith("Use Enclosed Folder as the Temporary encode path:"))
                        {
                            text = line.Replace("Use Enclosed Folder as the Temporary encode path:", "");
                            try
                            {
                                UseEnclosedFolder = bool.Parse(text.ToLower());
                            }
                            catch (Exception e)
                            {
                                log.Debug(e.StackTrace);
                                UseEnclosedFolder = true;
                            }
                        }

                        if (line.StartsWith("TempFolderLocation:"))
                        {
                            text = line.Replace("TempFolderLocation:", "");
                            TempFolderDirectory = text;
                        }

                        if (line.StartsWith("Show Encoder Console:"))
                        {
                            text = line.Replace("Show Encoder Console:", "");
                            bool.TryParse(text, out bool b);
                            ShowEncodeConsole = b;
                        }

                        if (line.StartsWith("Is Network Path:"))
                        {
                            text = line.Replace("Is Network Path:", "");
                            bool.TryParse(text, out bool b);
                            IsNetworkPath = b;
                        }
                        if (line.StartsWith("Sort Order:"))
                        {
                            text = line.Replace("Sort Order:", "");
                            sortOrder = MediaFiles.ParseOrder(text);
                        }
                        if (line.StartsWith("Sort Type:"))
                        {
                            text = line.Replace("Sort Type:", "");
                            sortType = MediaFiles.ParseType(text);
                        }
                    }
                    reader.Close();
                }
            }
            catch (FileNotFoundException)
            {
                Write(config(), file, path, false);
            }
            catch (Exception e)
            {
                Console.WriteLine("" + e.StackTrace);
            }

        }

    }
}
