using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace kokal
{

    public partial class NetstatWindow : Window
    {
        public NetstatWindow()
        {

            InitializeComponent();
            
            Loaded += Window_Loaded;

        }

        private DispatcherTimer timer;

        private void StartTimer()
        {
            // Create a new timer with a 3-second interval
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(3);

            // Set the timer event handler
            timer.Tick += Timer_Tick;

            // Start the timer
            timer.Start();

            Closed += (s, e) =>
            {
                timer.Stop();
                Mouse.OverrideCursor = Cursors.Arrow;
            };
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            // Set the cursor to the loading cursor before updating the DataGrid
            Mouse.OverrideCursor = Cursors.Wait;

            // Call the method to update the DataGrid
            UpdateDataGrid();

            // Set the cursor back to the default cursor after updating
            Mouse.OverrideCursor = null;
        }

        private void UpdateDataGrid()
        {
            using (Process process = new Process())
            {
                process.StartInfo.FileName = "cmd.exe";
                process.StartInfo.Arguments = "/c netstat -ano";
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.CreateNoWindow = true;

                process.Start();
                string output = process.StandardOutput.ReadToEnd();
                process.WaitForExit();

                string[] lines = output.Trim().Split('\n');

                // Skip the first row if it's a header
                IEnumerable<string> dataRows = lines.Skip(1);

                List<NetworkConnection> connections = new List<NetworkConnection>();

                foreach (string line in dataRows)
                {
                    string[] cells = line.Trim().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

                    if (cells.Length >= 5 && int.TryParse(cells[4], out int pid))
                    {
                        NetworkConnection connection = new NetworkConnection
                        {
                            Protocol = cells[0],
                            LocalAddress = cells[1],
                            ForeignAddress = cells[2],
                            State = cells[3],
                            PID = pid
                        };

                        connections.Add(connection);
                    }
                }

                // Update the ItemsSource for the DataGrid
                networkDataGrid.ItemsSource = connections;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Wait;
            // Start the timer when the window is loaded
            StartTimer();
        }

    }


    public class NetworkConnection
    {
        public string Protocol { get; set; }
        public string LocalAddress { get; set; }
        public string ForeignAddress { get; set; }
        public string State { get; set; }
        public int PID { get; set; }
    }

}
