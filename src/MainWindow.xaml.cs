using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Newtonsoft.Json;
using System.Linq;
using GlobalClockApp.Properties;

namespace GlobalClockApp
{
    public partial class MainWindow : Window
    {
        private bool is24HourFormat = false;

        public MainWindow()
        {
            InitializeComponent();

            string lastFilePath = Properties.Settings.Default.LastUsedFilePath;
            if (!string.IsNullOrWhiteSpace(lastFilePath) && File.Exists(lastFilePath))
            {
                LoadClocksFromFile(lastFilePath);
            }
            else
            {
                LoadDefaultClocks();
            }
        }

        private void LoadDefaultClocks()
        {
            AddClock("US - East Coast", new List<string> { "New York, NY", "East Coast" }, TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time"));
            AddClock("US - Central", new List<string> { "Chicago, IL" }, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time"));
        }

        private void AddClock(string location, List<string> labels, TimeZoneInfo timeZone)
        {
            var clockControl = new ClockControl
            {
                Location = location,
                Labels = labels,
                TimeZone = timeZone,
                TimeColor = Colors.Red,
                LabelColor = Colors.SteelBlue,
                Is24HourFormat = is24HourFormat,
                Margin = new Thickness(10) // Add margin for spacing
            };
            clockControl.RemoveRequested += ClockControl_RemoveRequested;
            clockControl.MoveUpRequested += ClockControl_MoveUpRequested;
            clockControl.MoveDownRequested += ClockControl_MoveDownRequested;
            clockControl.EditRequested += ClockControl_EditRequested;
            ClockContainer.Children.Add(clockControl);
            RearrangeClocks();
        }

        private void ClockControl_RemoveRequested(object sender, EventArgs e)
        {
            if (sender is ClockControl clockControl)
            {
                ClockContainer.Children.Remove(clockControl);
                RearrangeClocks();
            }
        }

        private void ClockControl_MoveUpRequested(object sender, EventArgs e)
        {
            if (sender is ClockControl clockControl)
            {
                int index = ClockContainer.Children.IndexOf(clockControl);
                if (index > 0)
                {
                    ClockContainer.Children.RemoveAt(index);
                    ClockContainer.Children.Insert(index - 1, clockControl);
                    RearrangeClocks();
                }
            }
        }

        private void ClockControl_MoveDownRequested(object sender, EventArgs e)
        {
            if (sender is ClockControl clockControl)
            {
                int index = ClockContainer.Children.IndexOf(clockControl);
                if (index < ClockContainer.Children.Count - 1)
                {
                    ClockContainer.Children.RemoveAt(index);
                    ClockContainer.Children.Insert(index + 1, clockControl);
                    RearrangeClocks();
                }
            }
        }

        private void RearrangeClocks()
        {
            int columns = 5;
            int rows = (ClockContainer.Children.Count + columns - 1) / columns;

            ClockContainer.RowDefinitions.Clear();
            ClockContainer.ColumnDefinitions.Clear();

            for (int r = 0; r < rows; r++)
            {
                ClockContainer.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            }
            for (int c = 0; c < columns; c++)
            {
                ClockContainer.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            }

            for (int i = 0; i < ClockContainer.Children.Count; i++)
            {
                var element = ClockContainer.Children[i] as UIElement;
                if (element != null)
                {
                    Grid.SetRow(element, i / columns);
                    Grid.SetColumn(element, i % columns);
                }
            }
        }

        private void AddClockButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new AddClockDialog();
            if (dialog.ShowDialog() == true)
            {
                AddClock(dialog.SelectedTimeZone.DisplayName, dialog.Labels, dialog.SelectedTimeZone);
            }
        }

        private void TimeFormatToggle_Checked(object sender, RoutedEventArgs e)
        {
            is24HourFormat = true;
            UpdateAllClocks();
        }

        private void TimeFormatToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            is24HourFormat = false;
            UpdateAllClocks();
        }

        private void UpdateAllClocks()
        {
            foreach (ClockControl clockControl in ClockContainer.Children)
            {
                clockControl.Is24HourFormat = is24HourFormat;
            }
        }

        private void SaveClocksButton_Click(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = "json"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                var clocks = new List<ClockData>();
                foreach (ClockControl clockControl in ClockContainer.Children)
                {
                    var clockData = new ClockData
                    {
                        Location = clockControl.Location,
                        Labels = clockControl.Labels.ToArray(),
                        TimeZoneId = clockControl.TimeZone.Id,
                        Is24HourFormat = clockControl.Is24HourFormat
                    };
                    clocks.Add(clockData);
                }
                var json = JsonConvert.SerializeObject(clocks, Formatting.Indented);
                File.WriteAllText(saveFileDialog.FileName, json);

                // 🔹 Save the file path to settings
                Properties.Settings.Default.LastUsedFilePath = saveFileDialog.FileName;
                Properties.Settings.Default.Save();
            }
        }

        private void LoadClocksButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new OpenFileDialog
            {
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = "json"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var json = File.ReadAllText(openFileDialog.FileName);
                var clocks = JsonConvert.DeserializeObject<List<ClockData>>(json);

                ClockContainer.Children.Clear();
                foreach (var clockData in clocks)
                {
                    var timeZone = TimeZoneInfo.FindSystemTimeZoneById(clockData.TimeZoneId);
                    AddClock(clockData.Location, clockData.Labels.ToList(), timeZone);
                }

                // ✅ Save the file path so it loads next time
                Properties.Settings.Default.LastUsedFilePath = openFileDialog.FileName;
                Properties.Settings.Default.Save();
            }
        }


        private void LoadClocksFromFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                try
                {
                    var json = File.ReadAllText(filePath);
                    var clocks = JsonConvert.DeserializeObject<List<ClockData>>(json);

                    ClockContainer.Children.Clear();
                    foreach (var clockData in clocks)
                    {
                        var timeZone = TimeZoneInfo.FindSystemTimeZoneById(clockData.TimeZoneId);
                        AddClock(clockData.Location, clockData.Labels.ToList(), timeZone);
                    }

                    is24HourFormat = clocks.FirstOrDefault()?.Is24HourFormat ?? false;
                    UpdateAllClocks();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load saved clocks:\n\n" + ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }


        public class ClockData
        {
            public string Location { get; set; }
            public string[] Labels { get; set; }
            public string TimeZoneId { get; set; }
            public bool Is24HourFormat { get; set; }
        }

        private void ClockControl_EditRequested(object sender, EventArgs e)
        {
            if (sender is ClockControl clockControl)
            {
                var dialog = new AddClockDialog(clockControl.TimeZone, clockControl.Labels);
                if (dialog.ShowDialog() == true)
                {
                    clockControl.TimeZone = dialog.SelectedTimeZone;
                    clockControl.Labels = dialog.Labels;
                    clockControl.UpdateTime();
                    clockControl.AddLabels();
                }
            }
        }
    }
}
