using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;
using System.Linq;


namespace GlobalClockApp
{
    public static class ThemeManager
    {
        // Resource keys used in XAML via {DynamicResource ...}
        public const string BrushWindowBackground = "Brush.WindowBackground";
        public const string BrushMenuBackground = "Brush.MenuBackground";
        public const string BrushMenuForeground = "Brush.MenuForeground";
        public const string BrushTimeText = "Brush.TimeText";
        public const string BrushTimeBackground = "Brush.TimeBackground";
        public const string BrushLabelText = "Brush.LabelText";
        public const string BrushLabelBackground = "Brush.LabelBackground";
        public const string BrushInfoText = "Brush.InfoText";
        public const string BrushInfoBackground = "Brush.InfoBackground";
        public const string BrushButtonForeground = "Brush.ButtonForeground";
        public const string BrushButtonBackground = "Brush.ButtonBackground";
        public const string BrushAccent = "Brush.Accent";

        public const string FontTime = "Font.Time";
        public const string FontUI = "Font.UI";

        /// <summary>
        /// Gets the Themes folder next to the exe.
        /// Ensures the folder exists.
        /// </summary>
        public static string ThemesFolder
        {
            get
            {
                string exeDir = AppDomain.CurrentDomain.BaseDirectory;
                string themesDir = Path.Combine(exeDir, "Themes");
                if (!Directory.Exists(themesDir))
                    Directory.CreateDirectory(themesDir);
                return themesDir;
            }
        }

        public static void Apply(ThemeData t)
        {
            var r = Application.Current.Resources;

            r[BrushWindowBackground] = ToBrush(t.WindowBackground);
            r[BrushMenuBackground] = ToBrush(t.MenuBackground);
            r[BrushMenuForeground] = ToBrush(t.MenuForeground);

            r[BrushTimeText] = ToBrush(t.TimeText);
            r[BrushTimeBackground] = ToBrush(t.TimeBackground);

            r[BrushLabelText] = ToBrush(t.LabelText);
            r[BrushLabelBackground] = ToBrush(t.LabelBackground);

            r[BrushInfoText] = ToBrush(t.InfoText);
            r[BrushInfoBackground] = ToBrush(t.InfoBackground);

            r[BrushButtonForeground] = ToBrush(t.ButtonForeground);
            r[BrushButtonBackground] = ToBrush(t.ButtonBackground);

            r[BrushAccent] = ToBrush(t.Accent);

            // Debug logging for font family values
            System.Diagnostics.Debug.WriteLine($"[ThemeManager] TimeFontFamily (from JSON): {t.TimeFontFamily}");
            System.Diagnostics.Debug.WriteLine($"[ThemeManager] UiFontFamily (from JSON): {t.UiFontFamily}");

            // Try to load Time font
            r[FontTime] = ResolveFontFamily(
                t.TimeFontFamily,
                "pack://application:,,,/DS-DIGI.TTF#DS-Digital"
            );

            // Try to load UI font
            r[FontUI] = ResolveFontFamily(
                t.UiFontFamily,
                "Arial"
            );
        }

        /// <summary>
        /// Helper that tries to resolve a FontFamily from a string, 
        /// and falls back if it fails.
        /// </summary>
        private static FontFamily ResolveFontFamily(string fontString, string fallback)
        {
            // If incoming string is empty, use the fallback
            if (string.IsNullOrWhiteSpace(fontString))
                fontString = fallback;

            try
            {
                if (fontString.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                {
                    // Normalize to 2-arg ctor form: baseUri + "./<path-or-file>#<face>"
                    const string root = "pack://application:,,,/";
                    var baseUri = new Uri(root, UriKind.Absolute);

                    // Strip the root so we have "<file[;component/...].ttf>#<face>"
                    var relative = fontString.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                        ? fontString.Substring(root.Length)
                        : fontString;

                    if (!relative.StartsWith("./"))
                        relative = "./" + relative;

                    // Force WPF to actually load the family from the TTF (prevents silent Arial fallback)
                    var fileOnly = relative.Substring(2).Split('#')[0]; // e.g., "DS-DIGI.TTF" or "Fonts/DS-DIGI.TTF"
                    _ = Fonts.GetFontFamilies(baseUri, "./" + fileOnly).FirstOrDefault();

                    var ff = new FontFamily(baseUri, relative);
                    System.Diagnostics.Debug.WriteLine($"[ThemeManager] (PACK) FontFamily resolved to: {ff.Source}");
                    return ff;
                }
                else
                {
                    // System-installed font name (e.g., "Arial")
                    var ff = new FontFamily(fontString);
                    System.Diagnostics.Debug.WriteLine($"[ThemeManager] (SYS ) FontFamily resolved to: {ff.Source}");
                    return ff;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[ThemeManager] Failed to load '{fontString}': {ex.Message}. Falling back to '{fallback}'.");
                try
                {
                    // Last-chance fallback (also handles pack fallback correctly)
                    if (fallback.StartsWith("pack://", StringComparison.OrdinalIgnoreCase))
                    {
                        const string root = "pack://application:,,,/";
                        var baseUri = new Uri(root, UriKind.Absolute);
                        var rel = fallback.StartsWith(root, StringComparison.OrdinalIgnoreCase)
                            ? "./" + fallback.Substring(root.Length)
                            : fallback.StartsWith("./") ? fallback : "./" + fallback;

                        return new FontFamily(baseUri, rel);
                    }
                    return new FontFamily(fallback);
                }
                catch
                {
                    return new FontFamily("Arial");
                }
            }
        }



        public static ThemeData Load(string path)
        {
            var json = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<ThemeData>(json) ?? new ThemeData();
        }

        public static void Save(ThemeData t, string path)
        {
            var json = JsonConvert.SerializeObject(t, Formatting.Indented);
            File.WriteAllText(path, json);
        }

        // Build a ThemeData from current resources (for "Save Current Theme...")
        public static ThemeData FromCurrent(string name = "Current")
        {
            var r = Application.Current.Resources;
            return new ThemeData
            {
                Name = name,
                WindowBackground = FromBrush(r[BrushWindowBackground]),
                MenuBackground = FromBrush(r[BrushMenuBackground]),
                MenuForeground = FromBrush(r[BrushMenuForeground]),
                TimeText = FromBrush(r[BrushTimeText]),
                TimeBackground = FromBrush(r[BrushTimeBackground]),
                LabelText = FromBrush(r[BrushLabelText]),
                LabelBackground = FromBrush(r[BrushLabelBackground]),
                InfoText = FromBrush(r[BrushInfoText]),
                InfoBackground = FromBrush(r[BrushInfoBackground]),
                ButtonForeground = FromBrush(r[BrushButtonForeground]),
                ButtonBackground = FromBrush(r[BrushButtonBackground]),
                Accent = FromBrush(r[BrushAccent]),

                // ✅ Preserve full pack URI if it starts with "pack://"
                TimeFontFamily = r[FontTime] is FontFamily ft && ft.Source.StartsWith("pack://")
                    ? ft.Source
                    : ((FontFamily)r[FontTime]).Source,

                UiFontFamily = r[FontUI] is FontFamily fu && fu.Source.StartsWith("pack://")
                    ? fu.Source
                    : ((FontFamily)r[FontUI]).Source
            };
        }

        private static Brush ToBrush(string hex) =>
            (SolidColorBrush)(new BrushConverter().ConvertFromString(hex));

        private static string FromBrush(object brushObj)
        {
            if (brushObj is SolidColorBrush b)
                return $"#{b.Color.A:X2}{b.Color.R:X2}{b.Color.G:X2}{b.Color.B:X2}";
            return "#FF000000";
        }
    }
}
