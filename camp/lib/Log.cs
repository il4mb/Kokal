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
        private readonly string FilePath;
        private readonly string FileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";


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
        }


        public void SetLogView(LogView logView)
        {
            this.view = logView;
        }

        private void Write(string instance, string message, Code code)
        {

            using StreamWriter sw = new StreamWriter(FilePath, true);
            DateTime dateTime = DateTime.Now;
            // Write the text to the file
            sw.WriteLine($"[{code}]\t[{dateTime}]\t[{instance}]\t{message}");
            sw.Dispose();

            view?.WriteToRichEditText(new LogObject()
            {
                DateTime = dateTime,
                Code = code,
                Instance = instance,
                Message = message,
            });

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

            Application.Current.Dispatcher.Invoke(() =>
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
                    scrollViewer.ScrollToVerticalOffset(scrollViewer.ScrollableHeight+100);
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
