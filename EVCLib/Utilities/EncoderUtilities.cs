using ChaseLabs.Echo.Video_Converter.Resources;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ChaseLabs.Echo.Video_Converter.Util
{

    public class EncoderUtilities
    {
        static log4net.ILog log => Logging.LogHelper.GetLogger();
        private readonly Dispatcher dis = Dispatcher.CurrentDispatcher;
        private string currentDirectory;
        public Process process;
        public bool HasAborted;
        private Button processBtn;
        private string encoding_file, file;
        private string original_path;
        public bool should_run = true, ready = false;
        private static EncoderUtilities _singleton;
        public static EncoderUtilities Singleton
        {
            get
            {
                if (_singleton == null)
                {
                    _singleton = new EncoderUtilities();
                }

                return _singleton;
            }
        }

        protected EncoderUtilities()
        {
        }


        public async Task ProcessFileAsync(string file, TextBlock tb, Button button)
        {
            processBtn = button;
            original_path = file;
            foreach (FileInfo f in Directory.GetParent(file).GetFiles())
            {
                if (f.Name.Contains($"{new FileInfo(file).Name}.done"))
                {
                    should_run = false;
                }
            }

            if (should_run)
            {
                if (Values.Singleton.ConfigUtil.IsNetworkPath)
                {
                    string f = Path.Combine(Values.Singleton.ConfigUtil.TempFolderDirectory, new FileInfo(file).Name);
                    Task task = Task.Run(new Action(() =>
                    {
                        dis.Invoke(new Action(() =>
                        {
                            log.Debug($"Copying {new FileInfo(file).Name} to {Values.Singleton.ConfigUtil.TempFolderDirectory}");
                        }), DispatcherPriority.ContextIdle);
                        if (!File.Exists(f) && file != f)
                            File.Copy(file, f);
                        else if (File.Exists(f) && file != f)
                        {
                            File.Delete(f);
                            File.Copy(file, f);
                        }
                        file = f;
                        ready = true;
                        dis.Invoke(new Action(() =>
                        {
                            log.Debug($"Coppied");
                        }), DispatcherPriority.ContextIdle);
                    }));
                    task.Wait();
                }

                dis.Invoke(new Action(() =>
                    {
                        Values.Singleton.OriginalSize.Text = $"Original Size: {FileUtilities.AdjustedFileSize(file)}";
                        Values.Singleton.OriginalSizeString = FileUtilities.AdjustedFileSize(file);
                    }), DispatcherPriority.ContextIdle);

                HasAborted = false;
                currentDirectory = Directory.GetParent(original_path).FullName;
                dis.Invoke(new Action(() =>
                {
                    Values.Singleton.setLogBlock(tb);
                    log.Info( "Processing File: " + new FileInfo(file).Name);
                }), DispatcherPriority.ContextIdle);
                this.file = file;

                string ffmpeg_file = Path.Combine(Environment.CurrentDirectory, "content", "ffmpeg.exe");
                int ffmpeg_qv = 24;
                if (!Values.Singleton.ConfigUtil.UseEnclosedFolder)
                {
                    if (Values.Singleton.ConfigUtil.TempFolderDirectory.Equals(""))
                    {
                        log.Info( "Temp Folder is not set");
                        return;
                    }

                    string fileName = new FileInfo(file).Name;
                    string ext = new FileInfo(file).Extension;
                    fileName = fileName.Replace(ext, "").Replace(".", "");
                    fileName = Path.Combine(Values.Singleton.ConfigUtil.TempFolderDirectory, fileName);
                    log.Debug(fileName);
                    encoding_file = fileName + "_Encoded" + ext;
                }
                else
                {
                    encoding_file = Path.Combine(Directory.GetParent(file).FullName, new FileInfo(file).Name.Replace(new FileInfo(file).Extension, "").Replace(".", "") + "_Encoded" + new FileInfo(file).Extension);
                }

                if (File.Exists(encoding_file))
                {
                    File.Delete(encoding_file);
                }

                string command = " -hwaccel auto -i \"" + file + "\" -pix_fmt p010le -map 0:v -map 0:a -map_metadata 0 -c:v hevc_nvenc -rc constqp -qp " + ffmpeg_qv + " -b:v 0K -c:a aac -b:a 384k -movflags +faststart -movflags use_metadata_tags \"" + encoding_file + "\"";

                process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = ffmpeg_file;
                startInfo.Arguments = command;
                if (!Values.Singleton.ConfigUtil.ShowEncodeConsole)
                {
                    startInfo.WindowStyle = ProcessWindowStyle.Hidden;
                    startInfo.CreateNoWindow = false;
                }
                else
                {
                    startInfo.WindowStyle = ProcessWindowStyle.Normal;
                    startInfo.CreateNoWindow = true;
                }
                process.Exited += new EventHandler(HandleExit);
                process.StartInfo = startInfo;
                process.Start();
                process.PriorityBoostEnabled = true;

                foreach (FileInfo f in Directory.GetParent(file).GetFiles())
                {
                    if (f.Name.Contains($"{new FileInfo(file).Name}.done"))
                    {
                        Abort(false, $"Encoder has Already processed {new FileInfo(file).Name}");
                    }
                }

                await Task.Run(new Action(() =>
                {
                    while (!process.HasExited && !HasAborted)
                    {
                        try
                        {
                            if (FileUtilities.FileSizeBytes(encoding_file) >= FileUtilities.FileSizeBytes(file))
                            {
                                Abort(true, $"Encoded File Size is larger than the original file. Moving on...");
                            }

                            dis.Invoke(new Action(() =>
                            {
                                Values.Singleton.CurrentSize.Text = $"Current Size: {FileUtilities.AdjustedFileSize(encoding_file)}";
                                Values.Singleton.CurrentSizeString = FileUtilities.AdjustedFileSize(encoding_file);
                            }), DispatcherPriority.ContextIdle);
                        }
                        catch (FileNotFoundException)
                        {

                        }
                        catch (Exception)
                        {

                        }
                    }
                }));

            }
        }

        private void HandleExit(object sender, EventArgs e)
        {
            if (HasAborted)
            {
                return;
            }

            try
            {
                float old_size = FileUtilities.FileSizeBytes(file), new_size = FileUtilities.FileSizeBytes(encoding_file);
                if (CheckFile(encoding_file))
                {
                    dis.Invoke(new Action(() =>
                    {
                        log.Info( "Check was Successful");
                    }), DispatcherPriority.ContextIdle);

                    if (old_size <= new_size)
                    {
                        dis.Invoke(new Action(() =>
                        {
                            log.Info( "Original File Size was smaller.");
                        }), DispatcherPriority.ContextIdle);
                        File.Delete(encoding_file);

                        dis.Invoke(new Action(() =>
                        {
                            log.Info( "Removing Encoded File.");
                        }), DispatcherPriority.ContextIdle);
                    }
                    else
                    {
                        float difference = old_size - new_size;
                        string percentage = Math.Round(((new_size - old_size) / old_size) * 100, 2) + "%";

                        dis.Invoke(new Action(() =>
                        {
                            log.Info( "Encode Sucessful");
                            log.Info( "You save " + FileUtilities.AdjustedFileSize(difference) + "!");
                            log.Debug($"File Size Reduced by {percentage}");
                            Values.Singleton.CurrentSize.Text = "Finished!";
                            Values.Singleton.OriginalSize.Text = $"File Size Reduced by {percentage}";
                        }), DispatcherPriority.ContextIdle);
                        if (Values.Singleton.ConfigUtil.OverWriteOriginal)
                        {
                            File.Delete(file);
                            File.Delete(original_path);
                            File.Move(encoding_file, original_path);

                            dis.Invoke(new Action(() =>
                            {
                                log.Info( "Replacing Original with Encoded File...");
                            }), DispatcherPriority.ContextIdle);
                        }
                    }
                    string done = Path.Combine(currentDirectory, $"{new FileInfo(file).Name}.done");
                    File.Create(done);
                    FileInfo FI = new FileInfo(done);
                    FI.Attributes = FileAttributes.Hidden;
                }
                else
                {
                    dis.Invoke(new Action(() =>
                    {
                        log.Error( "Check Failed!");
                        log.Error( "File Corrupted.");
                    }), DispatcherPriority.ContextIdle);
                    File.Delete(encoding_file);
                    File.Delete(file);
                    dis.Invoke(new Action(() =>
                    {
                        log.Info( "Restoring Original.");
                    }), DispatcherPriority.ContextIdle);
                }
            }
            catch (FileNotFoundException ex)
            {
                dis.Invoke(new Action(() =>
                {
                    log.Info( "File Couldn't Be Created!");
                    log.Info( "Error: " + ex.StackTrace);
                    Console.WriteLine(ex.StackTrace);
                }), DispatcherPriority.ContextIdle);
            }
            catch (Exception ex)
            {
                dis.Invoke(new Action(() =>
                {
                    log.Info( "Unknown Error: " + ex.StackTrace);
                    Console.WriteLine(ex.StackTrace);
                }), DispatcherPriority.ContextIdle);
            }
        }
        public void Abort(bool safe_abort, params string[] message)
        {
            try
            {
                HasAborted = true;
                process.Kill();

                if (safe_abort)
                {
                    string done = Path.Combine(currentDirectory, $"{new FileInfo(file).Name}.done");
                    File.Create(done);
                    FileInfo FI = new FileInfo(done);
                    FI.Attributes = FileAttributes.Hidden;
                }

                dis.Invoke(new Action(() =>
                {
                    processBtn.IsEnabled = true;
                    foreach (string s in message)
                    {
                        log.Debug(s);
                    }
                }));
                if (encoding_file != string.Empty && File.Exists(encoding_file))
                {
                    System.Threading.Thread.Sleep(1 * 1000);
                    File.Delete(encoding_file);
                    if (Values.Singleton.ConfigUtil.IsNetworkPath && ready && !file.Equals(original_path))
                    {
                        File.Delete(file);
                    }
                }
                log.Warn( "Process Aborted!");
            }
            catch (Exception) { }
        }

        private bool CheckFile(string new_file)
        {
            dis.Invoke(new Action(() =>
            {
                log.Info( "Checking File...");
            }), DispatcherPriority.ContextIdle);
            if (FileUtilities.FileSizeMB(new_file) >= 1)
            {
                return true;
            }

            return false;
        }

    }
}
