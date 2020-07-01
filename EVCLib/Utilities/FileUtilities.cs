using ChaseLabs.CLLogger;
using ChaseLabs.Echo.Video_Converter.Resources;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Threading;

namespace ChaseLabs.Echo.Video_Converter.Utilities
{
    public class FileUtilities
    {
        private static readonly CLLogger.Interfaces.ILog log = LogManger.Init().SetLogDirectory(Values.Singleton.LogFileLocation).EnableDefaultConsoleLogging().SetMinLogType(Lists.LogTypes.All);
        public enum FileExtensionType
        {
            All,
            Media,
            Log,
            Txt,
            Image,
            Directory,
            Executable
        }

        private static FileUtilities _singleton;
        public static FileUtilities Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new FileUtilities();
                }

                return _singleton;
            }
        }

        protected FileUtilities() { }

        public static string directory;

        public static System.Windows.Threading.Dispatcher dis => System.Windows.Threading.Dispatcher.CurrentDispatcher;

        public static bool IsSingleFile(string path)
        {
            FileAttributes attr = File.GetAttributes(path);

            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
            {
                return false;
            }
            else
            {
                return true;
            }
        }


        public static string OpenFolder(string InitDirectory, string Title)
        {

            using (FolderBrowserDialog folder = new FolderBrowserDialog())
            {
                folder.ShowNewFolderButton = true;
                folder.SelectedPath = InitDirectory;
                folder.Description = Title;
                DialogResult result = folder.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folder.SelectedPath))
                {
                    return folder.SelectedPath;
                }
                else if (result == DialogResult.Cancel)
                {
                    return string.Empty;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        /// <summary>
        /// Opens a File or Folder Selection Dialog
        /// </summary>
        /// <param name="type">File Extension Type</param>
        /// <param name="InitDirectory">Initial Directory</param>
        /// <param name="Title">Dialog Title</param>
        /// <returns></returns>
        public static string OpenFile(FileExtensionType? type, string InitDirectory, string Title)
        {
            using (OpenFileDialog file = new OpenFileDialog())
            {
                file.InitialDirectory = InitDirectory;
                file.Title = Title;
                switch (type)
                {
                    case FileExtensionType.Media:
                        string filter = "";
                        int index = 0;
                        foreach (string ext in Values.MediaExtensions)
                        {
                            if (ext != "done")
                            {
                                filter += $"*.{ext}";
                            }

                            if (index < Values.MediaExtensions.Length - 1 && index != 0)
                            {
                                filter += ";";
                            }

                            index++;
                        }
                        file.Filter = $"Media Files|{filter}";
                        break;
                    case FileExtensionType.Directory:
                        file.Filter = "Folders|\n";
                        file.ValidateNames = false;
                        file.AddExtension = false;
                        file.CheckFileExists = false;
                        file.CheckPathExists = true;
                        file.DereferenceLinks = true;
                        file.Multiselect = false;
                        file.FileName = "Folder";
                        break;
                    case FileExtensionType.Executable:
                        file.Filter = "EXE|*.exe";
                        break;
                    case FileExtensionType.Txt:
                        file.Filter = "TXT|*.txt;*.log;*.cfg;*.config;*.conf;";
                        break;
                    default:
                        file.Filter = "All Files (*.*)|*.*";
                        break;
                }

                DialogResult result = file.ShowDialog();
                if (result == DialogResult.OK && file.CheckFileExists)
                {
                    return file.FileName;
                }
                else if (result == DialogResult.Cancel)
                {
                    return string.Empty;
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public static async Task<MediaFiles> GetFilesAsync(params string[] _dir)
        {
            string dir = string.Empty;
            if (_dir.Length == 0)
            {
                dir = OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Select the Root Folder of the Media Files.");
            }
            else
            {
                dir = _dir[0];
            }

            if (dir != null || dir != string.Empty)
            {
                directory = dir;
                MediaFiles file = new MediaFiles();

                file = await Task.Run(() => Files(dir)).ConfigureAwait(continueOnCapturedContext: false);
                return file;
            }
            return new MediaFiles();
        }

        private static MediaFiles Files(string dir)
        {
            dis.Invoke(new Action(() =>
            {
                log.Info("Processing Directories and all Subdirectories.");
            }), DispatcherPriority.ContextIdle);

            MediaFiles MediaFiles = new MediaFiles();
            foreach (string s in Values.MediaExtensions)
            {
                foreach (string j in Directory.GetFiles(dir, "*." + s, SearchOption.AllDirectories))
                {

                    FileInfo fileInfo = new FileInfo(j);
                    if (!(fileInfo.Name.Contains($".done")))
                    {
                        dis.Invoke(new Action(() =>
                        {
                            log.Debug($"Found {fileInfo.Name}");
                        }), DispatcherPriority.ContextIdle);
                    }
                    else
                    {
                        dis.Invoke(new Action(() =>
                        {
                            log.Debug($"Already Proccessed {fileInfo.Name}");
                        }), DispatcherPriority.ContextIdle);
                    }
                    bool should_add = true;
                    foreach (FileInfo f in Directory.GetParent(j).GetFiles())
                    {
                        if (f.Name.Contains($"{fileInfo.Name}.done"))
                        {
                            should_add = false;
                        }
                    }
                    if (should_add)
                    {
                        MediaFiles.Add(new MediaFile() { FilePath = j, ID = MediaFiles.Count + 1 });
                    }
                }
            }
            return MediaFiles;
        }

        public static long FileSizeBytes(string file)
        {
            try
            {
                FileInfo info = new FileInfo(file);
                long num = info.Length;
                return num;
            }
            catch (System.IO.FileNotFoundException) { }
            catch { }
            return 0;
        }
        public static double FileSizeKB(string file)
        {
            double num = FileSizeBytes(file) / 1024;
            double.TryParse("" + num, NumberStyles.Any, CultureInfo.InvariantCulture, out num);
            num = Math.Round(num, 2);
            return num;
        }
        public static double FileSizeMB(string file)
        {
            double num = FileSizeKB(file) / 1024;
            double.TryParse("" + num, NumberStyles.Any, CultureInfo.InvariantCulture, out num);
            num = Math.Round(num, 2);
            return num;
        }
        public static double FileSizeGB(string file)
        {
            double num = FileSizeMB(file) / 1024;
            double.TryParse("" + num, NumberStyles.Any, CultureInfo.InvariantCulture, out num);
            num = Math.Round(num, 2);
            return num;
        }


        public static double FileSizeKB(double file)
        {
            double num = file / 1024;
            double.TryParse("" + num, NumberStyles.Any, CultureInfo.InvariantCulture, out num);
            num = Math.Round(num, 2);
            return num;
        }
        public static double FileSizeMB(double file)
        {
            double num = FileSizeKB(file) / 1024;
            double.TryParse("" + num, NumberStyles.Any, CultureInfo.InvariantCulture, out num);
            num = Math.Round(num, 2);
            return num;
        }
        public static double FileSizeGB(double file)
        {
            double num = FileSizeMB(file) / 1024;
            double.TryParse("" + num, NumberStyles.Any, CultureInfo.InvariantCulture, out num);
            num = Math.Round(num, 2);
            return num;
        }

        public static string AdjustedFileSize(string file)
        {
            return (FileSizeBytes(file) < 1024) ? FileSizeBytes(file) + "B" : (FileSizeKB(file) < 1024) ? FileSizeKB(file) + "KB" : (FileSizeMB(file) < 1024) ? FileSizeMB(file) + "MB" : FileSizeGB(file) + "GB";
        }

        public static string AdjustedFileSize(double size)
        {
            return (Math.Round(double.Parse("" + size, NumberStyles.Any, CultureInfo.InvariantCulture), 2) < 1024) ? Math.Round(double.Parse("" + size, NumberStyles.Any, CultureInfo.InvariantCulture), 2) + "B" : (Math.Round(FileSizeKB(size), 2) < 1024) ? Math.Round(FileSizeKB(size), 2) + "KB" : (Math.Round(FileSizeMB(size), 2) < 1024) ? Math.Round(FileSizeMB(size), 2) + "MB" : Math.Round(FileSizeGB(size), 2) + "GB";
        }
    }
}
