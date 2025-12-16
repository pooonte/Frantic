using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Frantic
{
    public sealed partial class SettingsPage : Page
    {
        // Твои цвета
        private static readonly string[] ColorHexes = {
            "#008299", "#2672EC", "#8C0095", "#5133AB",
            "#AC193D", "#D24726", "#008A00", "#094AB2",
            "#00A0B1", "#2E8DEF", "#A700AE", "#643EBF",
            "#BF1E4B", "#DC572E", "#00A600", "#0A5BC4"
        };

        private static readonly string[] ColorNames = {
            "DarkTeal", "DarkBlue", "DarkPurple1", "DDPurple",
            "DarkRed", "DarkOrange", "DarkGreen", "DSBlue",
            "Teal", "Blue", "Purple", "DarkPurple2",
            "Red", "Orange", "Green", "SkyBlue"
        };

        public SettingsPage()
        {
            this.InitializeComponent();
            CreateColorButtons();
        }

        private void CreateColorButtons()
        {
            ColorsContainer.Children.Clear();
            ColorsContainer.Children.Add(new TextBlock
            {
                Text = "Цвета панели",
                FontSize = 18,
                Foreground = new SolidColorBrush(Colors.White),
                Margin = new Thickness(16, 16, 0, 16)
            });

            for (int i = 0; i < ColorHexes.Length; i++)
            {
                var color = HexToColor(ColorHexes[i]);
                var button = new Button
                {
                    Content = ColorNames[i],
                    Background = new SolidColorBrush(color),
                    Foreground = new SolidColorBrush(Colors.White),
                    Height = 48,
                    Margin = new Thickness(8, 4, 8, 4),
                    BorderThickness = new Thickness(0)
                };
                button.Click += (s, e) => ApplyNavPanelColor(color);
                ColorsContainer.Children.Add(button);
            }
        }

        private void OpenColorPanelButton_Click(object sender, RoutedEventArgs e)
        {
            ColorPanel.IsPaneOpen = !ColorPanel.IsPaneOpen;
        }

        private void ApplyNavPanelColor(Color color)
        {
            if (Window.Current.Content is Frame frame && frame.Content is MainPage mainPage)
            {
                mainPage.SetNavPanelColor(color);
            }

            if (ColorPanel.Pane is StackPanel pane)
            {
                pane.Background = new SolidColorBrush(color);
            }

            ColorPanel.IsPaneOpen = false;
        }

        private static Color HexToColor(string hex)
        {
            hex = hex.Replace("#", "");
            byte r = Convert.ToByte(hex.Substring(0, 2), 16);
            byte g = Convert.ToByte(hex.Substring(2, 2), 16);
            byte b = Convert.ToByte(hex.Substring(4, 2), 16);
            return Color.FromArgb(255, r, g, b);
        }
    }
}