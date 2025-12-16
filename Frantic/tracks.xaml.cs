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
        private StorageFile _currentFile;
        private bool _isFullPlayerOpen = false;
        private bool _userIsSeeking = false;

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
            if (file == null) return;
            try
            {
                var source = Windows.Media.Core.MediaSource.CreateFromStorageFile(file);
                mediaPlayer.Source = source;
                mediaPlayer.MediaPlayer.Play();

                _currentFile = file;
                MiniTrackTitle.Text = file.DisplayName;
                MiniTrackArtist.Text = "Локальный трек";
                FullTrackTitle.Text = file.DisplayName;
                FullTrackArtist.Text = "Локальный трек";

                try
                {
                    var props = await file.Properties.RetrievePropertiesAsync(
                        new[] { "System.Music.Title", "System.Music.Artist" });
                    if (props["System.Music.Title"] is string title && !string.IsNullOrWhiteSpace(title))
                        MiniTrackTitle.Text = FullTrackTitle.Text = title;
                    if (props["System.Music.Artist"] is string artist && !string.IsNullOrWhiteSpace(artist))
                        MiniTrackArtist.Text = FullTrackArtist.Text = artist;
                }
                catch { }

                try
                {
                    var thumb = await file.GetThumbnailAsync(
                        Windows.Storage.FileProperties.ThumbnailMode.MusicView, 256);
                    if (thumb != null && thumb.Size > 0)
                    {
                        var bitmap = new BitmapImage();
                        await bitmap.SetSourceAsync(thumb);
                        MiniAlbumArt.Source = FullAlbumArt.Source = bitmap;
                    }
                }
                catch { }

                MiniPlayerPanel.Visibility = Visibility.Visible;
                UpdatePlayButtonState();
                StartPositionTimer();
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Ошибка: {ex.Message}").ShowAsync();
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
                }
            }
        }

        private void UpdatePlayButtonState()
        {
            string content = (mediaPlayer.MediaPlayer.PlaybackSession.PlaybackState ==
                Windows.Media.Playback.MediaPlaybackState.Playing) ? "⏸" : "▶";
            MiniPlayButton.Content = FullPlayButton.Content = content;
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

        private void MiniPlayButton_Click(object sender, RoutedEventArgs e)
        {
            TogglePlayback();
        }

        private void OpenPlayerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isFullPlayerOpen)
            {
                FullPlayerPanel.Visibility = Visibility.Visible;
                MiniPlayerPanel.Visibility = Visibility.Collapsed;
                _isFullPlayerOpen = true;
            }
        }

        private void FullPlayerPanel_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var originalSource = e.OriginalSource as FrameworkElement;
            if (originalSource == null) return;

            string name = originalSource.Name;
            if (name != "FullPlayButton" &&
                name != "ProgressSlider" &&
                !(originalSource is Button) &&
                !(originalSource is Slider))
            {
                FullPlayerPanel.Visibility = Visibility.Collapsed;
                MiniPlayerPanel.Visibility = Visibility.Visible;
                _isFullPlayerOpen = false;
            }
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

        private DispatcherTimer _positionTimer;

        private void StartPositionTimer()
        {
            _positionTimer?.Stop();
            _positionTimer = new DispatcherTimer();
            _positionTimer.Interval = TimeSpan.FromMilliseconds(500);
            _positionTimer.Tick += (s, e) =>
            {
                var session = mediaPlayer.MediaPlayer.PlaybackSession;
                if (session?.NaturalDuration > TimeSpan.Zero)
                {
                    ProgressSlider.Maximum = session.NaturalDuration.TotalSeconds;
                    TotalTimeText.Text = FormatTime(session.NaturalDuration);
                    if (!_userIsSeeking)
                    {
                        ProgressSlider.Value = session.Position.TotalSeconds;
                        CurrentTimeText.Text = FormatTime(session.Position);
                    }
                }
            };
            _positionTimer.Start();
        }

        private string FormatTime(TimeSpan t) => $"{(int)t.TotalMinutes}:{t.Seconds:D2}";

        private void ProgressSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (_userIsSeeking) return;
            var session = mediaPlayer.MediaPlayer.PlaybackSession;
            if (session != null && session.NaturalDuration > TimeSpan.Zero)
            {
                session.Position = TimeSpan.FromSeconds(e.NewValue);
            }
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            FullPlayerPanel.Visibility = Visibility.Collapsed;
            MiniPlayerPanel.Visibility = Visibility.Visible;
            _isFullPlayerOpen = false;
        }

        public void Album_ItemClick(object sender, ItemClickEventArgs e) { }
        public void Artist_ItemClick(object sender, ItemClickEventArgs e) { }
        public class Artist { public string Name { get; set; } }
        public class Album { public string Title { get; set; } }
    }
}