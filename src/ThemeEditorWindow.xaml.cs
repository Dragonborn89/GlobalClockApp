using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using WinForms = System.Windows.Forms;

namespace GlobalClockApp
{
    public partial class ThemeEditorWindow : Window
    {
        private ThemeData _current;
        private List<FontOption> _timeFonts;
        private List<FontOption> _uiFonts;

        public ThemeEditorWindow()
        {
            InitializeComponent();

            try
            {
                _current = ThemeManager.FromCurrent("Working");
            }
            catch
            {
                // fallback if ThemeManager fails
                _current = new ThemeData { Name = "New Theme" };
            }

            // Seed UI with values
            LoadFieldsFromTheme(_current);

            // Build font lists
            _timeFonts = BuildTimeFontOptions();
            _uiFonts = BuildUiFontOptions();

            CmbTimeFont.ItemsSource = _timeFonts;
            CmbUiFont.ItemsSource = _uiFonts;

            SelectComboItem(CmbTimeFont, _current.TimeFontFamily);
            SelectComboItem(CmbUiFont, _current.UiFontFamily);

            PaintAllSwatches();
        }

        // ——— Events ———

        private void AnyColor_TextChanged(object sender, TextChangedEventArgs e) =>
            UpdateThemeFromFields(applyNow: true);

        private void CmbTimeFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbTimeFont.SelectedItem is FontOption opt)
            {
                _current.TimeFontFamily = opt.Value;
                ApplyAndRefresh();
            }
        }

        private void CmbUiFont_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUiFont.SelectedItem is FontOption opt)
            {
                _current.UiFontFamily = opt.Value;
                ApplyAndRefresh();
            }
        }

        private void BtnLoadTheme_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog { Filter = "Theme Files (*.json)|*.json|All Files (*.*)|*.*" };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    var t = ThemeManager.Load(dlg.FileName);
                    _current = t;
                    LoadFieldsFromTheme(_current);
                    SelectComboItem(CmbTimeFont, _current.TimeFontFamily);
                    SelectComboItem(CmbUiFont, _current.UiFontFamily);
                    PaintAllSwatches();
                    ApplyAndRefresh();

                    Properties.Settings.Default.LastUsedTheme = dlg.FileName;
                    Properties.Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to load theme:\n\n" + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnSaveTheme_Click(object sender, RoutedEventArgs e)
        {
            UpdateThemeFromFields(applyNow: false);

            var dlg = new SaveFileDialog
            {
                Filter = "Theme Files (*.json)|*.json|All Files (*.*)|*.*",
                DefaultExt = "json",
                FileName = $"{_current.Name}.json"
            };
            if (dlg.ShowDialog() == true)
            {
                try
                {
                    ThemeManager.Save(_current, dlg.FileName);
                    Properties.Settings.Default.LastUsedTheme = dlg.FileName;
                    Properties.Settings.Default.Save();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Failed to save theme:\n\n" + ex.Message,
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void BtnClose_Click(object sender, RoutedEventArgs e) => Close();

        // ——— Helpers ———

        private void PickColor(TextBox targetBox)
        {
            var dlg = new System.Windows.Forms.ColorDialog();
            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var c = dlg.Color;
                targetBox.Text = $"#{c.A:X2}{c.R:X2}{c.G:X2}{c.B:X2}";
            }
        }

        private void PickColor_WindowBackground(object sender, RoutedEventArgs e) => PickColor(TxtWindowBackground);
        private void PickColor_MenuBackground(object sender, RoutedEventArgs e) => PickColor(TxtMenuBackground);
        private void PickColor_MenuForeground(object sender, RoutedEventArgs e) => PickColor(TxtMenuForeground);
        private void PickColor_TimeText(object sender, RoutedEventArgs e) => PickColor(TxtTimeText);
        private void PickColor_TimeBackground(object sender, RoutedEventArgs e) => PickColor(TxtTimeBackground);
        private void PickColor_LabelText(object sender, RoutedEventArgs e) => PickColor(TxtLabelText);
        private void PickColor_LabelBackground(object sender, RoutedEventArgs e) => PickColor(TxtLabelBackground);
        private void PickColor_InfoText(object sender, RoutedEventArgs e) => PickColor(TxtInfoText);
        private void PickColor_InfoBackground(object sender, RoutedEventArgs e) => PickColor(TxtInfoBackground);
        private void PickColor_ButtonForeground(object sender, RoutedEventArgs e) => PickColor(TxtButtonForeground);
        private void PickColor_ButtonBackground(object sender, RoutedEventArgs e) => PickColor(TxtButtonBackground);
        private void PickColor_Accent(object sender, RoutedEventArgs e) => PickColor(TxtAccent);

        private void ApplyAndRefresh()
        {
            ThemeManager.Apply(_current);
            Application.Current.Windows.OfType<MainWindow>().FirstOrDefault()?.RefreshThemeBindings();
            PaintAllSwatches();
        }

        private void LoadFieldsFromTheme(ThemeData t)
        {
            TxtWindowBackground.Text = t.WindowBackground;
            TxtMenuBackground.Text = t.MenuBackground;
            TxtMenuForeground.Text = t.MenuForeground;

            TxtTimeText.Text = t.TimeText;
            TxtTimeBackground.Text = t.TimeBackground;

            TxtLabelText.Text = t.LabelText;
            TxtLabelBackground.Text = t.LabelBackground;

            TxtInfoText.Text = t.InfoText;
            TxtInfoBackground.Text = t.InfoBackground;

            TxtButtonForeground.Text = t.ButtonForeground;
            TxtButtonBackground.Text = t.ButtonBackground;

            TxtAccent.Text = t.Accent;
        }

        private void UpdateThemeFromFields(bool applyNow)
        {
            if (_current == null)
                return; // just ignore until _current is initialized

            string V(string s) => s?.Trim();

            _current.WindowBackground = V(TxtWindowBackground.Text);
            _current.MenuBackground = V(TxtMenuBackground.Text);
            _current.MenuForeground = V(TxtMenuForeground.Text);

            _current.TimeText = V(TxtTimeText.Text);
            _current.TimeBackground = V(TxtTimeBackground.Text);

            _current.LabelText = V(TxtLabelText.Text);
            _current.LabelBackground = V(TxtLabelBackground.Text);

            _current.InfoText = V(TxtInfoText.Text);
            _current.InfoBackground = V(TxtInfoBackground.Text);

            _current.ButtonForeground = V(TxtButtonForeground.Text);
            _current.ButtonBackground = V(TxtButtonBackground.Text);

            _current.Accent = V(TxtAccent.Text);

            if (applyNow)
                ApplyAndRefresh();
        }


        private void PaintAllSwatches()
        {
            Paint(SwatchWindowBackground, TxtWindowBackground.Text);
            Paint(SwatchMenuBackground, TxtMenuBackground.Text);
            Paint(SwatchMenuForeground, TxtMenuForeground.Text);
            Paint(SwatchTimeText, TxtTimeText.Text);
            Paint(SwatchTimeBackground, TxtTimeBackground.Text);
            Paint(SwatchLabelText, TxtLabelText.Text);
            Paint(SwatchLabelBackground, TxtLabelBackground.Text);
            Paint(SwatchInfoText, TxtInfoText.Text);
            Paint(SwatchInfoBackground, TxtInfoBackground.Text);
            Paint(SwatchButtonForeground, TxtButtonForeground.Text);
            Paint(SwatchButtonBackground, TxtButtonBackground.Text);
            Paint(SwatchAccent, TxtAccent.Text);
        }

        private void Paint(System.Windows.Shapes.Rectangle r, string hex)
        {
            var b = TryToBrush(hex);
            if (b != null) r.Fill = b;
        }

        private Brush TryToBrush(string hex)
        {
            try { return (SolidColorBrush)(new BrushConverter().ConvertFromString(hex)); }
            catch { return null; }
        }

        private void SelectComboItem(ComboBox combo, string value)
        {
            var match = (combo.ItemsSource as IEnumerable<FontOption>)?
                .FirstOrDefault(f => string.Equals(f.Value, value, StringComparison.OrdinalIgnoreCase));
            if (match != null) combo.SelectedItem = match;
        }

        private List<FontOption> BuildTimeFontOptions()
        {
            var list = new List<FontOption>
            {
                new FontOption("DS-Digital (pack)", "pack://application:,,,/DS-DIGI.TTF#DS-Digital"),
                new FontOption("Segoe UI", "Segoe UI"),
                new FontOption("Arial", "Arial"),
                new FontOption("Consolas", "Consolas"),
                new FontOption("Courier New", "Courier New")
            };

            foreach (var ff in Fonts.SystemFontFamilies)
            {
                var name = ff.Source;
                if (!list.Any(x => x.Value.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    list.Add(new FontOption(name, name));
            }
            return list.OrderBy(x => x.Display).ToList();
        }

        private List<FontOption> BuildUiFontOptions()
        {
            var list = new List<FontOption>
            {
                new FontOption("Segoe UI", "Segoe UI"),
                new FontOption("Arial", "Arial"),
                new FontOption("DS-Digital (pack)", "pack://application:,,,/DS-DIGI.TTF#DS-Digital"),
                new FontOption("Consolas", "Consolas"),
                new FontOption("Courier New", "Courier New")
            };

            foreach (var ff in Fonts.SystemFontFamilies)
            {
                var name = ff.Source;
                if (!list.Any(x => x.Value.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    list.Add(new FontOption(name, name));
            }
            return list.OrderBy(x => x.Display).ToList();
        }
    }

    // FontOption class (not record, works everywhere)
    public class FontOption
    {
        public string Display { get; }
        public string Value { get; }
        public FontOption(string display, string value)
        {
            Display = display;
            Value = value;
        }
        public override string ToString() => Display;
    }
}
