using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace GlobalClockApp
{
    public partial class AddClockDialog : Window
    {
        public AddClockDialog(TimeZoneInfo timeZone = null, List<string> labels = null)
        {
            InitializeComponent();
            TimeZoneComboBox.ItemsSource = TimeZoneInfo.GetSystemTimeZones();
            SelectedTimeZone = timeZone ?? TimeZoneInfo.Local;
            Labels = labels ?? new List<string>();
        }

        public TimeZoneInfo SelectedTimeZone
        {
            get { return TimeZoneComboBox.SelectedItem as TimeZoneInfo; }
            set { TimeZoneComboBox.SelectedItem = value; }
        }

        public List<string> Labels
        {
            get { return LabelsTextBox.Text.Split(',').Select(label => label.Trim()).ToList(); }
            set { LabelsTextBox.Text = string.Join(", ", value); }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (SelectedTimeZone != null)
            {
                TimeZoneComboBox.SelectedItem = SelectedTimeZone;
            }

            if (Labels != null && Labels.Any())
            {
                LabelsTextBox.Text = string.Join(", ", Labels);
            }
        }
    }
}
