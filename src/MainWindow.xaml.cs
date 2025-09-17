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

            // Default fallback values
            double defaultWidth = 800;
            double defaultHeight = 600;
            double defaultTop = 100;
            double defaultLeft = 100;

            // Check if saved values are valid
            bool validSize = Properties.Settings.Default.WindowWidth > 100 &&
                             Properties.Settings.Default.WindowHeight > 100;

            bool validPosition = Properties.Settings.Default.WindowTop >= 0 &&
                                 Properties.Settings.Default.WindowTop < SystemParameters.VirtualScreenHeight &&
                                 Properties.Settings.Default.WindowLeft >= 0 &&
                                 Properties.Settings.Default.WindowLeft < SystemParameters.VirtualScreenWidth;

            if (validSize)
            {
                this.Width = Properties.Settings.Default.WindowWidth;
                this.Height = Properties.Settings.Default.WindowHeight;
            }
            else
            {
                this.Width = defaultWidth;
                this.Height = defaultHeight;
            }

            if (validPosition)
            {
                this.Top = Properties.Settings.Default.WindowTop;
                this.Left = Properties.Settings.Default.WindowLeft;
            }
            else
            {
                this.Top = defaultTop;
                this.Left = defaultLeft;
            }

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
                Margin = new Thickness(5)
            };

            clockControl.RemoveRequested += ClockControl_RemoveRequested;
            clockControl.MoveUpRequested += ClockControl_MoveUpRequested;
            clockControl.MoveDownRequested += ClockControl_MoveDownRequested;
            clockControl.EditRequested += ClockControl_EditRequested;

            // ✅ Wrap clock in a Viewbox for scaling
            var viewbox = new Viewbox
            {
                Stretch = Stretch.Uniform,
                Child = clockControl,
                Width = 350, // You can tweak this
                Height = 180 // Or scale dynamically later
            };

            ClockContainer.Children.Add(viewbox);
        }


        private void ClockControl_RemoveRequested(object sender, EventArgs e)
        {
            if (sender is ClockControl clockControl)
            {
                var viewbox = ClockContainer.Children
                    .OfType<Viewbox>()
                    .FirstOrDefault(v => v.Child == clockControl);

                if (viewbox != null)
                {
                    ClockContainer.Children.Remove(viewbox);
                }
            }
        }

        private void ClockControl_MoveUpRequested(object sender, EventArgs e)
        {
            if (sender is ClockControl clockControl)
            {
                var viewbox = ClockContainer.Children
                    .OfType<Viewbox>()
                    .FirstOrDefault(v => v.Child == clockControl);

                if (viewbox != null)
                {
                    int index = ClockContainer.Children.IndexOf(viewbox);
                    if (index > 0)
                    {
                        ClockContainer.Children.RemoveAt(index);
                        ClockContainer.Children.Insert(index - 1, viewbox);
                    }
                }
            }
        }

        private void ClockControl_MoveDownRequested(object sender, EventArgs e)
        {
            if (sender is ClockControl clockControl)
            {
                var viewbox = ClockContainer.Children
                    .OfType<Viewbox>()
                    .FirstOrDefault(v => v.Child == clockControl);

                if (viewbox != null)
                {
                    int index = ClockContainer.Children.IndexOf(viewbox);
                    if (index < ClockContainer.Children.Count - 1)
                    {
                        ClockContainer.Children.RemoveAt(index);
                        ClockContainer.Children.Insert(index + 1, viewbox);
                    }
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
            foreach (var child in ClockContainer.Children)
            {
                if (child is Viewbox viewbox && viewbox.Child is ClockControl clockControl)
                {
                    clockControl.Is24HourFormat = is24HourFormat;
                }
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
                foreach (var child in ClockContainer.Children)
                {
                    if (child is Viewbox viewbox && viewbox.Child is ClockControl clockControl)
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

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            base.OnClosing(e);

            // Save current window size and position
            Properties.Settings.Default.WindowWidth = this.Width;
            Properties.Settings.Default.WindowHeight = this.Height;
            Properties.Settings.Default.WindowTop = this.Top;
            Properties.Settings.Default.WindowLeft = this.Left;
            Properties.Settings.Default.Save();
        }

    }
}
