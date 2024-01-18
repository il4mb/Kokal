using kokal.lib;
using kokal.ui;
using Notification.Wpf;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;

namespace kokal
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, IModContainer
    {

        private static ModContainer? ModContainer;

        public event EventExit Exited;

        public string Kokal_Title { get; set; } = "Kokal Control Panel";
        public string Kokal_Version { get; set; } = "Version " + Assembly.GetExecutingAssembly().GetName().Version.ToString();
        public string BuildVersion { get; set; } = "Version " + (Assembly.GetExecutingAssembly().GetName().Version.ToString()) + " [Windows 64bit]";
        private bool shouldClose = false;

        NotificationManager notificationManager;

        public MainWindow()
        {

            InitializeComponent();

            this.DataContext = this;

            Log.Current.SetLogView(new LogView(this.LogContainer));
            Log.WriteLine($"Kokal Control Panel {Kokal_Version}");

            if (ModContainer == null)
            {
                ModContainer = new(this);
                ModContainer.InitialModule();
            }

          //  Application.Current.ShutdownMode = ShutdownMode.OnMainWindowClose; Application.Current.MainWindow = this;

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


        public void InokeExit()
        {
            Exited?.Invoke();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            NotifyIcon.Visibility = Visibility.Hidden;
        }

        // Override the OnStateChanged method to minimize to system tray when the window is minimized



        // Override OnClosing event to prevent the main window from being closed
        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {

            if (!shouldClose)
            {
                e.Cancel = true;
                base.OnClosing(e);
                this.Hide(); // Hide instead of closing
                NotifyIcon.Visibility = Visibility.Visible;
                notificationManager.Show("Kokal Control Panel", (string)FindResource("tray-enter-msg"), NotificationType.Information, onClick: () => BringToForeground(0));
            } else
            {

                Debug.WriteLine("Closing is kill");
                Application.Current.Shutdown();

            }
        }

        /// <summary>Brings main window to foreground.</summary>
        public void BringToForeground(int IsFromNewInst = 1)
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
            if (IsFromNewInst == 1)
            {
                notificationManager.Show("Kokal Control Panel", "The instance is already running, Reloading the previous instance!", NotificationType.Warning);

            }
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

            Task.Run(async () =>
            {
                // Perform any necessary cleanup or asynchronous tasks
                await Exited.Invoke();

                // Set the flag to allow closing
                shouldClose = true;

                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Close the application
                    Application.Current.Shutdown();
                });
            });
        }

        private void Button_Expand_Click(object sender, RoutedEventArgs e)
        {
            this.Show();
            NotifyIcon.Visibility = Visibility.Hidden;
            NotifyPopup.IsOpen = false;
        }
        
        private void Button_Support_Click(object sender, RoutedEventArgs e)
        {
            Window window = new SupportWindow();
            window.Owner = this;
            window.DataContext = this.DataContext;
            window.ShowDialog();
        }
    }
}