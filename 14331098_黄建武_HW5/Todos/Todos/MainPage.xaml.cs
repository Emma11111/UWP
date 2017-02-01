using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Core;
using Windows.UI;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Text;
using Windows.UI.Popups;
using Windows.Storage;
using Windows.Storage.Streams;
using System.Collections.Generic;
using Newtonsoft.Json;
using Windows.UI.Notifications;
using NotificationsExtensions.Tiles;
using Windows.ApplicationModel.DataTransfer;
using Windows.ApplicationModel;
using System.Diagnostics;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Todos {
  /// <summary>
  /// 可用于自身或导航至 Frame 内部的空白页。
  /// </summary>
  public sealed partial class MainPage : Page {
    private TodoItemViewModel ViewModel = Common.ViewModel;
    private string sharetitle = "", sharedetail = "", shareimgname = "";
    private StorageFile shareimg;
    DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
    private string sharedate;

    public MainPage() {
      InitializeComponent();
      var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
      titleBar.ForegroundColor = Colors.White;
      titleBar.BackgroundColor = Colors.DodgerBlue;
      titleBar.ButtonBackgroundColor = Colors.DodgerBlue;
      NavigationCacheMode = NavigationCacheMode.Enabled;
      DataTransferManager dataTransferManager = DataTransferManager.GetForCurrentView();
    }

    private void DataRequested(DataTransferManager sender, DataRequestedEventArgs e) {
      Debug.WriteLine(e.Request.ToString());
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

    private async void selectPic(object sender, RoutedEventArgs e) {
      Common common = new Common();
      await common.selectPic(pic);
    }

    private void check(object sender, RoutedEventArgs e) {
      var parent = VisualTreeHelper.GetParent(sender as DependencyObject);
      Line line = VisualTreeHelper.GetChild(parent, 2) as Line;
      line.Opacity = 1;
    }

    private void uncheck(object sender, RoutedEventArgs e) {
      var parent = VisualTreeHelper.GetParent(sender as DependencyObject);
      Line line = VisualTreeHelper.GetChild(parent, 2) as Line;
      line.Opacity = 0;
    }

    private async void Create_Update_Click(object sender, RoutedEventArgs e) {
      string alert = "";
      ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
      if (title.Text.Trim() == "")
        alert += "Title can't be empty!\n";
      if (range.Text.Trim() == "")
        alert += "Detail can't be empty!\n";
      if (ddl.Date.Date.CompareTo(DateTime.Today) < 0)
        alert += "The due date has passed!";

      if (alert != "")
        await new MessageDialog(alert).ShowAsync();
      else if (create_update.Content.ToString() == "Create") {
        ViewModel.AddItem(ddl.Date.DateTime, Common.selectName, title.Text, range.Text);
        Common.selectName = "";
        Cancle_Click(null, null);
        ViewModel.SelectedItem = null;
        await new MessageDialog("Create successfully!").ShowAsync();
      } else {
        ViewModel.UpdateItem(ddl.Date.DateTime, Common.selectName, title.Text, range.Text);
        await new MessageDialog("Update successfully!").ShowAsync();
      }
    }

    private void Cancle_Click(object sender, RoutedEventArgs e) {
      if (create_update.Content.ToString() == "Update") {
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

    private void addtodo_Click(object sender, RoutedEventArgs e) {
      if (rightContent.Visibility.ToString() == "Collapsed") {
        ViewModel.SelectedItem = null;
        Frame.Navigate(typeof(AddTodo));
      } else {
        create_update.Content = "Create";
        Cancle_Click(null, null);
        title.Focus(FocusState.Programmatic);
      }
      Common.selectName = "";
    }

    private async void delete_Click(object sender, RoutedEventArgs e) {
      create_update.Content = "Create";
      Cancle_Click(null, null);
      ViewModel.RemoveItem();
      Common.selectName = "";
      await new MessageDialog("Delete successfully！").ShowAsync();
    }

    private void editOneItem(object sender, RoutedEventArgs e) {
      var dc = (sender as FrameworkElement).DataContext;
      var item = leftContent.ContainerFromItem(dc) as ListViewItem;
      ViewModel.SelectedItem = item.Content as TodoItem;
      if (rightContent.Visibility.ToString() == "Collapsed") {
        Frame.Navigate(typeof(AddTodo));
      } else {
        create_update.Content = "Update";
        title.Text = ViewModel.SelectedItem.title;
        ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
        range.Text = ViewModel.SelectedItem.detail;
        ddl.Date = ViewModel.SelectedItem.date;
        pic.Source = ViewModel.SelectedItem.img;
      }
    }

    private void deleteOneItem(object sender, RoutedEventArgs e) {
      var dc = (sender as FrameworkElement).DataContext;
      var item = leftContent.ContainerFromItem(dc) as ListViewItem;
      ViewModel.SelectedItem = item.Content as TodoItem;
      create_update.Content = "Create";
      Cancle_Click(null, null);
      Common.selectName = "";
      ViewModel.RemoveItem();
    }

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

    protected override void OnNavigatedFrom(NavigationEventArgs e) {
      dataTransferManager.DataRequested -= DataRequested;
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
          var item = new myItem(a.date, a.imgname, a.title, a.detail, a.finish);
          L.Add(JsonConvert.SerializeObject(item));
        }
        ApplicationData.Current.LocalSettings.Values["allitems"] = JsonConvert.SerializeObject(L);
      }
    }

    protected override async void OnNavigatedTo(NavigationEventArgs e) {
      SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
      dataTransferManager.DataRequested += DataRequested;
      if (e.NavigationMode == NavigationMode.New) {
        // If this is a new navigation, this is a fresh launch so we can discard any saved state
        ApplicationData.Current.LocalSettings.Values.Remove("Item");
        ApplicationData.Current.LocalSettings.Values.Remove("allitems");
        ApplicationData.Current.LocalSettings.Values.Remove("selectitem");
      } else {
        // Try to restore state if any, in case we were terminated
        if (ApplicationData.Current.LocalSettings.Values.ContainsKey("allitems")) {
          ViewModel.getItems.Clear();
          List<string> L = JsonConvert.DeserializeObject<List<string>>(
            (string)ApplicationData.Current.LocalSettings.Values["allitems"]);
          foreach (var l in L) {
            myItem a = JsonConvert.DeserializeObject<myItem>(l);
            TodoItem item = new TodoItem(a.date.Date, a.imgname, a.title, a.detail, a.finish);
            ViewModel.getItems.Add(item);
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

    private void itemClick(object sender, ItemClickEventArgs e) {
      ViewModel.SelectedItem = e.ClickedItem as TodoItem;
      Common.selectName = ViewModel.SelectedItem.imgname;
      if (rightContent.Visibility.ToString() == "Collapsed") {
        Frame.Navigate(typeof(AddTodo));
      } else {
        create_update.Content = "Update";
        title.Text = ViewModel.SelectedItem.title;
        ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
        range.Text = ViewModel.SelectedItem.detail;
        ddl.Date = ViewModel.SelectedItem.date;
        pic.Source = ViewModel.SelectedItem.img;
      }
    }
  }
}
