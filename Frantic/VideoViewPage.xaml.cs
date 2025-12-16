using System;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Frantic
{
    public sealed partial class VideoViewPage : Page
    {
        public VideoViewPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is StorageFile file)
            {
                try
                {
                    var stream = await file.OpenReadAsync();
                    VideoPlayer.SetSource(stream, file.ContentType);
                    VideoPlayer.Play();
                }
                catch (Exception ex)
                {
                    var dialog = new Windows.UI.Popups.MessageDialog("Не удалось воспроизвести видео: " + ex.Message);
                    await dialog.ShowAsync();
                }
            }
        }
    }
}