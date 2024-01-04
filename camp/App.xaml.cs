using camp.lib;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Resources;
using System.Windows;


namespace camp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 

    public partial class App : Application
    {
        #region Constants and Fields

        /// <summary>The event mutex name.</summary>
        private const string UniqueEventName = "CampControlEvt";

        /// <summary>The unique mutex name.</summary>
        private const string UniqueMutexName = "CampControl";

        /// <summary>The event wait handle.</summary>
        private EventWaitHandle eventWaitHandle;

        /// <summary>The mutex.</summary>
        private Mutex mutex;

        #endregion

        #region Methods

        /// <summary>The app on startup.</summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The e.</param>
        private void AppOnStartup(object sender, StartupEventArgs e)
        {
            bool isOwned;
            this.mutex = new Mutex(true, UniqueMutexName, out isOwned);
            this.eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

            // So, R# would not give a warning that this variable is not used.
            GC.KeepAlive(this.mutex);

            if (isOwned)
            {
                // Spawn a thread which will be waiting for our event
                var thread = new Thread(
                    () =>
                    {
                        while (this.eventWaitHandle.WaitOne())
                        {
                            Current.Dispatcher.BeginInvoke(
                                (Action)(() => ((MainWindow)Current.MainWindow).BringToForeground()));
                        }
                    });

                // It is important mark it as background otherwise it will prevent app from exiting.
                thread.IsBackground = true;

                thread.Start();
                return;
            }

            // Notify other instance so it could bring itself to foreground.
            this.eventWaitHandle.Set();

            // Terminate this instance.
            this.Shutdown();
        }



#if false
        private static Mutex _mutex = null;

        public App()
        {
            const string appName = "CampControl";
            bool createdNew;

            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                // Another instance is already running, bring it to the foreground
                BringRunningInstanceToFront();
                Shutdown();
            }
            else
            {
                // Initialize the application
                InitializeComponent();
            }
        }

        private static void BringRunningInstanceToFront()
        {
            Process currentProcess = Process.GetCurrentProcess();
            foreach (Process process in Process.GetProcessesByName(currentProcess.ProcessName))
            {
                if (process.Id != currentProcess.Id)
                {

                    MessageBox.Show("The application is already running. Do you want to bring it to the foreground?", "Application Running");

                    ShowWindow(process.MainWindowHandle, SW_RESTORE);
                    SetForegroundWindow(process.MainWindowHandle);
                    break;
                }
            }
        }

        #region Win32 API
        const int SW_RESTORE = 9;

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SetForegroundWindow(IntPtr hWnd);
        #endregion
#endif

        public static void OpenEdiText(string filename)
        {

            try
            {
                string? editor = Setting.Get(SettingHelper.EDITOR);

                if (!string.IsNullOrEmpty(editor))
                {
                    editor = editor.Trim();
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = editor,
                        Arguments = filename,
                        UseShellExecute = true
                    });
                }
                else
                {
                    // Open the default web browser with the specified URL
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = "notepad",
                        Arguments = filename,
                        UseShellExecute = true,
                    });
                }
            }
            catch (Exception ex)
            {
                Log.WriteLine($"Error: {ex.Message}", Code.Danger);
            }
        }


        public static void OpenBrowser(string url)
        {

            try
            {
                string? browser = Setting.Get(SettingHelper.BROWSER);

                if (!string.IsNullOrEmpty(browser))
                {
                    browser = browser.Trim();
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = browser,
                        Arguments = url,
                        UseShellExecute = true
                    });
                }
                else
                {
                    // Open the default web browser with the specified URL
                    Process.Start(new ProcessStartInfo()
                    {
                        FileName = url,
                        UseShellExecute = true,
                    });
                }
            }
            catch (Exception e)
            {
                Log.WriteLine($"Error: {e.Message}", Code.Danger);
            }
        }


        public static void OpenExplorer(string path)
        {
            try
            {
                if (!Directory.Exists(path))
                {
                    throw new Exception($"Cant open explorer, path not exist {path}");
                }
                Process.Start(new ProcessStartInfo()
                {
                    FileName = "explorer.exe",
                    Arguments = path,
                    UseShellExecute = true
                });

            }
            catch (Exception e)
            {
                Log.WriteLine($"Error: {e.Message}", Code.Danger);

            }
        }


        public static Bitmap? GetImageByName(string imageName)
        {
            Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            string resourceName = asm.GetName().Name + ".Properties.Resources";
            var rm = new ResourceManager(resourceName, asm);
            return rm.GetObject(imageName) as Bitmap;

        }

    }
    #endregion
}