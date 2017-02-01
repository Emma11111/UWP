using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Todos {
  /// <summary>
  /// 可用于自身或导航至 Frame 内部的空白页。
  /// </summary>
  public sealed partial class AddTodo : Page {
    public AddTodo() {
      InitializeComponent();
    }
    ViewModels.TodoItemViewModel ViewModel { get; set; }
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
        ViewModel.AddItem(ddl.Date.DateTime, pic.Source, title.Text, range.Text);
        await new MessageDialog("Create successfully!").ShowAsync();
        ViewModel.SelectedItem = null;
        Frame.Navigate(typeof(MainPage), ViewModel);
      } else {
        ViewModel.SelectedItem.UpdateItem(ddl.Date.DateTime, pic.Source, title.Text, range.Text);
        ViewModel.SelectedItem = null;
        await new MessageDialog("Update successfully!").ShowAsync();
        Frame.Navigate(typeof(MainPage), ViewModel);
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

    private void selectPic(object sender, RoutedEventArgs e) {
      var common = new Common();
      common.selectPic(pic);
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
          AppViewBackButtonVisibility.Visible;
      this.ViewModel = e.Parameter as ViewModels.TodoItemViewModel;
      if (this.ViewModel.SelectedItem != null) {
        create_update.Content = "Update";
        title.Text = ViewModel.SelectedItem.title;
        ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
        range.Text = ViewModel.SelectedItem.detail;
        ddl.Date = ViewModel.SelectedItem.date;
        pic.Source = ViewModel.SelectedItem.img;
        delete.Visibility = Visibility.Visible;
      }
    }

    private async void delete_Click(object sender, RoutedEventArgs e) {
      ViewModel.getItems.Remove(ViewModel.SelectedItem);
      ViewModel.SelectedItem = null;
      await new MessageDialog("Delete successfully！").ShowAsync();
      Frame.Navigate(typeof(MainPage), ViewModel);
    }

    private void addtodo_Click(object sender, RoutedEventArgs e) {
      create_update.Content = "Create";
      Cancle_Click(null, null);
      ViewModel.SelectedItem = null;
      delete.Visibility = Visibility.Collapsed;
      title.Focus(FocusState.Programmatic);
    }
  }
}