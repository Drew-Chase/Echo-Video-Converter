using ChaseLabs.CLUpdate;
using ChaseLabs.Echo.Video_Converter.Resources;
using ChaseLabs.Echo.Video_Converter.Utilities;
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Path = System.IO.Path;

namespace Launcher
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Dispatcher dis = Dispatcher.CurrentDispatcher;
        public MainWindow()
        {
            InitializeComponent();
            RegisterEvents();
            UpdateLauncher();
            Update();
        }

        private void RegisterEvents()
        {
            MouseLeftButtonDown += ((object sender, MouseButtonEventArgs e) => DragMove());
            ExitBtn.Click += ((object sender, RoutedEventArgs e) => Environment.Exit(0));
        }

        private void UpdateLauncher()
        {
            VersionControlUtilities.UpdateLauncher();
        }

        private void Update()
        {
            VersionControlUtilities.UpdateApplication(StatusLabel: status_lbl, IsForced: false, CurrentDispatcher: dis);
        }
    }
}
