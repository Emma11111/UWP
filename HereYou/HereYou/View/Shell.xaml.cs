using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

//侧边导航栏

namespace HereYou {
  /// <summary>
  /// 可用于自身或导航至 Frame 内部的空白页。
  /// </summary>
  public sealed partial class Shell {
    public Shell() {
      this.InitializeComponent();
      var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
      titleBar.ForegroundColor = Colors.White;
      titleBar.BackgroundColor = Colors.DodgerBlue;
      titleBar.ButtonBackgroundColor = Colors.DodgerBlue;
      NavigationCacheMode = NavigationCacheMode.Enabled;
      this.Loaded += Shell_Loaded;
    }

    private void Shell_Loaded(object sender, RoutedEventArgs e) {
      if (((App)App.Current).myframe == null) {
        ((App)App.Current).myframe = MyFrame;
        todo.Focus(FocusState.Programmatic);
      }
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      var topage = e.Parameter.ToString();
      if (topage == "todo") {
        todo.IsSelected = true;
        MyFrame.Navigate(typeof(Todo));
      }
      if (topage == "score") {
        score.IsSelected = true;
        if (((App)App.Current).isLogin == true)
          MyFrame.Navigate(typeof(Scores));
        else MyFrame.Navigate(typeof(LoginJWXT), "score");
      }
      if (topage == "coursetable") {
        coursetable.IsSelected = true;
        if (((App)App.Current).isLogin == true)
          MyFrame.Navigate(typeof(CourseTable));
        else MyFrame.Navigate(typeof(LoginJWXT), "coursetable");
      }
      if (topage == "comment") {
        comment.IsSelected = true;
        MyFrame.Navigate(typeof(Courstack));
      }
    }
    private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e) {
      splitview.IsPaneOpen = false;
      if (score.IsSelected)
        Frame.Navigate(typeof(Shell), "score");
      else if (coursetable.IsSelected)
        Frame.Navigate(typeof(Shell), "coursetable");
      else if (comment.IsSelected)
        Frame.Navigate(typeof(Shell), "comment");
      else if (todo.IsSelected)
        Frame.Navigate(typeof(Shell), "todo");
    }
  }
}
