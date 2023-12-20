using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;

namespace camp.lib
{
    internal class Log
    {

        private Grid? ContainerGrid { get; set; }
        private RichTextBox ContainerTextBox;
        private static Log? Instance;

        public static Log GetInstance()
        {
            Instance ??= new Log();

            return Instance;
        }

        public static void SetGridContainer(Grid grid)
        {
            GetInstance().ContainerGrid = grid;
            Application.Current.Dispatcher.Invoke(() =>
            {
                grid.ColumnDefinitions.Clear();
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition());
                grid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = new GridLength(250, GridUnitType.Star)
                });
            });
        }

        public static void SetContainerTextBox(RichTextBox ritcTexBox)
        {
            GetInstance().ContainerTextBox = ritcTexBox;
        }

        public static void WriteLine(string message)
        {
            GetInstance().AddLine(DateTime.Now, "System", message);
        }

        public static void WriteLine(string name, string message)
        {
            GetInstance().AddLine(DateTime.Now, name, message);
        }

        private void AddLine(DateTime dateTime, string name, string message)
        {

            if(ContainerTextBox != null) {
                WriteToRichEditText(dateTime, name, message);
            } else if (ContainerGrid != null) { 
                WriteToGridColumn(dateTime, name, message);
            }

            Debug.WriteLine("LOG : " + message);
        }

        private void WriteToGridColumn(DateTime dateTime, string name, string message)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                int fontSize = 13;

                Label dateTimeLabel = new()
                {
                    Content = dateTime.ToString("HH:mm:ss"),
                    FontSize = fontSize,
                    Foreground =Brushes.Gray
                };

                Label nameLabel = new()
                {
                    Content = $"[{name}]",
                    FontSize = fontSize,
                    Foreground = Brushes.Blue
                };

                Label messageLabel = new()
                {
                    Content = message,
                    FontSize = fontSize,

                };


                ContainerGrid.Children.Add(dateTimeLabel);
                ContainerGrid.Children.Add(nameLabel);
                ContainerGrid.Children.Add(messageLabel);
                Grid.SetColumn(dateTimeLabel, 0);
                Grid.SetColumn(nameLabel, 1);
                Grid.SetColumn(messageLabel, 2);
                Grid.SetRow(dateTimeLabel, ContainerGrid.RowDefinitions.Count);
                Grid.SetRow(nameLabel, ContainerGrid.RowDefinitions.Count);
                Grid.SetRow(messageLabel, ContainerGrid.RowDefinitions.Count);

                ContainerGrid.RowDefinitions.Add(new RowDefinition());
            });
        }

        [Obsolete]
        private void WriteToRichEditText(DateTime dateTime, string name, string message)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {

                if (ContainerTextBox == null) return;
                // Create a new Paragraph
                Paragraph paragraph = new Paragraph();
                paragraph.LineHeight = 1;

                // Create a Run for the DateTime with a specific format
                Run dateTimeRun = new Run(dateTime.ToString("HH:mm:ss") + "\t   ");
                dateTimeRun.Foreground = Brushes.Gray; // Optionally set the color
                paragraph.Inlines.Add(dateTimeRun);

                // Create a Run for the Name
                Run nameRun = new Run("[" + name + "]\t");
                nameRun.Foreground = System.Windows.Media.Brushes.Blue; // Optionally set the color
                paragraph.Inlines.Add(nameRun);

                // Create a Run for the Message
                Run messageRun = new Run(message);
                paragraph.Inlines.Add(messageRun);

                // Add the Paragraph to the RichTextBox
                ContainerTextBox.Document.Blocks.Add(paragraph);
            });


        }

        [Obsolete]
        private double MeasureStringWidth(string text, RichTextBox richTextBox)
        {
            var formattedText = new FormattedText(
                text,
                System.Globalization.CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(richTextBox.FontFamily, richTextBox.FontStyle, richTextBox.FontWeight, richTextBox.FontStretch),
                richTextBox.FontSize,
                Brushes.Black);

            return formattedText.Width;
        }
    }
}
