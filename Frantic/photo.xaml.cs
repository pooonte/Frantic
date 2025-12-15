using System;
using System.Collections.ObjectModel;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Frantic
{
    public sealed partial class photo : Page
    {
        private ObservableCollection<PhotoItem> _photos = new ObservableCollection<PhotoItem>();

        public photo()
        {
            this.InitializeComponent();
            LoadPhotoFiles();
        }

        private async void LoadPhotoFiles()
        {
            try
            {
                var picturesFolder = KnownFolders.PicturesLibrary;
                var queryOptions = new QueryOptions(CommonFileQuery.OrderByDate, new[]
                {
                    ".jpg", ".jpeg", ".png", ".gif", ".bmp"
                });
                var fileQuery = picturesFolder.CreateFileQueryWithOptions(queryOptions);
                var files = await fileQuery.GetFilesAsync();

                foreach (var file in files)
                {
                    var thumb = await file.GetThumbnailAsync(
                        Windows.Storage.FileProperties.ThumbnailMode.PicturesView,
                        150);

                    var bitmap = new BitmapImage();
                    if (thumb != null && thumb.Size > 0)
                    {
                        await bitmap.SetSourceAsync(thumb);
                    }

                    _photos.Add(new PhotoItem { File = file, Thumbnail = bitmap });
                }

                PhotoList.ItemsSource = _photos;
            }
            catch (Exception ex)
            {
                var dialog = new Windows.UI.Popups.MessageDialog($"Ошибка: {ex.Message}");
                _ = dialog.ShowAsync();
            }
        }

        private void GalleryPhoto_Click(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is PhotoItem item)
            {
                Frame.Navigate(typeof(PhotoViewPage), item.File);
            }
        }
    }

    public class PhotoItem
    {
        public StorageFile File { get; set; }
        public BitmapImage Thumbnail { get; set; }
    }
}