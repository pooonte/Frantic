using System;
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
                    ContentFrame.Navigate(typeof(tracks));
                    break;
                case "video":
                    ContentFrame.Navigate(typeof(video));
                    break;
                case "photo":
                    ContentFrame.Navigate(typeof(photo));
                    break;
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MyPane.IsPaneOpen = !MyPane.IsPaneOpen;
        }
    }
}
