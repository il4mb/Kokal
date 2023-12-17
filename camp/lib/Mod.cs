using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace camp.lib
{

    public delegate void OnPerformStart(Process process);

    internal class Mod
    {

        public event OnPerformStart OnStart;

        public string Name { get; }
        public string Path { get; }

        public string Command = "";
        public Process Process { get; }


        public Mod(string name, string path)
        {

            Name = name;
            Path = path;

            Process = new()
            {
                StartInfo = new ()
                {
                    FileName = "cmd.exe", // Use the command prompt
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true
                },
                EnableRaisingEvents = true
            };



            Process.OutputDataReceived += (sender, e) =>
            {
                Log.WriteLine(Name, e.Data != null ? e.Data : "");
            };

            Process.Exited += (sender, e) =>
            {
                Log.WriteLine(Name, "Background process exited.");
            };

            
        }


        public void Run() {

            try
            {

                Process.Start();
                Process.BeginOutputReadLine();
                Log.WriteLine(Name, "Begin To Start");

            }
            catch (Exception ex)
            {
                // Handle exceptions, e.g., command not found, etc.
                MessageBox.Show($"Error executing command: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
