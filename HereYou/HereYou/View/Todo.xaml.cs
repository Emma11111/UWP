using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI;
using Windows.UI.Popups;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Notifications;
using NotificationsExtensions.Tiles;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel;
using System.Diagnostics;
using SQLitePCL;
using System.Threading.Tasks;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;
using Windows.UI.Xaml.Data;
using Windows.UI.Text;

namespace HereYou {
  /// <summary>
  /// 可用于自身或导航至 Frame 内部的空白页。
  /// </summary>
  public sealed partial class Todo {
    private TodoItemViewModel ViewModel = Common.ViewModel;
    private string sharetitle = "", sharedetail = "", shareimgname = "";
    private StorageFile shareimg;
    DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
    private string sharedate;
    private SQLiteConnection conn = App.conn;
    public Todo() {
      InitializeComponent();
      var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
      titleBar.ForegroundColor = Colors.White;
      titleBar.BackgroundColor = Colors.DodgerBlue;
      titleBar.ButtonBackgroundColor = Colors.DodgerBlue;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      Frame rootFrame = Window.Current.Content as Frame;
      if (rootFrame != null && rootFrame.CanGoBack) {
        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Visible;
      } else {
        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Collapsed;
      }
      nonetodo.Visibility = ViewModel.getItems.Count == 0 ?  Visibility.Visible : Visibility.Collapsed;
      update_tile(null, null);
    }

    //分享
    private void DataRequested(DataTransferManager sender, DataRequestedEventArgs e) {
      DataRequest request = e.Request;
      DataPackage requestData = request.Data;
      requestData.Properties.Title = sharetitle;
      requestData.SetText(sharedetail + sharedate);

      // Because we are making async calls in the DataRequested event handler,
      //  we need to get the deferral first.
      DataRequestDeferral deferral = request.GetDeferral();

      // Make sure we always call Complete on the deferral.
      try {
        requestData.SetBitmap(RandomAccessStreamReference.CreateFromFile(shareimg));
      } finally {
        deferral.Complete();
      }
    }

    //选择图片
    private async void selectPic(object sender, RoutedEventArgs e) {
      await Common.selectPic(pic);
    }

    private void check(object sender, RoutedEventArgs e) {
      //var parent = VisualTreeHelper.GetParent(sender as DependencyObject);
      //Line line = VisualTreeHelper.GetChild(parent, 4) as Line;
      //line.Opacity = 1;
      try {
        var dc = (sender as FrameworkElement).DataContext;
        var listitem = leftContent.ContainerFromItem(dc) as ListViewItem;
        var item = listitem.Content as TodoItem;
        string sql = @"UPDATE Todo SET finish = ? WHERE ID = ?";
        using (var res = conn.Prepare(sql)) {
          res.Bind(1, "true");
          res.Bind(2, item.ID);
          res.Step();
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
    }

    private void uncheck(object sender, RoutedEventArgs e) {
      //var parent = VisualTreeHelper.GetParent(sender as DependencyObject);
      //Line line = VisualTreeHelper.GetChild(parent, 4) as Line;
      //line.Opacity = 0;
      try {
        var dc = (sender as FrameworkElement).DataContext;
        var listitem = leftContent.ContainerFromItem(dc) as ListViewItem;
        var item = listitem.Content as TodoItem;
        string sql = @"UPDATE Todo SET finish = ? WHERE ID = ?";
        using (var res = conn.Prepare(sql)) {
          res.Bind(1, "false");
          res.Bind(2, item.ID);
          res.Step();
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
    }

    //添加事项
    private void addtodo_Click(object sender, RoutedEventArgs e) {
      ViewModel.SelectedItem = null;
      delete.Visibility = Visibility.Collapsed;
      Frame.Navigate(typeof(AddTodo));
      Common.selectName = "";
    }

    private async void delete_Click(object sender, RoutedEventArgs e) {
      string sql = @"DELETE FROM Todo WHERE ID = ?";
      rightContent.Visibility = Visibility.Collapsed;
      try {
        using (var res = conn.Prepare(sql)) {
          res.Bind(1, ViewModel.SelectedItem.ID);
          res.Step();
          ViewModel.RemoveItem();
          Common.selectName = "";
          nonetodo.Visibility = ViewModel.getItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
          delete.Visibility = Visibility.Collapsed;
          await new MessageDialog("删除成功！").ShowAsync();
          update_tile(null, null);
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        throw;
      }
    }

    //编辑事项
    private void editOneItem(object sender, RoutedEventArgs e) {
      var dc = (sender as FrameworkElement).DataContext;
      var item = leftContent.ContainerFromItem(dc) as ListViewItem;
      ViewModel.SelectedItem = item.Content as TodoItem;
      delete.Visibility = Visibility.Collapsed;
      Frame.Navigate(typeof(AddTodo));
    }

    //删除事项
    private void deleteOneItem(object sender, RoutedEventArgs e) {
      var dc = (sender as FrameworkElement).DataContext;
      var item = leftContent.ContainerFromItem(dc) as ListViewItem;
      ViewModel.SelectedItem = item.Content as TodoItem;
      delete_Click(null, null);
      rightContent.Visibility = Visibility.Collapsed;
      nonetodo.Visibility = ViewModel.getItems.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    //分享事项
    private async void shareOneItem(object sender, RoutedEventArgs e) {
      var dc = (sender as FrameworkElement).DataContext;
      var item = (leftContent.ContainerFromItem(dc) as ListViewItem).Content as TodoItem;
      sharetitle = item.title;
      sharedetail = item.detail;
      shareimgname = item.imgname;
      var date = item.date;
      sharedate = "\nDue date: " + date.Year + '-' + date.Month + '-' + date.Day;
      if (shareimgname == "") {
        shareimg = await Package.Current.InstalledLocation.GetFileAsync("Assets\\todo.png");
      } else {
        shareimg = await ApplicationData.Current.LocalFolder.GetFileAsync(shareimgname);
      }
      DataTransferManager.ShowShareUI();
    }

    //更新磁贴
    private void update_tile(object sender, RoutedEventArgs e) {
      if (ViewModel.getItems.Count < 1) {
        return;
      }
      string titleText = ViewModel.getItems[0].title;
      string dateText = "" + ViewModel.getItems[0].date.Year + '-' + ViewModel.getItems[0].date.Month + '-' + ViewModel.getItems[0].date.Day;
      TileContent content = new TileContent() {
        Visual = new TileVisual() {
          TileLarge = new TileBinding() {
            Content = new TileBindingContentAdaptive() {
              Children = {
                                new TileText() {
                                Text = titleText,
                                Style = TileTextStyle.Subheader
                                },
                                new TileText() {
                                Text = dateText,
                                Style = TileTextStyle.Title
                                }
                            }
            }
          },
          TileMedium = new TileBinding() {
            Content = new TileBindingContentAdaptive() {
              Children = {
                                new TileText() {
                                Text = titleText,
                                Style = TileTextStyle.Subtitle
                            },
                                new TileText() {
                                Text = dateText,
                                Style = TileTextStyle.Body
                                }
                            }
            }
          },
          TileWide = new TileBinding() {
            Content = new TileBindingContentAdaptive() {
              Children = {
                                new TileText() {
                                Text = titleText,
                                Style = TileTextStyle.Subtitle
                                },
                                new TileText() {
                                Text = dateText,
                                Style = TileTextStyle.Subtitle
                                }
                            }
            }
          }
        }
      };

      var notification = new TileNotification(content.GetXml());
      TileUpdateManager.CreateTileUpdaterForApplication().Update(notification);
    }

    //查询事项
    private async void search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
      var text = args.QueryText.Trim();
      if (text == "")
        return;
      string alert = "";
      try {
        var sql = "SELECT date, title, detail FROM Todo WHERE date LIKE ? OR title LIKE ? OR detail LIKE ?";
        using (var statement = conn.Prepare(sql)) {
          statement.Bind(1, "%%" + text + "%%");
          statement.Bind(2, "%%" + text + "%%");
          statement.Bind(3, "%%" + text + "%%");
          while (SQLiteResult.ROW == statement.Step()) {
            var date = statement[0].ToString();
            date = date.Substring(0, date.IndexOf(' '));
            string title = statement[1] as string;
            string detail = statement[2] as string;
            alert += "标题: " + title + "\n详情: " + detail + "\nDeadline: " + date + "\n\n";
          }
          if (alert == "")
            alert = "没有相关事项\n";
          await new MessageDialog(alert).ShowAsync();
        }
      } catch (Exception) {
        throw;
      }
    }

    //点击某个事项，显示详情
    private void itemClick(object sender, ItemClickEventArgs e) {
      ViewModel.SelectedItem = e.ClickedItem as TodoItem;
      Common.selectName = ViewModel.SelectedItem.imgname;
      delete.Visibility = Visibility.Visible;
      detail.IsReadOnly = false;
      if (VisualStateGroup.CurrentState.Name.ToString() == "VisualStateMin0") {
        Frame.Navigate(typeof(AddTodo));
      } else {
        rightContent.Visibility = Visibility.Visible;
        title.Text = ViewModel.SelectedItem.title;
        ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
        range.Text = ViewModel.SelectedItem.detail;
        detail.IsReadOnly = true;
        DateTime Date = ViewModel.SelectedItem.date;
        ddl.Date = Date;
        TimeSpan span = Date.Subtract(DateTime.Today);
        int gap = span.Days;
        string dayleftcontent = "";
        if (ViewModel.SelectedItem.finish == true) {
          dayleftcontent += "该事项已经完成了~~";
        }
        else if (gap < 0) {
          dayleftcontent += "Deadline已过";
        } else if (gap == 0) {
          dayleftcontent += "今天就要Deadline了, 快抓紧啊！";
        } else { 
          dayleftcontent += "距离Deadline还有" + gap.ToString() + "天";
        }
        dayleft.Text = dayleftcontent;
        pic.Source = ViewModel.SelectedItem.img;
      }
    }
  }
}
