using System;
using System.Windows;

namespace Kapture
{
    /// <summary>
    /// Interaction logic for NewRecordingPrompt.xaml
    /// </summary>
    public partial class NewRecordingPrompt : Window
    {
        public int delay { get; set; }
        public int Duration { get; set; }

        public NewRecordingPrompt()
        {
            InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            this.Duration = Int32.Parse(this.DurationBox.Text);
            this.DialogResult = true;
            this.Close();
        }
    }
}
