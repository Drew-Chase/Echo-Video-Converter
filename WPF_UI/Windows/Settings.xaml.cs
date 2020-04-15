using ChaseLabs.CLLogger;
using ChaseLabs.CLLogger.Interfaces;
using ChaseLabs.CLUpdate;
using ChaseLabs.Echo.Video_Converter.Resources;
using ChaseLabs.Echo.Video_Converter.Util;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ChaseLabs.Echo.Video_Converter.Windows
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Settings : Window
    {
        public ILog log => LogManger.Init().SetLogDirectory(Values.Singleton.LogFileLocation).EnableDefaultConsoleLogging().SetMinLogType(Lists.LogTypes.All);
        private List<Button> navButtons;

        public Settings()
        {
            InitializeComponent();
            onStartUp();
        }

        private void onStartUp()
        {
            LogLocation_TxtBx.Text = Values.Singleton.LogLocation;
            ConfigLocation_TxtBx.Text = Values.Singleton.ConfigLocation;
            RootLocation_TxtBx.Text = Values.Singleton.RootLocation;
            InstallationLocation_TxtBx.Text = Values.Singleton.InstallationFolder;
            Overwrite_CKBX.IsChecked = Values.Singleton.OverwriteOriginal;
            build_txtblk.Text = Values.Singleton.ApplicationBuildVersion;

            EnclosedFolderOpenBtn.IsEnabled = !Values.Singleton.UseEnclosedFolder;
            TempEncodeFolder_CKBX.IsChecked = Values.Singleton.UseEnclosedFolder;
            UseNvidiaNVENC_CKBX.IsChecked = Values.Singleton.UseNvidiaNVENC;
            EnclosedFolderTxtBox.Text = Values.Singleton.TempFolderLocation;

            EncoderConsole_CKBX.IsChecked = Values.Singleton.ShowEncoderConsole;
            IsNetworkPath_CKBX.IsChecked = Values.Singleton.IsNetworkPath;

            FFMPEG_TxtBx.Text = Values.Singleton.FFMPEGFile;

            navButtons = new List<Button>();
            addNavItems();

            foreach (object item in Enum.GetValues(typeof(SortType)))
            {
                sortType_comboBox.Items.Add(item);
            }
            sortType_comboBox.Foreground = System.Windows.Media.Brushes.Black;

        }

        private void addNavItems()
        {
            foreach (TabItem item in contentTabController.Items)
            {
                Button btn = new Button
                {
                    Name = item.Header.ToString() + "Btn",
                    Content = item.Header.ToString(),
                    Style = Application.Current.Resources["NavButton"] as Style
                };

                btn.Click += nav_button_Clicked;

                navButtons.Add(btn);
                NavigationBar.Children.Add(btn);

                TextBlock tblk = new TextBlock
                {
                    Text = item.Header.ToString(),
                    Name = item.Header.ToString() + "TxtBx",
                    FontSize = 72,
                    Margin = new Thickness
                    {
                        Top = 48,
                        Left = 31,
                        Bottom = 0,
                        Right = 0
                    },
                    VerticalAlignment = VerticalAlignment.Top,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    Width = 72 * item.Header.ToString().Length + 15,
                    Height = 72 + 15,
                };
                Grid grid = item.Content as Grid;
                grid.Children.Add(tblk);
                Grid.SetColumn(tblk, 1);
            }
        }

        private void nav_button_Clicked(object sender, RoutedEventArgs e)
        {
            if (sender.GetType().Equals(typeof(Button)))
            {
                Button btn = (Button)sender;
                foreach (TabItem tab in contentTabController.Items)
                {
                    if (btn.Content.Equals(tab.Header.ToString()))
                    {
                        contentTabController.SelectedItem = tab;
                    }
                }

            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void OpenConfigLocation_Click(object sender, RoutedEventArgs e)
        {
            Util.Utilities.Singleton.OpenFolder(Values.Singleton.ConfigLocation);
        }

        private void OpenLogLocation_Click(object sender, RoutedEventArgs e)
        {
            Util.Utilities.Singleton.OpenFolder(Values.Singleton.LogLocation);
        }

        private void OpenRootLocation_Click(object sender, RoutedEventArgs e)
        {
            Util.Utilities.Singleton.OpenFolder(Values.Singleton.RootLocation);
        }

        private void Overwrite_CKBX_Checked(object sender, RoutedEventArgs e)
        {
            Values.Singleton.OverwriteOriginal = (bool)Overwrite_CKBX.IsChecked;
        }

        private void OpenLogFile_Click(object sender, RoutedEventArgs e)
        {
            Util.Utilities.Singleton.OpenFile(System.IO.Path.Combine(Values.Singleton.LogLocation, "latest.log"));
        }

        private void OpenConfigFile_Click(object sender, RoutedEventArgs e)
        {
            Util.Utilities.Singleton.OpenFile(System.IO.Path.Combine(Values.Singleton.ConfigLocation, Values.Singleton.ConfigFileLocation));
        }

        private void Clean_Logs_Btn_Click(object sender, RoutedEventArgs args)
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

        private void OpenInstallationLocation_Click(object sender, RoutedEventArgs e)
        {
            Util.Utilities.Singleton.OpenFolder(Values.Singleton.InstallationFolder);
        }

        private void UpdateBtn_Click(object sender, RoutedEventArgs e)
        {

            if (File.Exists(Path.Combine(Values.Singleton.ApplicationDirectory, "UpdateLauncher.exe")))
            {
                new Process() { StartInfo = new ProcessStartInfo() { FileName = Path.Combine(Values.Singleton.ApplicationDirectory, "UpdateLauncher.exe"), UseShellExecute = true, CreateNoWindow = true } }.Start();
                Environment.Exit(0);
            }
            if (UpdateManager.Singleton.CheckForUpdate(Values.Singleton.LauncherVersionKey, Values.Singleton.VersionPath, Values.Singleton.RemoteVersionURL))
                log.Info("Update Found");
            else
                log.Info("No Update Found");
        }

        public string InstallationExecFile => System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase;

        private void TempEncodeFolder_CKBX_Click(object sender, RoutedEventArgs e)
        {
            Values.Singleton.UseEnclosedFolder = (bool)TempEncodeFolder_CKBX.IsChecked;
            EnclosedFolderOpenBtn.IsEnabled = !(bool)TempEncodeFolder_CKBX.IsChecked;
        }

        private void EnclosedFolderOpenBtn_Click(object sender, RoutedEventArgs e)
        {
            string dir = string.Empty;
            if (EnclosedFolderTxtBox.Text != string.Empty || EnclosedFolderTxtBox.Text != null)
            {
                try
                {
                    dir = FileUtilities.OpenFolder(EnclosedFolderTxtBox.Text, "Select the temp folder for the media files.");

                }
                catch (Exception)
                {
                    dir = FileUtilities.OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Select the temp folder for the media files.");
                }
            }
            else
            {
                dir = FileUtilities.OpenFolder(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Select the temp folder for the media files.");
            }

            if (dir != string.Empty && dir != "" && dir != " ")
            {
                EnclosedFolderTxtBox.Text = dir;
                log.Debug(@"File location set to " + dir);
                Values.Singleton.TempFolderLocation = dir;
            }
            else
            {
                EnclosedFolderTxtBox.Text = "Source ";
                log.Debug(@"File location was not found");
            }
        }

        private void EncoderConsole_CKBX_Click(object sender, RoutedEventArgs e)
        {
            if (sender.GetType().Equals(typeof(CheckBox)))
            {
                CheckBox check = (CheckBox)sender;
                Values.Singleton.ShowEncoderConsole = (bool)check.IsChecked;
            }
        }

        private void IsNetworkPath_CKBX_Click(object sender, RoutedEventArgs e)
        {
            Values.Singleton.IsNetworkPath = (bool)((CheckBox)sender).IsChecked;
        }



        private void sortType_comboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
        public string ApplicationName => "Muybridge Mass Media Encoder";

        public string ApplicationID => "MME";

        public Assembly ApplicationAssembly => Assembly.GetExecutingAssembly();

        public Uri UpdateXmlLocation => new Uri("");

        public Window Context => this;

        public ImageSource ApplicationIcon => Icon;

        private void UseNvidiaNVENC_CKBX_Checked(object sender, RoutedEventArgs e)
        {
            Values.Singleton.UseNvidiaNVENC = (bool)((CheckBox)sender).IsChecked;
        }
        private void UseHardwareEncoding_CKBX_Checked(object sender, RoutedEventArgs e)
        {
            Values.Singleton.UseNvidiaNVENC = (bool)((CheckBox)sender).IsChecked;
        }

        private void SelectFFMPEGFile_Click(object sender, RoutedEventArgs e)
        {
            Values.Singleton.FFMPEGFile = FileUtilities.OpenFile(FileUtilities.FileExtensionType.Executable, Values.Singleton.FFMPEGDirectory, "Select FFMPEG Executable File");
            FFMPEG_TxtBx.Text = Values.Singleton.FFMPEGFile;
        }
    }
}
