using System;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Imaging;

namespace Frantic
{
    public sealed partial class tracks : Page
    {
        private ObservableCollection<StorageFile> _tracks = new ObservableCollection<StorageFile>();
        private int _currentIndex = -1;
        private bool _isFullPlayerOpen = false;

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

        private async void PlayTrack(StorageFile file)
        {
            try
            {
                var source = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
                mediaPlayer.Source = source;
                mediaPlayer.MediaPlayer.Play();

                _currentFile = file;
                MiniTrackTitle.Text = file.DisplayName;
                MiniTrackArtist.Text = "Локальный трек";

                // Загрузка обложки
                try
                {
                    var thumb = await file.GetThumbnailAsync(
                        Windows.Storage.FileProperties.ThumbnailMode.MusicView, 256);
                    if (thumb != null && thumb.Size > 0)
                    {
                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(thumb);
                        MiniAlbumArt.Source = bitmap;
                    }
                }
                catch { }

                MiniPlayerPanel.Visibility = Visibility.Visible;
                UpdatePlayButtonState();
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Ошибка: {ex.Message}").ShowAsync();
            }
        }
        private void OpenPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isFullPlayerOpen)
            {
                // Копируем данные в полноэкранный режим
                FullTrackTitle.Text = MiniTrackTitle.Text;
                FullTrackArtist.Text = MiniTrackArtist.Text;
                FullAlbumArt.Source = MiniAlbumArt.Source;

                FullPlayerPanel.Visibility = Visibility.Visible;
                MiniPlayerPanel.Visibility = Visibility.Collapsed;
                _isFullPlayerOpen = true;
            }
        }
        private StorageFile _currentFile;

        private void Track_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is StorageFile file)
            {
                _currentIndex = _tracks.IndexOf(file);
                if (_currentIndex >= 0)
                {
                    PlayTrack(file);
                }
            }
        }

        private void UpdatePlayButtonState()
        {
            string content = (mediaPlayer.MediaPlayer.PlaybackSession.PlaybackState ==
                Windows.Media.Playback.MediaPlaybackState.Playing) ? "⏸" : "▶";
            MiniPlayButton.Content = content;
            if (_isFullPlayerOpen)
            {
                var fullButton = FullPlayerPanel.FindName("FullPlayButton") as Button;
                if (fullButton != null) fullButton.Content = content;
            }
        }

        private void MiniPlayButton_Click(object sender, RoutedEventArgs e)
        {
            TogglePlayback();
        }

        private void TogglePlayback()
        {
            if (mediaPlayer.MediaPlayer.PlaybackSession.PlaybackState ==
                Windows.Media.Playback.MediaPlaybackState.Playing)
            {
                mediaPlayer.MediaPlayer.Pause();
            }
            else
            {
                mediaPlayer.MediaPlayer.Play();
            }
            UpdatePlayButtonState();
        }

        private void PlayerPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            if (!_isFullPlayerOpen)
            {
                FullTrackTitle.Text = MiniTrackTitle.Text;
                FullTrackArtist.Text = MiniTrackArtist.Text;
                FullAlbumArt.Source = MiniAlbumArt.Source;

                FullPlayerPanel.Visibility = Visibility.Visible;
                MiniPlayerPanel.Visibility = Visibility.Collapsed;
                _isFullPlayerOpen = true;
            }
        }

        private void FullPlayerPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            FullPlayerPanel.Visibility = Visibility.Collapsed;
            MiniPlayerPanel.Visibility = Visibility.Visible;
            _isFullPlayerOpen = false;
        }

        private void MediaButton_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as FrameworkElement)?.Tag?.ToString();
            switch (tag)
            {
                case "PlayPause":
                    TogglePlayback();
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

        public void Album_ItemClick(object sender, ItemClickEventArgs e) { }
        public void Artist_ItemClick(object sender, ItemClickEventArgs e) { }
        public class Artist { public string Name { get; set; } }
        public class Album { public string Title { get; set; } }
    }
}