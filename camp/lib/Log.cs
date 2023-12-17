using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace camp.lib
{
    internal class Log
    {

        private RichTextBox? RichTextBox { get; set; }
        private static Log? Instance;

        public static Log GetInstance()
        {
            if (Instance == null) Instance = new Log();

            return Instance;
        }

        public static void SetTextBox(RichTextBox textBox)
        {
            GetInstance().RichTextBox = textBox;
        }

        public static void WriteLine(string message)
        {
            GetInstance().AddFormattedParagraph(DateTime.Now, "System", message);
        }

        public static void WriteLine(string name, string message)
        {
            GetInstance().AddFormattedParagraph(DateTime.Now, name, message);
        }


        private void AddFormattedParagraph(DateTime dateTime, string name, string message)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {

                if (RichTextBox == null) return;
                // Create a new Paragraph
                Paragraph paragraph = new Paragraph();
                paragraph.LineHeight = 1;

                // Create a Run for the DateTime with a specific format
                Run dateTimeRun = new Run(dateTime.ToString("HH:mm:ss") + "\t   ");
                dateTimeRun.Foreground = System.Windows.Media.Brushes.Gray; // Optionally set the color
                paragraph.Inlines.Add(dateTimeRun);

                // Create a Run for the Name
                Run nameRun = new Run("[" + name + "]\t");
                nameRun.Foreground = System.Windows.Media.Brushes.Blue; // Optionally set the color
                paragraph.Inlines.Add(nameRun);

                // Create a Run for the Message
                Run messageRun = new Run(message);
                paragraph.Inlines.Add(messageRun);

                // Add the Paragraph to the RichTextBox
                RichTextBox.Document.Blocks.Add(paragraph);
            });
        
        }
    }
}
