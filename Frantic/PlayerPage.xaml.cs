using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Media.Playback;

namespace Frantic
{
    public sealed partial class PlayerPage : Page
    {
        private DispatcherTimer _positionTimer;
        private bool _userIsSeeking = false;

        public PlayerPage()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Используем единый MediaPlayer
            mediaPlayer.SetMediaPlayer(MediaPlayerSingleton.Player);

            // Загружаем информацию о текущем треке
            LoadCurrentTrackInfo();

            // Подписываемся на события
            MediaPlayerSingleton.Player.PlaybackSession.PlaybackStateChanged += PlaybackSession_PlaybackStateChanged;

            StartPositionTimer();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            // Отписываемся от событий
            MediaPlayerSingleton.Player.PlaybackSession.PlaybackStateChanged -= PlaybackSession_PlaybackStateChanged;

            _positionTimer?.Stop();
        }

        private async void LoadCurrentTrackInfo()
        {
            var file = MediaPlayerSingleton.CurrentFile;
            if (file == null) return;

            try
            {
                FullTrackTitle.Text = file.DisplayName;
                FullTrackArtist.Text = "Локальный трек";

                try
                {
                    var props = await file.Properties.RetrievePropertiesAsync(
                        new[] { "System.Music.Title", "System.Music.Artist" });
                    if (props["System.Music.Title"] is string title && !string.IsNullOrWhiteSpace(title))
                        FullTrackTitle.Text = title;
                    if (props["System.Music.Artist"] is string artist && !string.IsNullOrWhiteSpace(artist))
                        FullTrackArtist.Text = artist;
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
                        FullAlbumArt.Source = bitmap;
                    }
                }
                catch { }

                UpdatePlayButtonState();
            }
            catch (Exception ex)
            {
                await new Windows.UI.Popups.MessageDialog($"Ошибка: {ex.Message}").ShowAsync();
            }
        }

        private void UpdatePlayButtonState()
        {
            string content = MediaPlayerSingleton.IsPlaying ? "⏸" : "▶";
            PlayPauseButton.Content = content;
        }

        private void PlaybackSession_PlaybackStateChanged(MediaPlaybackSession sender, object args)
        {
            Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                UpdatePlayButtonState();
            });
        }

        private void StartPositionTimer()
        {
            _positionTimer?.Stop();
            _positionTimer = new DispatcherTimer();
            _positionTimer.Interval = TimeSpan.FromMilliseconds(500);
            _positionTimer.Tick += (s, e) =>
            {
                var session = MediaPlayerSingleton.Player.PlaybackSession;
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

        private void PlayPauseButton_Click(object sender, RoutedEventArgs e)
        {
            MediaPlayerSingleton.TogglePlayPause();
        }

        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            // Здесь можно добавить логику переключения на предыдущий трек
            // Для этого нужно будет передавать плейлист между страницами
        }

        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            // Здесь можно добавить логику переключения на следующий трек
        }

        private void ProgressSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        {
            if (_userIsSeeking) return;
            var session = MediaPlayerSingleton.Player.PlaybackSession;
            if (session != null && session.NaturalDuration > TimeSpan.Zero)
            {
                session.Position = TimeSpan.FromSeconds(e.NewValue);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}