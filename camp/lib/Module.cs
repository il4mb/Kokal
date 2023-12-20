using System.Diagnostics;
using System.IO;
using System.Windows;

namespace camp.lib
{

    public delegate void EventListener();
    public delegate void EventListenerReturnable(object obj);

    public class Module
    {

        public event EventListener OnStart;
        public event EventListener OnStop;
        public event EventListener OnToggle;

        public string Name { get; set; }
        public string Version { get; set; }
        public string Path { get; set; }
        public Command Command { get; set; }


        public void Toggle()
        {

            new Thread(async () =>
            {

                Thread.CurrentThread.IsBackground = true;

                string pids = await GetPidAsync();
                if (string.IsNullOrEmpty(pids))
                {
                    Start();
                }
                else
                {
                    Stop();

                }

                Thread.Sleep(3000);
                Application.Current.Dispatcher.InvokeAsync(() => OnToggle?.Invoke());


            }).Start();

        }


        public Task<string> GetPidAsync()
        {

            TaskCompletionSource<string> tcs = new TaskCompletionSource<string>();
            string pid_finder = System.IO.Path.Combine(Path, "apache_pidfinder.bat");

            if (!File.Exists(pid_finder))
            {
                Log.WriteLine($"Cannot run module {Name} because file module_apache.bat not found!!!");
                tcs.SetException(new Exception($"Cannot run module {Name} because file module_apache.bat not found!!!"));
            }


            Process process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = $"/c \"E:\\Camp\\Apache\\apache_pidfinder.bat\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true,
            };


            process.OutputDataReceived += (sender, args) =>
            {
                tcs.TrySetResult(args.Data);

            };

            process.ErrorDataReceived += (sender, args) =>
            {
                tcs.SetException(new Exception(args.Data));
                if (!string.IsNullOrEmpty(args.Data))
                {
                    Debug.WriteLine($"Error: {args.Data}");
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

            return tcs.Task;

        }


        private void Stop()
        {

            Process process = new Process();

            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = "/c \"E:\\Camp\\Apache\\module_apache.bat /stop\"";
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;

            process.OutputDataReceived += async (s, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                {
                    Log.WriteLine(Name, e.Data);

                    if(e.Data.Contains("Successfully"))
                    {
                        Application.Current.Dispatcher.Invoke(() => OnStop?.Invoke());
                    }
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.WaitForExit();

        }


        private void Start()
        {

            try
            {

                Process process = new Process()
                {
                    StartInfo = new()
                    {
                        FileName = "cmd.exe",
                        Arguments = "/c \"E:\\Camp\\Apache\\module_apache.bat /start\"",
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                    },
                    EnableRaisingEvents = true
                };

                process.ErrorDataReceived += (sender, e) =>
                {
                    if (e.Data != null) { Log.WriteLine(Name, $"Error: {e.Data}"); }
                };
                process.OutputDataReceived += async (sender, e) =>
                {

                    Debug.WriteLine(e.Data);

                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Log.WriteLine(Name, e.Data.Trim());

                        string pid = await GetPidAsync();
                        if (!string.IsNullOrEmpty(pid))
                        {
                            Log.WriteLine($"{Name} run sucessful process with id {pid}");
                            Application.Current.Dispatcher.Invoke(() => OnStart?.Invoke());
                        }
                    }
                };

                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                process.WaitForExit();

            }
            catch (Exception ex)
            {
                // Handle exceptions, e.g., command not found, etc.
                MessageBox.Show($"Error executing command: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



    }

    public class Command
    {
        public string? Start { get; set; }
        public string? Stop { get; set; }


    }
}
