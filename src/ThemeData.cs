using System;

namespace GlobalClockApp
{
    // JSON-serializable theme definition
    public class ThemeData
    {
        public string Name { get; set; } = "Default";

        // Brushes (hex ARGB or RGB, e.g. "#1A1A1A" or "#FF1A1A1A")
        public string WindowBackground { get; set; } = "#1A1A1A";
        public string MenuBackground { get; set; } = "Gray";
        public string MenuForeground { get; set; } = "Black";

        public string TimeText { get; set; } = "#D4EDFF";
        public string TimeBackground { get; set; } = "#1B2230";

        public string LabelText { get; set; } = "#C6D4E2";
        public string LabelBackground { get; set; } = "#212B36";

        public string InfoText { get; set; } = "#E4E8EF";
        public string InfoBackground { get; set; } = "#394355";

        public string ButtonForeground { get; set; } = "#D8E8FF";
        public string ButtonBackground { get; set; } = "#4B5568";

        public string Accent { get; set; } = "#5AA9FF";

        // Fonts
        public string TimeFontFamily { get; set; } = "pack://application:,,,/DS-DIGI.TTF#DS-Digital";
        public string UiFontFamily { get; set; } = "Arial";
    }
}
