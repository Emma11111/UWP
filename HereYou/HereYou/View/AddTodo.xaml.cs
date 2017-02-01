using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;
using Windows.UI;
using Windows.Storage;
using Newtonsoft.Json;
using Windows.Storage.Streams;
using System.Collections.Generic;
using System.Diagnostics;
using SQLitePCL;

namespace HereYou {
  /// <summary>
  /// 可用于自身或导航至 Frame 内部的空白页。
  /// </summary>
  public sealed partial class AddTodo {
    private TodoItemViewModel ViewModel = Common.ViewModel;
    private string selectName = Common.selectName;
    private SQLiteConnection conn = App.conn;
    public AddTodo() {
      InitializeComponent();
      var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
      titleBar.ForegroundColor = Colors.White;
      titleBar.BackgroundColor = Colors.DodgerBlue;
      titleBar.ButtonBackgroundColor = Colors.DodgerBlue;
    }

    //选择事项图片
    private async void selectPic(object sender, RoutedEventArgs e) {
      await Common.selectPic(pic);
    }

    //新建或更新事项
    private async void Create_Update_Click(object sender, RoutedEventArgs e) {
      string alert = "";
      ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
      if (title.Text.Trim() == "")
        alert += "标题不能为空！\n";
      if (range.Text.Trim() == "")
        alert += "详情不能为空！\n";
      if (ddl.Date.Date.CompareTo(DateTime.Today) < 0)
        alert += "Deadline已经过去！";

      if (alert != "")
        await new MessageDialog(alert).ShowAsync();
      else if (create_update.Content.ToString() == "新 建") {
        try {
          string sql = @"INSERT INTO Todo (date, imgname, title, detail, finish) VALUES (?,?,?,?,?)";
          using (var res = conn.Prepare(sql)) {
            res.Bind(1, ddl.Date.Date.ToString());
            res.Bind(2, Common.selectName);
            res.Bind(3, title.Text.Trim());
            res.Bind(4, range.Text.Trim());
            res.Bind(5, "false");
            res.Step();
            ViewModel.AddItem(conn.LastInsertRowId(), ddl.Date.DateTime, Common.selectName, title.Text.Trim(), range.Text.Trim(), false);
            Common.selectName = "";
            Cancle_Click(null, null);
            ViewModel.SelectedItem = null;
            await new MessageDialog("新建成功！").ShowAsync();
            Frame.Navigate(typeof(Todo));
          }
        } catch (Exception ex) {
          Debug.WriteLine(ex.Message);
          throw;
        }
      } else {
        try {
          string sql = @"UPDATE Todo SET date = ?, imgname = ?, title = ?, detail = ? WHERE ID = ?";
          using (var res = conn.Prepare(sql)) {
            res.Bind(1, ddl.Date.Date.ToString());
            res.Bind(2, Common.selectName);
            res.Bind(3, title.Text.Trim());
            res.Bind(4, range.Text.Trim());
            res.Bind(5, ViewModel.SelectedItem.ID);
            res.Step();
            ViewModel.UpdateItem(ddl.Date.DateTime, Common.selectName, title.Text.Trim(), range.Text.Trim());
            await new MessageDialog("更新成功！").ShowAsync();
            Frame.Navigate(typeof(Todo));
          }
        } catch (Exception ex) {
          Debug.WriteLine(ex.Message);
          throw;
        }
      }
    }

    //取消编辑，新建则清空输入，更新则回到编辑前
    private void Cancle_Click(object sender, RoutedEventArgs e) {
      if (create_update.Content.ToString() == "更 新") {
        title.Text = ViewModel.SelectedItem.title;
        ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
        range.Text = ViewModel.SelectedItem.detail;
        ddl.Date = ViewModel.SelectedItem.date;
        pic.Source = ViewModel.SelectedItem.img;
        Common.selectName = ViewModel.SelectedItem.imgname;
      } else {
        title.Text = "";
        ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
        range.Text = "";
        ddl.Date = DateTime.Today;
        pic.Source = new BitmapImage(new Uri("ms-appx:///Assets/todo.png"));
        Common.selectName = "";
      }
    }

    //删除事项
    private async void delete_Click(object sender, RoutedEventArgs e) {
      string sql = @"DELETE FROM Todo WHERE ID = ?";
      try {
        using (var res = conn.Prepare(sql)) {
          res.Bind(1, ViewModel.SelectedItem.ID);
          res.Step();
          ViewModel.RemoveItem();
          Common.selectName = "";
          await new MessageDialog("删除成功！").ShowAsync();
          Frame.Navigate(typeof(Todo));
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        throw;
      }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e) {
      SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
          AppViewBackButtonVisibility.Visible;
      title.Focus(FocusState.Programmatic);
      if (e.NavigationMode == NavigationMode.New) {
        if (ViewModel.SelectedItem != null) {
          create_update.Content = "更 新";
          delete.Visibility = Visibility.Visible;
          title.Text = ViewModel.SelectedItem.title;
          ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
          range.Text = ViewModel.SelectedItem.detail;
          ddl.Date = ViewModel.SelectedItem.date;
          pic.Source = ViewModel.SelectedItem.img;
        }
        // If this is a new navigation, this is a fresh launch so we can discard any saved state
        ApplicationData.Current.LocalSettings.Values.Remove("Item");
        ApplicationData.Current.LocalSettings.Values.Remove("allitems");
        ApplicationData.Current.LocalSettings.Values.Remove("selectitem");
      } else {
        // Try to restore state if any, in case we were terminated
        if (ApplicationData.Current.LocalSettings.Values.ContainsKey("allitems")) {
          var allitems = ViewModel.getItems;
          allitems.Clear();
          List<string> L = JsonConvert.DeserializeObject<List<string>>(
            (string)ApplicationData.Current.LocalSettings.Values["allitems"]);
          foreach (var l in L) {
            myItem a = JsonConvert.DeserializeObject<myItem>(l);
            TodoItem item = new TodoItem(a.ID, a.date.Date, a.imgname, a.title, a.detail, a.finish);
            allitems.Add(item);
          }
        }
        if (ApplicationData.Current.LocalSettings.Values.ContainsKey("selectitem")) {
          ViewModel.SelectedItem = ViewModel.getItems[(int)(ApplicationData.Current.LocalSettings.Values["selectitem"])];
        }
        if (ApplicationData.Current.LocalSettings.Containers.ContainsKey("Item")) {
          ApplicationDataContainer Item = ApplicationData.Current.LocalSettings.Containers["Item"];
          create_update.Content = Item.Values["btn"] as string;
          title.Text = Item.Values["title"] as string;
          ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
          range.Text = Item.Values["detail"] as string;
          ddl.Date = (DateTimeOffset)(Item.Values["date"]);
          Common.selectName = Item.Values["imgname"] as string;

          if (Common.selectName == "") {
            pic.Source = new BitmapImage(new Uri("ms-appx:///Assets/todo.png"));
          } else {
            var file = await ApplicationData.Current.LocalFolder.GetFileAsync(Common.selectName);
            IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
            BitmapImage bitmapImage = new BitmapImage();
            await bitmapImage.SetSourceAsync(fileStream);
            pic.Source = bitmapImage;
          }
        }
      }
    }

    protected override void OnNavigatedFrom(NavigationEventArgs e) {
      if (((App)App.Current).IsSuspending) {
        // Save volatile state in case we get terminated later on
        // then we can restore as if we'd never been gone

        ApplicationDataContainer Item = ApplicationData.Current.LocalSettings.CreateContainer("Item", ApplicationDataCreateDisposition.Always);
        if (ApplicationData.Current.LocalSettings.Containers.ContainsKey("Item")) {
          Item.Values["title"] = title.Text;
          ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
          Item.Values["detail"] = range.Text;
          Item.Values["date"] = ddl.Date;
          Item.Values["imgname"] = Common.selectName;
          Item.Values["btn"] = create_update.Content;
        }
        if (ViewModel.SelectedItem != null) {
          ApplicationData.Current.LocalSettings.Values["selectitem"] = ViewModel.getItems.IndexOf(ViewModel.SelectedItem);
        }

        List<string> L = new List<string>();
        var allitems = ViewModel.getItems;
        foreach (var a in allitems) {
          var item = new myItem(a.ID, a.date, a.imgname, a.title, a.detail, a.finish);
          L.Add(JsonConvert.SerializeObject(item));
        }
        ApplicationData.Current.LocalSettings.Values["allitems"] = JsonConvert.SerializeObject(L);
      }
    }
  }
}