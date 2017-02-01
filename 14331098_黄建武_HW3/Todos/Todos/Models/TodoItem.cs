using System;
using System.ComponentModel;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Todos.Models {
  public class TodoItem : INotifyPropertyChanged {
    public event PropertyChangedEventHandler PropertyChanged;
    public TodoItem(DateTime date, ImageSource img, string title = "", string detail = "", bool? finish = false) {
      this.date = date;
      this.img = (img == null ? new BitmapImage(new Uri("ms-appx:///Assets/todo.png")) : img);
      this.title = title;
      this.detail = detail;
      this.finish = finish;
    }
    private void NotifyPropertyChanged(string propertyname) {
      if (PropertyChanged != null)
        PropertyChanged(this, new PropertyChangedEventArgs(propertyname));
    }
    public string title {
      set {
        _title = value;
        NotifyPropertyChanged("title");
      }
      get {
        return _title;
      }
    }
    private string _title;
    private ImageSource _img;
    public string detail { get; set; }
    public DateTime date { get; set; }
    public bool? finish { get; set; }
    public ImageSource img {
      set {
        _img = value;
        NotifyPropertyChanged("img");
      }
      get {
        return _img;
      }
    }

    public void UpdateItem(DateTime date, ImageSource img, string title, string detail) {
      this.date = date;
      this.img = img;
      this.title = title;
      this.detail = detail;
    }
  }
}
