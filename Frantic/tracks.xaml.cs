using System;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Frantic
{
    public sealed partial class tracks : Page
    {
        private ObservableCollection<StorageFile> _tracks = new ObservableCollection<StorageFile>();
        private int _currentIndex = -1;
        public tracks()
        {
            this.InitializeComponent();
            LoadMusicFiles();

        }

        private async void LoadMusicFiles()
        {
            try
            {
                var musicFolder = KnownFolders.MusicLibrary;
                var queryOptions = new QueryOptions(CommonFileQuery.OrderByTitle, new[]
                {
                    ".mp3", ".wav", ".wma", ".flac", ".m4a"
                });
                var fileQuery = musicFolder.CreateFileQueryWithOptions(queryOptions);
                var files = await fileQuery.GetFilesAsync();
                foreach (var file in files)
                    _tracks.Add(file);

                TracksList.ItemsSource = _tracks;
            }
            catch (Exception ex)
            {
                var dialog = new Windows.UI.Popups.MessageDialog($"Ошибка: {ex.Message}");
                _ = dialog.ShowAsync();
            }
        }

        private void Track_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is StorageFile file)
            {
                _currentIndex = _tracks.IndexOf(file);
                if (_currentIndex >= 0)
                {
                    PlayTrack(file);
                    PlayerPanel.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
            }
        }

        private async void PlayTrack(StorageFile file)
        {
            try
            {
                var source = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
                mediaPlayer.Source = source;
                mediaPlayer.MediaPlayer.Play();

                // Обнови UI
                MiniTrackTitle.Text = file.DisplayName;
                MiniTrackArtist.Text = "Локальный трек";

                // Загрузка обложки
                try
                {
                    var thumb = await file.GetThumbnailAsync(
                        Windows.Storage.FileProperties.ThumbnailMode.MusicView, 256);
                    if (thumb != null && thumb.Size > 0)
                    {
                        var bitmap = new Windows.UI.Xaml.Media.Imaging.BitmapImage();
                        await bitmap.SetSourceAsync(thumb);
                        MiniAlbumArt.Source = bitmap;
                    }
                }
                catch { }
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Ошибка: {ex.Message}").ShowAsync();
            }
        }
        public void Album_ItemClick(object sender, ItemClickEventArgs e)
        {
            var album = e.ClickedItem as Album;
            if (album != null)
            {
                System.Diagnostics.Debug.WriteLine($"Альбом: {album.Title}");
            }
        }

        public void Artist_ItemClick(object sender, ItemClickEventArgs e)
        {
            var artist = e.ClickedItem as Artist;
            if (artist != null)
            {
                System.Diagnostics.Debug.WriteLine($"Исполнитель: {artist.Name}");
            }
        }
        public class Artist
        {
            public string Name { get; set; }
        }

        public class Album
        {
            public string Title { get; set; }
        }
        private void MediaButton_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as FrameworkElement)?.Tag?.ToString();
            switch (tag)
            {
                case "PlayPause":
                    if (mediaPlayer.MediaPlayer.PlaybackSession.PlaybackState == Windows.Media.Playback.MediaPlaybackState.Playing)
                    {
                        mediaPlayer.MediaPlayer.Pause();
                        MiniPlayButton.Content = "▶";
                    }
                    else
                    {
                        mediaPlayer.MediaPlayer.Play();
                        MiniPlayButton.Content = "⏸";
                    }
                    break;

                case "Prev":
                    if (_currentIndex > 0)
                    {
                        _currentIndex--;
                        PlayTrack(_tracks[_currentIndex]);
                    }
                    break;

                case "Next":
                    if (_currentIndex < _tracks.Count - 1)
                    {
                        _currentIndex++;
                        PlayTrack(_tracks[_currentIndex]);
                    }
                    break;
            }
        }
    }
}