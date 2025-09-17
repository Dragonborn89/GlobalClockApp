using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace GlobalClockApp
{
    public partial class ClockControl : UserControl
    {
        public string Location { get; set; }
        public List<string> Labels { get; set; }
        public TimeZoneInfo TimeZone { get; set; }
        public Color TimeColor { get; set; } = Colors.Red;
        public Color LabelColor { get; set; } = Colors.Lime;
        public bool Is24HourFormat { get; set; } = true;

        private DispatcherTimer timer;
        private Brush originalBackground;

        public ClockControl()
        {
            InitializeComponent();
            Loaded += ClockControl_Loaded;
            MouseDown += ClockControl_MouseDown;
            AllowDrop = true;
            Drop += ClockControl_Drop;
            DragEnter += ClockControl_DragEnter;
            DragLeave += ClockControl_DragLeave;
            originalBackground = Background;
        }

        private void ClockControl_Loaded(object sender, RoutedEventArgs e)
        {
            UpdateTime();
            timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
            timer.Tick += Timer_Tick;
            timer.Start();
            AddLabels();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            UpdateTime();
        }

        public void UpdateTime()
        {
            var localTime = TimeZoneInfo.ConvertTime(DateTime.Now, TimeZone);
            TimeTextBlock.Text = Is24HourFormat ? localTime.ToString("HH:mm") : localTime.ToString("hh:mm tt");

            var offset = TimeZone.BaseUtcOffset;
            var offsetString = offset < TimeSpan.Zero
                ? $"-{offset:hh\\:mm}"
                : $"+{offset:hh\\:mm}";
            UTCTextBlock.Text = $"UTC{offsetString}";

            DateTextBlock.Text = localTime.ToString("yyyy-MM-dd");
            DSTTextBlock.Text = TimeZone.IsDaylightSavingTime(localTime) ? "DST Active" : "DST Inactive";
        }

        public void AddLabels()
        {
            LabelsPanel.Children.Clear(); // Clear any existing children
            foreach (var label in Labels)
            {
                if (!string.IsNullOrWhiteSpace(label))
                {
                    var labelBorder = new Border
                    {
                        Background = new SolidColorBrush(Colors.SteelBlue),
                        Margin = new Thickness(2),
                        BorderBrush = new SolidColorBrush(Colors.DarkSlateBlue),
                        BorderThickness = new Thickness(1)
                    };

                    var labelTextBlock = new TextBlock
                    {
                        Text = label,
                        Style = (Style)Resources["LabelTextStyle"],
                        Width = 320 // Ensure consistent width
                    };

                    labelBorder.Child = labelTextBlock;
                    LabelsPanel.Children.Add(labelBorder);
                }
            }
        }

        private void ClockControl_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                DragDrop.DoDragDrop(this, this, DragDropEffects.Move);
            }
        }

        private void ClockControl_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ClockControl)))
            {
                var droppedClock = e.Data.GetData(typeof(ClockControl)) as ClockControl;
                var parentPanel = this.Parent as UniformGrid;

                if (parentPanel != null)
                {
                    int removeIndex = parentPanel.Children.IndexOf(droppedClock);
                    int addIndex = parentPanel.Children.IndexOf(this);

                    if (removeIndex != addIndex)
                    {
                        parentPanel.Children.RemoveAt(removeIndex);
                        if (removeIndex < addIndex)
                        {
                            addIndex--; // Adjust index if the item is moved down the list
                        }
                        parentPanel.Children.Insert(addIndex, droppedClock);
                    }
                }
            }
        }

        private void ClockControl_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(ClockControl)))
            {
                Background = Brushes.Gray;
            }
        }

        private void ClockControl_DragLeave(object sender, DragEventArgs e)
        {
            Background = originalBackground;
        }

        public event EventHandler RemoveRequested;
        public event EventHandler MoveUpRequested;
        public event EventHandler MoveDownRequested;
        public event EventHandler EditRequested;

        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            RemoveRequested?.Invoke(this, EventArgs.Empty);
        }

        private void MoveUpButton_Click(object sender, RoutedEventArgs e)
        {
            MoveUpRequested?.Invoke(this, EventArgs.Empty);
        }

        private void MoveDownButton_Click(object sender, RoutedEventArgs e)
        {
            MoveDownRequested?.Invoke(this, EventArgs.Empty);
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            EditRequested?.Invoke(this, EventArgs.Empty);
        }
    }
}
