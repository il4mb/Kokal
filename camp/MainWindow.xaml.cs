using camp.lib;
using camp.ui;
using Notification.Wpf;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;

namespace camp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IModContainer
    {

        private static ModContainer ModContainer;

        public event EventExit Exited;

        public string Camp_Title { get; set; } = "Camp Control Panel";
        public string Camp_Version { get; set; } = "Version 1.0";
        private bool IsKill = false;

        NotificationManager notificationManager;

        public MainWindow()
        {

            InitializeComponent();

            this.DataContext = this;

            Log.Current.SetLogView(new LogView(this.LogContainer));
            Log.WriteLine($"Camp Control Panel {Camp_Version}");

            if (ModContainer == null)
            {
                ModContainer = new(this);
                ModContainer.InitialModule();
            }

            notificationManager = new NotificationManager();
            Loaded += MainWindow_Loaded;

        }


        public Grid GetParentHolder()
        {
            return this.ModulesGrid;
        }


        public Grid? GetTrayHolder()
        {
            return this.TrayModuleLayout;
        }



        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Hidden;
        }

        // Override the OnStateChanged method to minimize to system tray when the window is minimized
        protected override void OnStateChanged(EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
            {
                this.Hide();
            }
            base.OnStateChanged(e);
        }


        // Override OnClosing event to prevent the main window from being closed
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {

            if (!IsKill)
            {
                e.Cancel = true;
                base.OnClosing(e);
                this.Hide(); // Hide instead of closing
                NotifyIcon.Visibility = Visibility.Visible;
                notificationManager.Show("Camp Control Panel", "Enter system tray mode!\nClick here to reopen.", NotificationType.Information, onClick: BringToForeground);
            }
        }

        /// <summary>Brings main window to foreground.</summary>
        public void BringToForeground()
        {
            NotifyIcon.Visibility = Visibility.Hidden;

            if (this.WindowState == WindowState.Minimized || this.Visibility == Visibility.Hidden)
            {
                this.Show();
                this.WindowState = WindowState.Normal;
            }

            // According to some sources these steps gurantee that an app will be brought to foreground.
            this.Activate();
            this.Topmost = true;
            this.Topmost = false;
            this.Focus();

            notificationManager.Show("Camp Control Panel", "The instance is already running, Reloading the previous instance!", NotificationType.Warning);
        }


        private void Button_Setting_Click(object sender, RoutedEventArgs e)
        {

            Window settingWindow = new SettingWindow();
            settingWindow.Owner = this;
            settingWindow.DataContext = this.DataContext;
            settingWindow.ShowDialog();

        }

        private void Button_Netstat_Click(object sender, RoutedEventArgs e)
        {

            Window settingWindow = new NetstatWindow();
            settingWindow.Owner = this;
            settingWindow.DataContext = this.DataContext;
            settingWindow.ShowDialog();

        }

        private void Button_Explorer_Click(object sender, RoutedEventArgs e)
        {
            App.OpenExplorer(Directory.GetCurrentDirectory());
        }

        private void Button_Help_Click(object sender, RoutedEventArgs e)
        {

            Window window = new HelpWindow();
            window.Owner = this;
            window.DataContext = this.DataContext;
            window.ShowDialog();
        }

        private void Button_Quit_Click(object sender, RoutedEventArgs e)
        {

            new Thread(async t =>
            {
                Thread.CurrentThread.IsBackground = true;
                await Exited.Invoke();
                IsKill = true;
                System.Windows.Application.Current.Dispatcher.Invoke(() => System.Windows.Application.Current.Shutdown());
            }).Start();
        }

        private void Button_Expand_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            NotifyIcon.Visibility = Visibility.Hidden;
            NotifyPopup.IsOpen = false;
        }
    }
}