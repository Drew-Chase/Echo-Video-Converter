using ChaseLabs.CLLogger;
using ChaseLabs.Echo.Video_Converter.Resources;
using ChaseLabs.Echo.Video_Converter.Utilities;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace ChaseLabs.Echo.Video_Converter.Windows
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dispatcher dis = Dispatcher.CurrentDispatcher;
        private readonly LogManger log = LogManger.Init().SetLogDirectory(Values.Singleton.LogFileLocation).EnableDefaultConsoleLogging().SetMinLogType(Lists.LogTypes.All);
        private Settings settingsWindow = null;
        private System.Windows.Forms.ContextMenuStrip contextMenu;
        private System.Windows.Forms.NotifyIcon NotifyIcon;

        private EncoderUtilities encoder = EncoderUtilities.Singleton;

        public MainWindow()
        {
            InitializeComponent();
            SystemTray();
            RegisterEvents();
            onStartUp();
            Update();
        }

        private void RegisterEvents()
        {
            //log.LoggedMessage += OnLogMessage;
            //Closed += ApplicationClosedEvent;
            OpenFolderBtn.Click += ((object sender, RoutedEventArgs e) => { FileLocationTxb.Text = InterfaceUtilities.Singleton.SelectWorkingDirectory(FileLocationTxb.Text); });
            SelectFileBtn.Click += ((object sender, RoutedEventArgs e) => { FileLocationTxb.Text = InterfaceUtilities.Singleton.SelectWorkingFile(FileLocationTxb.Text); });
            ProcessBtn.Click += ((object sender, RoutedEventArgs e) => Process());
            StopBtn.Click += ((object sender, RoutedEventArgs e) => Abort());
            CloseBtn.Click += ((object sender, RoutedEventArgs e) =>
            {
                if (encoder == null || encoder.process == null || EncoderUtilities.Singleton.HasAborted)
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
            });
            SettingsBtn.Click += ((object sender, RoutedEventArgs e) => OpenSettings());
            MinimizeBtn.Click += ((object sender, RoutedEventArgs e) => { WindowState = WindowState.Minimized; });
            SkipBtn.Click += ((object sender, RoutedEventArgs e) =>
            {
                if (encoder != null && encoder.process != null && !encoder.process.HasExited)
                {
                    encoder.Abort(false);
                }
            });

            NotifyIcon.MouseDoubleClick += ((object sender, System.Windows.Forms.MouseEventArgs e) => { WindowState = WindowState.Normal; });

            //Window Events
            MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => DragMove());
            StateChanged += ((object sender, EventArgs e) =>
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
            });

        }

        private void onStartUp()
        {
            if (Values.Singleton.LastUsedMediaDirectory != "")
            {
                FileLocationTxb.Text = Values.Singleton.LastUsedMediaDirectory;
            }
            StopBtn.IsEnabled = false;
            SkipBtn.IsEnabled = false;
            ProcessBtn.IsEnabled = true;
            Values.Singleton.OriginalSize = OriginalSize_Txb;
            Values.Singleton.CurrentSize = CurrentSize_Txb;
            string startTime = DateTime.Now.ToString();
            log.Info("---------------------Log Start---------------------");

        }

        private void Update()
        {
            double seconds = 2;
            long currentTime = DateTime.Now.Ticks, neededTime = 0;
            neededTime = DateTime.Now.AddSeconds(seconds).Ticks;
            Task.Run(() =>
            {
                while (true)
                {
                    currentTime = DateTime.Now.Ticks;
                    if (currentTime >= neededTime)
                    {
                        dis.Invoke(new Action(() =>
                        {
                            UpdateLog();
                            UpdateButtons();
                        }), DispatcherPriority.ContextIdle);
                        neededTime = DateTime.Now.AddSeconds(seconds).Ticks;
                    }
                }
            });
        }

        private void UpdateLog()
        {
            try
            {
                using (StreamReader reader = new StreamReader(Values.Singleton.LogFileLocation))
                {
                    ConsoleOutputTxtBlk.Text = reader.ReadToEnd();
                    ConsoleOutputScrView.ScrollToBottom();
                }
            }
            catch
            {
            }
        }

        void UpdateButtons()
        {
            if (encoder.HasAborted)
            {
                StopBtn.IsEnabled = false;
                SkipBtn.IsEnabled = false;
                ProcessBtn.IsEnabled = true;
            }
            else
            {

                StopBtn.IsEnabled = true;
                SkipBtn.IsEnabled = true;
                ProcessBtn.IsEnabled = false;
            }
        }

        private async void Process()
        {
            encoder.HasAborted = false;
            if (Values.Singleton.IsNetworkPath && Values.Singleton.UseEnclosedFolder)
            {
                log.Warn("No Temp Directory Specified");
                log.Warn("A Temp Directory needs to be specified if the file is on a network path");
                encoder.HasAborted = true;
                return;
            }

            if (!Values.Singleton.UseEnclosedFolder && Values.Singleton.TempFolderLocation.Equals(""))
            {
                log.Warn("No Temp Directory Specified");
                encoder.HasAborted = true;
                return;
            }
            if (FileLocationTxb.Text.ToLower() == "source")
            {
                log.Warn("No Directory Specified");
                encoder.HasAborted = true;
                return;
            }

            MessageBoxButton button = MessageBoxButton.YesNo;
            MessageBoxResult notify = MessageBox.Show("This could take a REALLY long time! It may seem frozen, but its not.\nWould you like to continue?", "Warning", button);
            if (notify == MessageBoxResult.No)
            {
                log.Warn("Process Canceled");
                log.Debug("No Hard Feelings.");
                encoder.HasAborted = true;
                return;
            }
            if (!encoder.HasAborted)
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
                encoder.HasAborted = true;
                ProcessBtn.IsEnabled = true;
                return;
            }
            foreach (MediaFile value in file.Get(SortType.Size, SortOrder.Ascending))
            {
                if (encoder.HasAborted)
                {
                    ProcessBtn.IsEnabled = true;
                    return;
                }
                try
                {
                    await Task.Run(() => encoder.ProcessFileAsync(value.FilePath, ProcessBtn));
                }
                catch (Exception e)
                {
                    log.Error($"Unknown Error Has Occurred while Processing \"{value.FileName}\"", e);
                }
            }
            ProcessBtn.IsEnabled = true;
            WindowState = WindowState.Normal;
        }

        public new void Close()
        {
            WindowState = WindowState.Normal;
            encoder.Abort(false);
            string endTime = DateTime.Now.ToString();
            log.Info("---------------------Log End---------------------" + Environment.NewLine);
            base.Close();
            Environment.Exit(0);
        }

        private void Abort()
        {
            if ((encoder != null && encoder.process != null && !encoder.process.HasExited) || !encoder.ready)
            {
                MessageBoxResult result = MessageBox.Show("Are you sure you want to Abort?\nThis Could corrupt any unprocessed files.", "Warning", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    encoder.Abort(false);
                    encoder.HasAborted = true;
                }
            }
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


        private void SystemTray()
        {
            NotifyIcon = new System.Windows.Forms.NotifyIcon
            {
                Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location)
            };
            contextMenu = new System.Windows.Forms.ContextMenuStrip();
            //contextMenu.Items.Add($"C: {(Values.Singleton.CurrentSizeString == string.Empty ? Values.Singleton.CurrentSizeString : "N/A")} | O: {(Values.Singleton.OriginalSizeString == string.Empty ? Values.Singleton.OriginalSizeString : "N/A")}", null, new EventHandler((object sender, EventArgs args) => { WindowState = WindowState.Normal; }));
            contextMenu.Items.Add("Show", null, new EventHandler((object sender, EventArgs args) => { WindowState = WindowState.Normal; }));
            contextMenu.Items.Add("Settings", null, new EventHandler((object sender, EventArgs args) => { OpenSettings(); }));
            contextMenu.Items.Add("Exit", null, new EventHandler((object sender, EventArgs args) => { Close(); }));
            NotifyIcon.ContextMenuStrip = contextMenu;
        }
    }
}
