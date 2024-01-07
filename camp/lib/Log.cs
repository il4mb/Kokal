using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace camp.lib
{

    public enum Code
    {

        None = 0,
        Suceess,
        Warning,
        Danger
    }

    public class Log
    {

        public FileSystemEventHandler FileSystemEventHandler;
        public static readonly Log Current = new();
        private LogView view;

        private readonly string path;
        // private string FilePath { get; private set; }
        private readonly string FileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";

        private readonly Queue<LogEntry> logQueue;
        private readonly object queueLock = new object();
        private readonly AutoResetEvent queueSignal = new AutoResetEvent(false);
        private bool isWriting;
        private bool isExit = false;

        public string FilePath { get; private set; }


        public static void WriteLine(string message)
        {

            WriteLine("System", message);
        }

        public static void WriteLine(string instance, string message)
        {
            WriteLine(instance, message, Code.None);
        }

        public static void WriteLine(string message, Code code)
        {
            WriteLine("System", message, code);
        }


        public static void WriteLine(string instance, string message, Code code)
        {
            if (string.IsNullOrEmpty(message)) return;
            Current.Write(instance, message, code);

        }


        private Log()
        {

            path = Path.Combine(Directory.GetCurrentDirectory(), "Log");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            FilePath = Path.Combine(path, FileName);
            if (!File.Exists(FilePath))
            {
                File.Create(FilePath).Close();
            }

            logQueue = new Queue<LogEntry>();
            isWriting = false;

            // Start a background thread to write to the file
            Thread writerThread = new Thread(WriteToLogFile);
            writerThread.IsBackground = true; // Make it a background thread
            writerThread.Start();

            Application.Current.Exit += (s, e) =>
            {
                isExit = true;
                queueSignal.Set(); // Signal the thread to wake up and check the exit condition
                writerThread.Join(); // Wait for the thread to finish before the application exits
            };

        }


        public void SetLogView(LogView logView)
        {
            this.view = logView;
        }

        private void Write(string instance, string message, Code code)
        {

            DateTime dateTime = DateTime.Now;
            lock (queueLock)
            {
                logQueue.Enqueue(new LogEntry(dateTime, instance, message, code));
                // Signal the writer thread to process the queue
                queueSignal.Set();
            }

            view?.WriteToRichEditText(new LogObject()
            {
                DateTime = dateTime,
                Code = code,
                Instance = instance,
                Message = message,
            });

        }

        private void WriteToLogFile()
        {

            while (!isExit)
            {
                queueSignal.WaitOne(); // Wait for a signal that the queue has items

                Debug.WriteLine("Is Exit : " + isExit);

                lock (queueLock)
                {
                    if (logQueue.Count > 0)
                    {
                        isWriting = true;
                        try
                        {
                            using (StreamWriter sw = new StreamWriter(FilePath, true))
                            {
                                while (logQueue.Count > 0)
                                {
                                    LogEntry logEntry = logQueue.Dequeue();

                                    sw.WriteLine($"[{logEntry.Code}]\t[{logEntry.Datetime}]\t[{logEntry.Instance}]\t{logEntry.Message}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error writing to log file: {ex.Message}");
                        }
                        finally
                        {
                            isWriting = false;
                        }
                    }
                }
            }
        }

        private class LogEntry
        {
            public DateTime Datetime { get; }
            public string Instance { get; }
            public string Message { get; }
            public Code Code { get; }

            public LogEntry(DateTime datetime, string instance, string message, Code code)
            {
                Datetime = datetime;
                Instance = instance;
                Message = message;
                Code = code;
            }

        }
    }



    public class LogObject
    {
        public required DateTime DateTime { get; set; }
        public required Code Code { get; set; }
        public required string Instance { get; set; }
        public required string Message { get; set; }
    }


    public class LogView(RichTextBox Container)
    {

        private static LogObject OldData;
        public void WriteToRichEditText(LogObject log)
        {

            if (OldData != null && OldData.Instance == log.Instance && OldData.Code == log.Code && OldData.Message == log.Message)
            {
                OldData = log;
                return;

            }

            Application.Current?.Dispatcher.Invoke(() =>
            {

                if (Container == null) return;

                OldData = log;

                // Create a new Paragraph
                Paragraph paragraph = new Paragraph();
                paragraph.LineHeight = 1;

                // Create a Run for the DateTime with a specific format
                Run dateTimeRun = new Run(log.DateTime.ToString("HH:mm:ss") + "\t   ");
                dateTimeRun.Foreground = Brushes.Gray; // Optionally set the color
                paragraph.Inlines.Add(dateTimeRun);

                // Create a Run for the Name
                Run nameRun = new Run("[" + log.Instance + "]\t");
                nameRun.Foreground = Brushes.Blue; // Optionally set the color
                paragraph.Inlines.Add(nameRun);

                // Create a TextBlock to contain the message without line breaks
                TextBlock textBlock = new TextBlock();
                textBlock.Focusable = true;
                textBlock.TextWrapping = TextWrapping.Wrap;

                // Create a Run for the Message
                Run messageRun = new Run(log.Message);
                messageRun.Foreground = GetColorByCode(log.Code);

                textBlock.Inlines.Add(messageRun);

                // Set BaselineAlignment to Top for the non-editable runs
                dateTimeRun.BaselineAlignment = BaselineAlignment.Top;
                nameRun.BaselineAlignment = BaselineAlignment.Top;

                // Add the non-editable Runs and the TextBlock to the Paragraph
                paragraph.Inlines.Add(dateTimeRun);
                paragraph.Inlines.Add(nameRun);
                paragraph.Inlines.Add(new InlineUIContainer(textBlock));

                // Add the Paragraph to the RichTextBox
                Container.Document.Blocks.Add(paragraph);


                ScrollToBottom(Container);
            });

        }


        private SolidColorBrush GetColorByCode(Code code)
        {
            return code switch
            {
                Code.Suceess => Brushes.DarkGreen,
                Code.Danger => Brushes.Red,
                Code.Warning => Brushes.Orange,
                _ => Brushes.Black
            };
        }


        private void ScrollToBottom(RichTextBox richTextBox)
        {
            if (richTextBox != null)
            {
                // Get the ScrollViewer from the RichTextBox
                var scrollViewer = FindVisualChild<ScrollViewer>(richTextBox);

                if (scrollViewer != null)
                {
                    // Scroll to the bottom
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight + 100);
                }
            }
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);

                if (child != null && child is T)
                {
                    return (T)child;
                }
                else
                {
                    T childOfChild = FindVisualChild<T>(child);
                    if (childOfChild != null)
                    {
                        return childOfChild;
                    }
                }
            }

            return null;
        }

    }
}
