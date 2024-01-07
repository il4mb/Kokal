using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.Imaging;

namespace camp.lib
{

    public delegate void ModRunListener(RunInfo? runinfo);

    public abstract class Module : IModule
    {

        /*
         DECLARE ATTRIBUTE
         */
        // protected string Name = "";
        // protected string Version = "";
        private ModulePaths? ModulePaths;
        private IModWatcher? ModWatcher;
        public RunInfo? LastRunInfo { get; set; }


        public Module()
        {
            new Thread(async t =>
            {
                Thread.CurrentThread.IsBackground = true;
                await Task.Delay(600);

                try
                {

                    RunInfo? runInfo = await GetRunInfo();
                    if (runInfo != null && !string.IsNullOrEmpty(runInfo.pid))
                    {
                        Log.WriteLine(GetName(), $"Reload to last instance", Code.Warning);
                        Log.WriteLine(GetName(), $"Process has been started with pid {runInfo.pid}, port {runInfo.port}", Code.Suceess);
                        Application.Current.Dispatcher.Invoke(() => ModWatcher?.OnReloadState(runInfo));
                    }
                }
                catch (Exception e)
                {
                    Log.WriteLine(GetName(), e.Message, Code.Danger);
                }
            }).Start();
        }

        public abstract string GetName();

        public abstract string GetVersion();

        public abstract void AdminNavigate();

        public abstract List<MenuOption>? GetMenuOpts();

        public abstract void OnMenuItemClicked(int code, int position);

        public abstract BitmapImage GetIcon();

        public string GetPath()
        {
            return Path.Combine(Directory.GetCurrentDirectory(), GetName());
        }

        public ModulePaths GetModulePaths()
        {
            if (this.ModulePaths == null)
            {
                this.ModulePaths = new ModulePaths(GetPath());
            }
            return this.ModulePaths;
        }

        public void SetModWacher(IModWatcher modWatcher)
        {
            this.ModWatcher = modWatcher;
        }

        public abstract bool IsComponentOkay();

        public void Toggle()
        {
            Application.Current.Dispatcher.Invoke(() => ModWatcher?.OnToggle());

            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;

                try
                {
                    RunInfo? runinfo = await GetRunInfo();
                    if (runinfo != null && !string.IsNullOrEmpty(runinfo.pid))
                    {
                        Debug.WriteLine("Toggle Stop");
                        Application.Current.Dispatcher.Invoke(() => ModWatcher?.OnStopTrigged()); // Event dispatcher
                        Stop(runinfo);
                    }
                    else
                    {
                        Debug.WriteLine("Toggle Start");
                        Application.Current.Dispatcher?.Invoke(() => ModWatcher?.OnStartTrigged()); // Event dispatcher
                        Start();
                    }

                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }

            }).Start();

            Debug.WriteLine($"Toggle {GetName()}");
        }

        private void Start()
        {
            Debug.WriteLine("Begin start");

            int error = 0;

            new Thread(() =>
            {
                Thread.CurrentThread.IsBackground = true;
                string command = $"/c {this.GetModulePaths().Controller} /start";
                Process proc = new Process();
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.Arguments = command;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;
                proc.StartInfo.CreateNoWindow = true;
                proc.EnableRaisingEvents = true;

                proc.ErrorDataReceived += (s, e) =>
                {
                    error++;
                    Debug.WriteLine("Error : " + e.Data);
                    if (!string.IsNullOrEmpty(e.Data))
                    {
                        Log.WriteLine(GetName(), $"{e.Data}", Code.Danger);
                    }
                };

                proc.Exited += (s, e) =>
                {
                    if (error == 0) { OnProcExited(s, e); }
                    else
                    {
                        Application.Current?.Dispatcher.Invoke(() => ModWatcher?.OnErrorPerfomed());
                    }
                };

                proc.Start();
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
                proc.WaitForExit();

            }).Start();

            new Thread(async () =>
            {
                Thread.CurrentThread.IsBackground = true;

                for (int i = 0; i < 3; i++)
                {
                    await Task.Delay(1500);

                    RunInfo? runInfo = await GetRunInfo();
                    if (runInfo != null && !string.IsNullOrEmpty(runInfo.pid))
                    {

                        Log.WriteLine(GetName(), $"Process has been started with pid {runInfo.pid}, port {runInfo.port}", Code.Suceess);
                        Application.Current.Dispatcher.Invoke(() => ModWatcher?.OnStartPerfomed(runInfo)); // Event dispatcher
                        LastRunInfo = runInfo;

                        string[] pids = [runInfo.pid];
                        if (runInfo.pid.Contains(','))
                        {
                            pids = runInfo.pid.Split(',');
                        }
                        foreach (string pid in pids)
                        {
                            if (!string.IsNullOrEmpty(pid) && int.Parse(pid) > 0)
                            {
                                WatchPidToExit(int.Parse(pid));
                            }
                        }
                        break;
                    }

                }

            }).Start();

        }

        private void Stop(RunInfo runInfo)
        {

            string[] pids = [runInfo.pid];
            if (runInfo.pid.Contains(','))
            {
                pids = runInfo.pid.Split(',');
            }

            foreach (string pid in pids)
            {
                try
                {
                    var proc = Process.GetProcessById(int.Parse(pid));
                    proc.Kill();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            }


            Log.WriteLine(GetName(), $"Process has been stoped", Code.Warning);
            Application.Current.Dispatcher.Invoke(() => ModWatcher?.OnStopPerfomed()); // Event dispatcher

        }

        private void WatchPidToExit(int pid)
        {

            var proc = Process.GetProcessById(pid);
            proc.EnableRaisingEvents = true;
            proc.Exited += OnProcExited;
        }

        private void OnProcExited(object? sender, EventArgs args)
        {
            try
            {
                LastRunInfo = null;
                Log.WriteLine(GetName(), $"Process has been stoped", Code.Warning);
                Application.Current.Dispatcher.Invoke(() => ModWatcher?.OnStopPerfomed()); // Event dispatcher
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error: {ex}");
            }
        }

        public async Task Install()
        {
            try
            {

                Log.WriteLine(GetName(), $"Traying reinstall module...");

                if (!File.Exists(GetModulePaths().Install))
                {
                    throw new Exception($"Cannot find module installer!");
                }

                string command = $"/c {GetModulePaths().Install}";
                Process proc = new()
                {
                    StartInfo = new ProcessStartInfo()
                    {
                        FileName = "cmd.exe",
                        Arguments = command,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        RedirectStandardInput = true,
                    }
                };

                proc.ErrorDataReceived += (s, e) =>
                {
                    throw new Exception(e.Data);
                };


                proc.Start();

                await proc.WaitForExitAsync();

                Log.WriteLine(GetName(), $"Installation module complete!", Code.Suceess);

            }
            catch (Exception ex)
            {
                Log.WriteLine(GetName(), $"Error: {ex.Message}", Code.Danger);
            }
        }

        public async Task KillAsync()
        {

            Debug.WriteLine($"{GetName()} Kill");

            RunInfo? runInfo = await GetRunInfo();
            if (runInfo != null && !string.IsNullOrEmpty(runInfo.pid))
            {
                Stop(runInfo);
            }

        }

        public Task<RunInfo?> GetRunInfo()
        {

            TaskCompletionSource<RunInfo?> tcs = new();
            if (IsCalledFromUIThread())
            {

            }

            Debug.WriteLine("Begin get run info");

            using (var proc = new Process())
            {

                string command = $"/c {this.GetModulePaths().Controller} /info";
                Debug.WriteLine(command);
                proc.StartInfo.FileName = "cmd.exe";
                proc.StartInfo.Arguments = command;
                proc.StartInfo.UseShellExecute = false;
                proc.StartInfo.CreateNoWindow = true;
                proc.StartInfo.RedirectStandardOutput = true;

                proc.Start();
                proc.WaitForExit();

                string output = proc.StandardOutput.ReadToEnd();
                RunInfo? runinfo = new();
                try
                {
                    if (!string.IsNullOrEmpty(output.Trim()))
                    {
                        runinfo = JsonSerializer.Deserialize<RunInfo>(output);

                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
                finally
                {
                    if (!tcs.Task.IsCompleted) tcs.SetResult(runinfo);
                }
            }
            return tcs.Task;
        }

        static bool IsCalledFromUIThread()
        {
            // Check if the current synchronization context is null or from the UI thread
            return SynchronizationContext.Current != null;
        }

    }


    public class MenuOption
    {
        public required string Name;
        public required int Code;
    }

    public class ModulePaths
    {

        public string? Controller { get; set; }
        public string? Info { get; set; }
        public string? Install { get; set; }

        public ModulePaths(string? path)
        {

            if (!string.IsNullOrEmpty(path) && Directory.Exists(path))
            {
                Controller = FindRealPath("*module-controller*.exe", path);
                Info = FindRealPath("*module-info*.exe", path);
                Install = FindRealPath("module-install*.exe", path);
            }
        }

        private string? FindRealPath(string pattern, string directoryPath)
        {
            // Use Directory.GetFiles to get an array of file paths matching the pattern
            string[] matchingFiles = Directory.GetFiles(directoryPath, pattern);

            // Check if any matching files were found
            if (matchingFiles.Length > 0)
            {
                // Return the full path of the first matching file
                return matchingFiles[0];
            }
            else
            {
                // Return null or an empty string if no matching files were found
                return null;
            }
        }

    }

    public class RunInfo
    {
        public string pid { get; set; }
        public string port { get; set; }

    }

    public interface IModWatcher
    {
        void OnToggle();
        void OnStartTrigged();
        void OnStopTrigged();
        void OnStartPerfomed(RunInfo runInfo);
        void OnStopPerfomed();
        void OnReloadState(RunInfo runInfo);

        void OnErrorPerfomed();
    }

    public interface IModule
    {

        public string GetName();

        public string GetVersion();

        public string GetPath();

        public ModulePaths GetModulePaths();

    }


    // public interface 
}
