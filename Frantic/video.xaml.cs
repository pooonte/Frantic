using System;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Frantic
{
    public sealed partial class video : Page
    {
        private ObservableCollection<VideoItem> _videos = new ObservableCollection<VideoItem>();

        public video()
        {
            this.InitializeComponent();
            LoadVideoFiles();
        }

        private async void LoadVideoFiles()
        {

            try
            {
                var videosFolder = KnownFolders.VideosLibrary;
                var queryOptions = new QueryOptions(CommonFileQuery.OrderByDate, new[]
                {
                    ".mp4", ".avi", ".mkv", ".wmv", ".mov", ".flv", ".webm"
                });
                var fileQuery = videosFolder.CreateFileQueryWithOptions(queryOptions);
                var files = await fileQuery.GetFilesAsync();

                foreach (var file in files)
                {
                    // Пропускаем недоступные файлы (OneDrive и т.д.)
                    try
                    {
                        using (var stream = await file.OpenReadAsync()) { }
                    }
                    catch
                    {
                        continue;
                    }

                    // Получаем миниатюру
                    var thumb = await file.GetThumbnailAsync(
                        Windows.Storage.FileProperties.ThumbnailMode.VideosView,
                        150);

                    var bitmap = new BitmapImage();
                    if (thumb != null && thumb.Size > 0)
                    {
                        await bitmap.SetSourceAsync(thumb);
                    }

                    _videos.Add(new VideoItem { File = file, Thumbnail = bitmap });
                }

                VideoList.ItemsSource = _videos;
            }
            catch (Exception ex)
            {
                var dialog = new Windows.UI.Popups.MessageDialog($"Ошибка: {ex.Message}");
                _ = dialog.ShowAsync();
            }
        }

        private void GalleryVideo_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is VideoItem item)
            {
                Frame.Navigate(typeof(VideoViewPage), item.File);
            }
        }
    }

    public class VideoItem
    {
        public StorageFile File { get; set; }
        public BitmapImage Thumbnail { get; set; }
    }
}