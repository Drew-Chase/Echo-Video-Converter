using ChaseLabs.CLLogger;
using ChaseLabs.Echo.Video_Converter.Resources;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ChaseLabs.Echo.Video_Converter.Utilities
{

    public class EncoderUtilities
    {
        private static readonly CLLogger.Interfaces.ILog log = LogManger.Init().SetLogDirectory(Values.Singleton.LogFileLocation).EnableDefaultConsoleLogging().SetMinLogType(Lists.LogTypes.All);
        private Dispatcher dis = Dispatcher.CurrentDispatcher;
        private string currentDirectory;
        public Process process;
        public bool HasAborted = true;
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


        public async Task ProcessFileAsync(string file)
        {
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
                if (Values.Singleton.IsNetworkPath)
                {
                    string f = Path.Combine(Values.Singleton.TempFolderLocation, new FileInfo(file).Name);
                    Task task = Task.Run(new Action(() =>
                    {
                        dis.Invoke(new Action(() =>
                        {
                            log.Debug($"Copying {new FileInfo(file).Name} to {Values.Singleton.TempFolderLocation}");
                        }), DispatcherPriority.ContextIdle);
                        if (!File.Exists(f) && file != f)
                        {
                            File.Copy(file, f);
                        }
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
                }), DispatcherPriority.ContextIdle);
                log.Info("Processing File: " + new FileInfo(file).Name);
                this.file = file;

                string ffmpeg_file = Values.Singleton.FFMPEGFile;
                int ffmpeg_qv = 24;
                if (!Values.Singleton.UseEnclosedFolder)
                {
                    if (Values.Singleton.TempFolderLocation.Equals(""))
                    {
                        log.Info("Temp Folder is not set");
                        return;
                    }

                    string fileName = new FileInfo(file).Name;
                    string ext = new FileInfo(file).Extension;
                    fileName = fileName.Replace(ext, "").Replace(".", "");
                    fileName = Path.Combine(Values.Singleton.TempFolderLocation, fileName);
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

                string command = "";
                if (Values.Singleton.UseNvidiaNVENC)
                {
                    command = $" -hwaccel auto -i \"{file}\" -pix_fmt p010le -map 0:v -map 0:a -map_metadata 0 -c:v hevc_nvenc -rc constqp -qp {ffmpeg_qv} -b:v 0K -c:a aac -b:a 384k -movflags +faststart -movflags use_metadata_tags \"{encoding_file}\"";
                }
                else if (Values.Singleton.UseHardwareEncoding)
                {
                    command = $" -hwaccel -hwaccel_device auto -i \"{file}\" -pix_fmt p010le -map 0:v -map 0:a -map_metadata 0 -c:v libx264 -rc constqp -qp {ffmpeg_qv} -b:v 0K -c:a aac -b:a 384k -movflags +faststart -movflags use_metadata_tags \"{encoding_file}\"";
                }
                else
                {
                    command = $" auto -i \"{file}\" -pix_fmt p010le -map 0:v -map 0:a -map_metadata 0 -c:v libx264 -rc constqp -qp {ffmpeg_qv} -b:v 0K -c:a aac -b:a 384k -movflags +faststart -movflags use_metadata_tags \"{encoding_file}\"";
                }

                process = new Process();
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    FileName = ffmpeg_file,
                    Arguments = command
                };
                if (!Values.Singleton.ShowEncoderConsole)
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
                try
                {
                    process.Start();
                }
                catch (Exception e)
                {
                    log.Error("Check if the FFMPEG File is Availible", e);
                    if (File.Exists(encoding_file))
                    {
                        File.Delete(encoding_file);
                    }
                    string done = Path.Combine(currentDirectory, $"{new FileInfo(file).Name}.done");
                    File.Create(done);
                    new FileInfo(done).Attributes = FileAttributes.Hidden;
                    return;
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
                                try
                                {
                                    Values.Singleton.CurrentSize.Text = $"Current Size: {FileUtilities.AdjustedFileSize(encoding_file)}";
                                    Values.Singleton.CurrentSizeString = FileUtilities.AdjustedFileSize(encoding_file);
                                }
                                catch (Exception e)
                                {
                                    log.Error("Failed to Retrieve the Encoding File Data", e);
                                }
                            }), DispatcherPriority.ContextIdle);
                        }
                        catch (FileNotFoundException e)
                        {
                            log.Error($"Could Not Find File {e.FileName}", e);
                        }
                        catch (Exception e)
                        {
                            log.Error("Unknown Error Has Occurred", e);
                        }
                    }
                }));

            }
        }

        private void HandleExit(object sender, EventArgs e)
        {
            HandleExit(sender, e, 0);
        }

        private void HandleExit(object sender, EventArgs @event, int attempt)
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
                        log.Info("Check was Successful");
                    }), DispatcherPriority.ContextIdle);

                    if (old_size <= new_size)
                    {
                        dis.Invoke(new Action(() =>
                        {
                            log.Info("Original File Size was smaller.");
                        }), DispatcherPriority.ContextIdle);
                        File.Delete(encoding_file);

                        dis.Invoke(new Action(() =>
                        {
                            log.Info("Removing Encoded File.");
                        }), DispatcherPriority.ContextIdle);
                    }
                    else
                    {
                        float difference = old_size - new_size;
                        string percentage = Math.Round(((new_size - old_size) / old_size) * 100, 2) + "%";

                        dis.Invoke(new Action(() =>
                        {
                            log.Info("Encode Sucessful");
                            log.Info("You save " + FileUtilities.AdjustedFileSize(difference) + "!");
                            log.Debug($"File Size Reduced by {percentage}");
                            Values.Singleton.CurrentSize.Text = "Finished!";
                            Values.Singleton.OriginalSize.Text = $"File Size Reduced by {percentage}";
                        }), DispatcherPriority.ContextIdle);
                        if (Values.Singleton.OverwriteOriginal)
                        {
                            System.Threading.Thread.Sleep((int)(1.5 * 1000));
                            try
                            {
                                File.Delete(file);
                                File.Delete(original_path);
                                File.Move(encoding_file, original_path);
                            }
                            catch (IOException e)
                            {
                                log.Warn($"Attempted to Remove the Encoded Files from Temp Workspace, but was unsuccessful...  {attempt <= 2: Trying again ? Not Proceeding Further}", e);
                                if (attempt <= 2)
                                {
                                    HandleExit(sender, @event, attempt + 1);
                                }
                            }
                            catch (Exception e)
                            {
                                log.Error("Unknown Error has Occurred!", e);
                            }
                            dis.Invoke(new Action(() =>
                            {
                                log.Info("Replacing Original with Encoded File...");
                            }), DispatcherPriority.ContextIdle);
                        }
                    }
                    string done = Path.Combine(currentDirectory, $"{new FileInfo(file).Name}.done");
                    File.Create(done);
                    FileInfo FI = new FileInfo(done)
                    {
                        Attributes = FileAttributes.Hidden
                    };
                }
                else
                {
                    dis.Invoke(new Action(() =>
                    {
                        log.Error("Check Failed!");
                        log.Error("File Corrupted.");
                    }), DispatcherPriority.ContextIdle);
                    File.Delete(encoding_file);
                    if (Values.Singleton.IsNetworkPath)
                        File.Delete(file);
                    dis.Invoke(new Action(() =>
                    {
                        log.Info("Restoring Original.");
                    }), DispatcherPriority.ContextIdle);
                }
            }
            catch (FileNotFoundException ex)
            {
                dis.Invoke(new Action(() =>
                {
                    log.Info("File Couldn't Be Created!");
                    log.Info("Error: " + ex.StackTrace);
                    Console.WriteLine(ex.StackTrace);
                }), DispatcherPriority.ContextIdle);
            }
            catch (Exception ex)
            {
                dis.Invoke(new Action(() =>
                {
                    log.Info("Unknown Error: " + ex.StackTrace);
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
                    FileInfo FI = new FileInfo(done)
                    {
                        Attributes = FileAttributes.Hidden
                    };
                }

                dis.Invoke(new Action(() =>
                {
                        log.Debug(message);
                }));
                if (encoding_file != string.Empty && File.Exists(encoding_file))
                {
                    System.Threading.Thread.Sleep(1 * 1000);
                    File.Delete(encoding_file);
                    if (Values.Singleton.IsNetworkPath && ready && !file.Equals(original_path))
                    {
                        File.Delete(file);
                    }
                }
                log.Warn("Process Aborted!");
            }
            catch { }
        }

        private bool CheckFile(string new_file)
        {
            dis.Invoke(new Action(() =>
            {
                log.Info("Checking File...");
            }), DispatcherPriority.ContextIdle);



            if (FileUtilities.FileSizeMB(new_file) <= 1)
            {
                log.Error($"{new_file} was smaller than 1Mb");
                return false;
            }

            return true;
        }

    }
}
