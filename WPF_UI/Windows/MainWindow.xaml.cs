using ChaseLabs.CLLogger;
using ChaseLabs.CLLogger.Events;
using ChaseLabs.CLLogger.Interfaces;
using ChaseLabs.Echo.Video_Converter.Resources;
using ChaseLabs.Echo.Video_Converter.Util;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace ChaseLabs.Echo.Video_Converter.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dispatcher dis = Dispatcher.CurrentDispatcher;
        LogManger log = LogManger.Init().SetLogDirectory(Values.Singleton.LogFileLocation).EnableDefaultConsoleLogging().SetMinLogType(Lists.LogTypes.All);
        private Util.Utilities utilities;
        private EncoderUtilities encode_util = null;
        private ConfigUtilities config;
        //public static Values values = null;
        private bool HasAborted = false;
        private Settings settingsWindow = null;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        double seconds = 2;
        long currentTime = DateTime.Now.Ticks, neededTime = 0;

        public MainWindow()
        {
            InitializeComponent();
            onStartUp();
            neededTime = DateTime.Now.AddSeconds(seconds).Ticks;
            Task.Run(() =>
            {
                while (true)
                {
                    currentTime = DateTime.Now.Ticks;
                    if (currentTime >= neededTime)
                    {
                        Update();
                        neededTime = DateTime.Now.AddSeconds(seconds).Ticks;
                    }
                }
            });
        }

        private void onStartUp()
        {
            Closed += ApplicationClosedEvent;
            log.LoggedMessage += OnLogMessage;
            utilities = Util.Utilities.Singleton;
            config = ConfigUtilities.Singleton;
            Values.Singleton.ConfigUtil = config;
            Values.Singleton.setScrollView(ConsoleOutputScrView);
            if (config.LastUsedMediaDirectory != "")
            {
                FileLocationTxb.Text = config.LastUsedMediaDirectory;
            }
            Values.Singleton.OriginalSize = OriginalSize_Txb;
            Values.Singleton.CurrentSize = CurrentSize_Txb;
            encode_util = EncoderUtilities.Singleton;
            string startTime = DateTime.Now.ToString();
            SystemTray();
            log.Info("------------Log Start-------------------");
        }

        void Update()
        {
            dis.Invoke(new Action(() =>
            {
                try
                {
                    using (var reader = new StreamReader(Values.Singleton.LogFileLocation))
                    {
                        ConsoleOutputTxtBlk.Text = reader.ReadToEnd();
                        ConsoleOutputScrView.ScrollToBottom();
                    }
                }
                catch
                {
                }
            }), DispatcherPriority.ContextIdle);
        }

        private void OnLogMessage(object sender, LogEventArgs args)
        {
            //try
            //{
            //    using (var reader = new StreamReader(Values.Singleton.LogFileLocation))
            //    {
            //        ConsoleOutputTxtBlk.Text = reader.ReadToEnd();
            //    }
            //}
            //catch
            //{

            //}
            //ConsoleOutputTxtBlk.Text += args.Log + Environment.NewLine;
            Console.WriteLine(args.Log);
        }

        private void ApplicationClosedEvent(object sender, EventArgs e)
        {
            //Close();
        }

        private void OpenFolderBtn_Click(object sender, RoutedEventArgs e)
        {
            string dir = string.Empty;
            if (FileLocationTxb.Text != string.Empty || FileLocationTxb.Text != null)
            {
                try
                {
                    dir = FileUtilities.OpenFolder(FileLocationTxb.Text, "Select the Root Folder of the Media Files.");
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
                FileLocationTxb.Text = dir;
                log.Debug(@"File location set to " + dir);
                config.LastUsedMediaDirectory = dir;
            }
            else
            {
                FileLocationTxb.Text = "Source ";
                log.Debug(@"File location was not found");
            }
        }

        private void Clear(object value)
        {
            if (value.GetType().Equals(typeof(TextBlock)))
            {
                TextBlock item = (TextBlock)value;
                item.Text = string.Empty;
            }
            else if (value.GetType().Equals(typeof(TextBox)))
            {
                TextBox item = (TextBox)value;
                item.Text = string.Empty;
            }
        }

        private async void ProcessBtn_Click(object sender, RoutedEventArgs args)
        {
            if (config.IsNetworkPath && config.UseEnclosedFolder)
            {
                log.Warn("No Temp Directory Specified");
                log.Warn("A Temp Directory needs to be specified if the file is on a network path");
                HasAborted = true;
                return;
            }

            if (!config.UseEnclosedFolder && config.TempFolderDirectory.Equals(""))
            {
                log.Warn("No Temp Directory Specified");
                HasAborted = true;
                return;
            }
            if (FileLocationTxb.Text.ToLower() == "source")
            {
                log.Warn("No Directory Specified");
                HasAborted = true;
                return;
            }

            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxResult notify = MessageBox.Show("This could take a REALLY long time! It may seem frozen, but its not.\nWould you like to continue?", "Warning", button);
            if (notify == MessageBoxResult.No)
            {
                log.Warn("Process Canceled");
                log.Debug("No Hard Feelings.");
                HasAborted = true;
                return;
            }
            if (!HasAborted)
            {
                ProcessBtn.IsEnabled = false;
            }

            MediaFiles file = new MediaFiles();
            string path = FileLocationTxb.Text;
            if (FileUtilities.IsSingleFile(FileLocationTxb.Text))
            {
                file.Add(new MediaFile() { FilePath = path, ID = 0 });
            }
            else
            {
                file = await Task.Run(() => FileUtilities.GetFilesAsync(path));
            }
            if (file.Count() == 0)
            {
                log.Error("No Media Files Found in " + FileUtilities.directory);
                HasAborted = true;
                ProcessBtn.IsEnabled = true;
                return;
            }
            foreach (MediaFile value in file.Get(SortType.Size, SortOrder.Ascending))
            {
                if (HasAborted)
                {
                    ProcessBtn.IsEnabled = true;
                    return;
                }
                try
                {
                    await Task.Run(() => encode_util.ProcessFileAsync(value.FilePath, ProcessBtn));
                }
                catch (Exception e)
                {
                    log.Error("Check if the FFMPEG File is Availible", e);
                }
            }
            ProcessBtn.IsEnabled = true;
            WindowState = WindowState.Normal;
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            if (encode_util == null || encode_util.process == null)
            {
                Close();
                return;
            }
            MessageBoxResult result = MessageBox.Show("Are you sure you want to quit?\nThis Could corrupt any unprocessed files.", "Warning", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
            {
                Close();
            }
            else
            {
                log.Info("Exit Canceld!");
                log.Warn("Encoding Still Running!");
            }
        }

        public new void Close()
        {
            WindowState = WindowState.Normal;
            encode_util.Abort(false);
            string endTime = DateTime.Now.ToString();
            log.Info("--------------Log End---------------------" + Environment.NewLine);
            base.Close();
            Environment.Exit(0);
        }

        private void Window_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void StopBtn_Click(object sender, RoutedEventArgs e)
        {
            if ((encode_util != null && encode_util.process != null && !encode_util.process.HasExited) || !encode_util.ready)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to Abort?\nThis Could corrupt any unprocessed files.", "Warning", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    encode_util.Abort(false);
                    HasAborted = true;
                }
            }
        }

        private void SettingsBtn_Click(object sender, RoutedEventArgs e)
        {
            OpenSettings();
        }

        private void OpenSettings()
        {
            if (settingsWindow == null)
            {
                settingsWindow = new Settings();
            }

            settingsWindow.Show();
            if (!settingsWindow.IsLoaded)
            {
                settingsWindow.Show();
            }
            else if (settingsWindow.WindowState == WindowState.Minimized)
            {
                settingsWindow.WindowState = WindowState.Normal;
            }
            else
            {
                settingsWindow.Activate();
            }
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void SkipBtn_Click(object sender, RoutedEventArgs e)
        {
            if (encode_util != null && encode_util.process != null && !encode_util.process.HasExited)
            {
                encode_util.Abort(false);
            }
        }

        private System.Windows.Forms.NotifyIcon NotifyIcon;

        private void SystemTray()
        {
            NotifyIcon = new System.Windows.Forms.NotifyIcon();
            NotifyIcon.Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location);
            NotifyIcon.MouseDoubleClick += NotifyIcon_MouseDoubleClick;
            StateChanged += Window_StateChanged;
            contextMenu = new System.Windows.Forms.ContextMenuStrip();
            //contextMenu.Items.Add($"C: {(Values.Singleton.CurrentSizeString == string.Empty ? Values.Singleton.CurrentSizeString : "N/A")} | O: {(Values.Singleton.OriginalSizeString == string.Empty ? Values.Singleton.OriginalSizeString : "N/A")}", null, new EventHandler((object sender, EventArgs args) => { WindowState = WindowState.Normal; }));
            contextMenu.Items.Add("Show", null, new EventHandler((object sender, EventArgs args) => { WindowState = WindowState.Normal; }));
            contextMenu.Items.Add("Settings", null, new EventHandler((object sender, EventArgs args) => { OpenSettings(); }));
            contextMenu.Items.Add("Exit", null, new EventHandler((object sender, EventArgs args) => { Close(); }));
            NotifyIcon.ContextMenuStrip = contextMenu;
        }


        private void NotifyIcon_MouseDoubleClick(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            WindowState = WindowState.Normal;
        }
        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                ShowInTaskbar = false;
                NotifyIcon.BalloonTipTitle = "Minimize Sucessful";
                NotifyIcon.BalloonTipText = "Minimized the app ";
                NotifyIcon.ShowBalloonTip(400);
                NotifyIcon.Visible = true;
            }
            else if (WindowState == WindowState.Normal)
            {
                NotifyIcon.Visible = false;
                ShowInTaskbar = true;
                Activate();
            }
        }

        private void SelectFileBtn_Click(object sender, RoutedEventArgs e)
        {
            string dir = string.Empty;
            if (FileLocationTxb.Text != string.Empty || FileLocationTxb.Text != null)
            {
                try
                {
                    dir = FileUtilities.OpenFile(FileUtilities.FileExtensionType.Media, FileLocationTxb.Text, "Select the Media file to Be Compressed.");

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
                FileLocationTxb.Text = dir;
                log.Debug(@"File location set to " + dir);
                config.LastUsedMediaDirectory = dir;
            }
            else
            {
                FileLocationTxb.Text = "Source ";
                log.Debug(@"File location was not found");
            }
        }
    }
}
