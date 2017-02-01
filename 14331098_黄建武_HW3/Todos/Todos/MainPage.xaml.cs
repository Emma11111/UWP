using Windows.UI.Xaml;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Core;
using Windows.UI;
using Windows.Storage.Streams;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Text;
using Windows.UI.Popups;


//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace Todos {
  /// <summary>
  /// 可用于自身或导航至 Frame 内部的空白页。
  /// </summary>
  public sealed partial class MainPage : Page {
    ViewModels.TodoItemViewModel ViewModel { get; set; }
    public MainPage() {
      InitializeComponent();
      var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
      titleBar.ForegroundColor = Colors.White;
      titleBar.BackgroundColor = Colors.DodgerBlue;
      titleBar.ButtonBackgroundColor = Colors.DodgerBlue;
      NavigationCacheMode = NavigationCacheMode.Enabled;
      ViewModel = new ViewModels.TodoItemViewModel();
    }

    private void selectPic(object sender, RoutedEventArgs e) {
      var common = new Common();
      common.selectPic(pic);
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
        this.ViewModel.AddItem(ddl.Date.DateTime, pic.Source, title.Text, range.Text);
        Cancle_Click(null, null);
        ViewModel.SelectedItem = null;
        await new MessageDialog("Create successfully!").ShowAsync();
      } else {
        this.ViewModel.SelectedItem.UpdateItem(ddl.Date.DateTime, pic.Source, title.Text, range.Text);
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
      } else {
        title.Text = "";
        ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
        range.Text = "";
        ddl.Date = DateTime.Today;
        pic.Source = new BitmapImage(new Uri("ms-appx:///Assets/todo.png"));
      }
    }

    private void addtodo_Click(object sender, RoutedEventArgs e) {
      if (rightContent.Visibility.ToString() == "Collapsed") {
        ViewModel.SelectedItem = null;
        Frame.Navigate(typeof(AddTodo), this.ViewModel);
      } else {
        create_update.Content = "Create";
        Cancle_Click(null, null);
        delete.Visibility = Visibility.Collapsed;
        title.Focus(FocusState.Programmatic);
      }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
          AppViewBackButtonVisibility.Collapsed;
      if (e.Parameter.GetType() == typeof(ViewModels.TodoItemViewModel))
        ViewModel = e.Parameter as ViewModels.TodoItemViewModel;
    }

    private void itemClick(object sender, ItemClickEventArgs e) {
      ViewModel.SelectedItem = e.ClickedItem as Models.TodoItem;
      if (rightContent.Visibility.ToString() == "Collapsed") {
        Frame.Navigate(typeof(AddTodo), ViewModel);
      } else {
        delete.Visibility = Visibility.Visible;
        create_update.Content = "Update";
        title.Text = ViewModel.SelectedItem.title;
        ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
        range.Text = ViewModel.SelectedItem.detail;
        ddl.Date = ViewModel.SelectedItem.date;
        pic.Source = ViewModel.SelectedItem.img;
      }
    }

    private async void delete_Click(object sender, RoutedEventArgs e) {
      delete.Visibility = Visibility.Collapsed;
      create_update.Content = "Create";
      Cancle_Click(null, null);
      ViewModel.getItems.Remove(ViewModel.SelectedItem);
      ViewModel.SelectedItem = null;
      await new MessageDialog("Delete successfully！").ShowAsync();
    }
  }
}
