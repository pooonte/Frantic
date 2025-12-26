using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace Frantic
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            ContentFrame.Navigate(typeof(home));
        }

        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            string tag = (sender as Button)?.Tag?.ToString();
            switch (tag)
            {
                case "tracks":
                    ContentFrame.Navigate(typeof(MainLibraryPage));
                    break;
                case "video":
                    ContentFrame.Navigate(typeof(video));
                    break;
                case "photo":
                    ContentFrame.Navigate(typeof(photo));
                    break;
                case "settings":
                    ContentFrame.Navigate(typeof(SettingsPage));
                    break;
            }
            MyPane.IsPaneOpen = false;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyPane.IsPaneOpen = !MyPane.IsPaneOpen;
        }
        public void SetNavPanelColor(Windows.UI.Color color)
        {
            if (MyPane.Pane is StackPanel pane)
            {
                pane.Background = new SolidColorBrush(color);

                foreach (var child in pane.Children)
                {
                    if (child is TextBlock tb)
                    {
                        tb.Foreground = new SolidColorBrush(Colors.White);
                    }
                    else if (child is Button btn)
                    {
                        btn.Foreground = new SolidColorBrush(Colors.White);
                    }
                }
            }
        }
    }
}
