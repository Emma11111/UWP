using System;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Todos {
  class Common {
    public async void selectPic(Image pic) {
      var fop = new FileOpenPicker();
      fop.ViewMode = PickerViewMode.Thumbnail;
      fop.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
      fop.FileTypeFilter.Add(".jpg");
      fop.FileTypeFilter.Add(".jpeg");
      fop.FileTypeFilter.Add(".png");
      fop.FileTypeFilter.Add(".gif");

      Windows.Storage.StorageFile file = await fop.PickSingleFileAsync();
      try {
        using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read)) {
          BitmapImage bitmapImage = new BitmapImage();
          await bitmapImage.SetSourceAsync(fileStream);
          pic.Source = bitmapImage;
        }
      } catch (Exception) {
        return;
      }
    }
  }
}
