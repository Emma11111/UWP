using System;
using System.ComponentModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

//待办事项
namespace HereYou {
  public class TodoItem : INotifyPropertyChanged {
    public TodoItem(long ID, DateTime date, string imgname = "", string title = "", string detail = "", bool? finish = false) {
      this.ID = ID;
      this.date = date;
      this.imgname = imgname;
      this.title = title;
      this.detail = detail;
      this.finish = finish;
      setImg();
    }

    //设置图片
    public async void setImg() {
      if (imgname == "") {
        this.img = new BitmapImage(new Uri("ms-appx:///Assets/todo.png"));
      } else {
        var file = await ApplicationData.Current.LocalFolder.GetFileAsync(imgname);
        IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
        BitmapImage bitmapImage = new BitmapImage();
        await bitmapImage.SetSourceAsync(fileStream);
        this.img = bitmapImage;
      }
    }
    public event PropertyChangedEventHandler PropertyChanged;
    private void NotifyPropertyChanged(string propertyname) {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
    }

    public string detail { get; set; }
    public DateTime date { get; set; }
    public long ID { get; set; }

    private bool? _finish { get; set;}
    public bool? finish {
      set {
        _finish = value;
        NotifyPropertyChanged("finish");
      }
      get {
        return _finish;
      }
    }
    private string _imgname;
    public string imgname {
      set {
        _imgname = value;
        NotifyPropertyChanged("imgname");
        setImg();
      }
      get {
        return _imgname;
      }
    }
    private string _title;
    public string title {
      set {
        _title = value;
        NotifyPropertyChanged("title");
      }
      get {
        return _title;
      }
    }
    private ImageSource _img;
    public ImageSource img {
      set {
        _img = value;
        NotifyPropertyChanged("img");
      }
      get {
        return _img;
      }
    }
  }
}
