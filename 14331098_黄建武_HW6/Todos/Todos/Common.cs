using System;
using System.Diagnostics;
using System.Threading.Tasks;
using SQLitePCL;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace Todos {
  class Common {
    public static TodoItemViewModel ViewModel = new TodoItemViewModel();
    public static string selectName = "";
    public static async Task selectPic(Image pic) {
      var fop = new FileOpenPicker();
      fop.ViewMode = PickerViewMode.Thumbnail;
      fop.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
      fop.FileTypeFilter.Add(".jpg");
      fop.FileTypeFilter.Add(".jpeg");
      fop.FileTypeFilter.Add(".png");
      fop.FileTypeFilter.Add(".gif");

      StorageFile file = await fop.PickSingleFileAsync();
      try {
        IRandomAccessStream fileStream;
        using (fileStream = await file.OpenAsync(FileAccessMode.Read)) {
          BitmapImage bitmapImage = new BitmapImage();
          await bitmapImage.SetSourceAsync(fileStream);
          pic.Source = bitmapImage;

          var name = file.Path.Substring(file.Path.LastIndexOf('\\') + 1);
          selectName = name;

          await file.CopyAsync(ApplicationData.Current.LocalFolder, name, NameCollisionOption.ReplaceExisting);
        }
      } catch (Exception e) {
        Debug.WriteLine(e.Message);
        return;
      }
    }
  }

  class myItem {
    public long ID;
    public DateTimeOffset date;
    public string imgname;
    public string title;
    public string detail;
    public bool? finish;

    public myItem(long ID, DateTimeOffset date, string imgname, string title, string detail, bool? finish) {
      this.ID = ID;
      this.date = date;
      this.imgname = imgname;
      this.title = title;
      this.detail = detail;
      this.finish = finish;
    }
  }

  public class myConverter : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, string language) {
      string s = value as string;
      switch (s) {
        case "Create":
          return Visibility.Collapsed;
        case "Update":
          return Visibility.Visible;
        default:
          return Visibility.Collapsed;
      }
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) {
      throw new NotImplementedException();
    }
  }
}