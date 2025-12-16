using System;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace Frantic
{
    public sealed partial class PlayerViewPage : Page
    {
        private ObservableCollection<StorageFile> _tracks;
        private int _currentIndex = -1;

        public PlayerViewPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is TrackNavigationData data)
            {
                _tracks = data.Tracks;
                _currentIndex = data.CurrentIndex;
                await LoadTrack();
            }
        }

        private async System.Threading.Tasks.Task LoadTrack()
        {
            if (_currentIndex < 0 || _currentIndex >= _tracks.Count)
                return;

            var file = _tracks[_currentIndex];
            try
            {
                var source = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
                mediaPlayer.Source = source;
                mediaPlayer.MediaPlayer.Play();

                TrackTitle.Text = file.DisplayName;
                TrackArtist.Text = "Локальный трек";

                try
                {
                    var thumb = await file.GetThumbnailAsync(
                        Windows.Storage.FileProperties.ThumbnailMode.MusicView, 256);
                    if (thumb != null && thumb.Size > 0)
                    {
                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(thumb);
                        AlbumArt.Source = bitmap;
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Ошибка: {ex.Message}").ShowAsync();
            }
        }

        private void MediaButton_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as FrameworkElement)?.Tag?.ToString();
            switch (tag)
            {
                case "PlayPause":
                    if (mediaPlayer.MediaPlayer.PlaybackSession.PlaybackState ==
                        Windows.Media.Playback.MediaPlaybackState.Playing)
                    {
                        mediaPlayer.MediaPlayer.Pause();
                        PlayPauseButton.Content = "▶";
                    }
                    else
                    {
                        mediaPlayer.MediaPlayer.Play();
                        PlayPauseButton.Content = "⏸";
                    }
                    break;

                case "Prev":
                    if (_currentIndex > 0)
                    {
                        _currentIndex--;
                        _ = LoadTrack();
                    }
                    break;

                case "Next":
                    if (_currentIndex < _tracks.Count - 1)
                    {
                        _currentIndex++;
                        _ = LoadTrack();
                    }
                    break;
            }
        }

        public class TrackNavigationData
        {
            public ObservableCollection<StorageFile> Tracks { get; set; }
            public int CurrentIndex { get; set; }
        }
    }
}