using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

//代码重用，以及一些转换类
namespace HereYou {
  class Common {
    public static TodoItemViewModel ViewModel = new TodoItemViewModel();
    public static string selectName = "";
    public static async Task selectPic(Windows.UI.Xaml.Controls.Image pic) {
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
  public class myItem {
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
  public class isEableEdit : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, string language) {
      bool? s = value as bool?;
      if (s == false)
        return true;
      else
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language) {
      throw new NotImplementedException();
    }
  }
  public class isShowLine : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, string language) {
      var s = value as bool?;
      switch (s) {
        case true:
          return 1.0;
        default:
          return 0.0;
      }
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) {
      throw new NotImplementedException();
    }
  }
  public class NullableBooleanToBoolean : IValueConverter {
    public object Convert(object value, Type targetType, object parameter, string language) {
      return (value as bool?) == true;
    }
    public object ConvertBack(object value, Type targetType, object parameter, string language) {
      return new Nullable<bool>((bool)value);
    }
  }
}
