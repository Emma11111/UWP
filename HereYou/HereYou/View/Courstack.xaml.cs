using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using HereYou.ViewModel;
using Windows.UI;
using Windows.UI.ViewManagement;
using System.Diagnostics;


namespace HereYou {
  /// <summary>
  /// 可用于自身或导航至 Frame 内部的空白页。
  /// </summary>
  public sealed partial class Courstack {
    public static bool hasGetCourse = false;
    public Courstack() {
      this.InitializeComponent();
      this.currentPage = 1;
      this.ViewModel = new CourseItemViewModel();
      this.commentViewModel = new CommentItemViewModel();
      var titleBar = ApplicationView.GetForCurrentView().TitleBar;
      titleBar.ForegroundColor = Colors.White;
      titleBar.BackgroundColor = Colors.DodgerBlue;
      titleBar.ButtonBackgroundColor = Colors.DodgerBlue;
      NavigationCacheMode = NavigationCacheMode.Enabled;
      if (hasGetCourse == false) {
        GetCourseList();
        hasGetCourse = true;
      }
    }

    CourseItemViewModel ViewModel { get; set; }
    private CommentItemViewModel commentViewModel;

    public int currentPage { get; set; }

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      Frame rootFrame = Window.Current.Content as Frame;
      if (rootFrame != null && rootFrame.CanGoBack) {
        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Visible;
      } else {
        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Collapsed;
      }
    }

    //查询老师或课程名
    private async void search_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
      try {
        HttpClient httpClient = new HttpClient();

        var headers = httpClient.DefaultRequestHeaders;
        string header = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.87 Safari/537.36";
        if (!headers.UserAgent.TryParseAdd(header)) {
          throw new Exception("Invaild header value: " + header);
        }

        string courseURL = "http://www.courstack.com/search/sysu?word=" + args.QueryText;
        HttpResponseMessage res = await httpClient.GetAsync(courseURL);
        res.EnsureSuccessStatusCode();

        string returnContent = await res.Content.ReadAsStringAsync();
        HtmlDocument html = new HtmlDocument();
        html.LoadHtml(returnContent);
        var nodes = html.DocumentNode.SelectNodes("//a[@href]");

        CourseItemViewModel searchViewModel = new CourseItemViewModel();

        foreach (var item in nodes) {
          var href = item.Attributes["href"].Value.ToString();
          if (href.Contains("/course/sysu")) {
            string id = href.Substring(13);
            var result = ViewModel.FindCourseByID(id);
            if (result != null) {
              searchViewModel.AddCourseItem(result);
            }
          }
        }
        httpClient.Dispose();
        ((App)App.Current).myframe.Navigate(typeof(SearchPage), searchViewModel);
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        if (ex.Message.Equals("An error occurred while sending the request.")) {
          await new MessageDialog("应用无法联网，请检查您的网络设置").ShowAsync();
        }
      }
    }

    //获取评价
    private async void getComments() {
      try {
        HttpClient httpClient = new HttpClient();

        var headers = httpClient.DefaultRequestHeaders;
        string header = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.87 Safari/537.36";
        if (!headers.UserAgent.TryParseAdd(header)) {
          throw new Exception("Invaild header value: " + header);
        }

        string id = ViewModel.SelectedItem.id;
        string commentURL = "http://www.courstack.com/course/sysu/" + id;
        HttpResponseMessage resPage = await httpClient.GetAsync(commentURL);
        resPage.EnsureSuccessStatusCode();

        string returnContent = await resPage.Content.ReadAsStringAsync();

        HtmlDocument html = new HtmlDocument();
        html.LoadHtml(returnContent);

        var node = html.DocumentNode.SelectSingleNode("//div[@commentid]");

        if (!node.Attributes["commentid"].Value.ToString().Equals("")) {
          var commentID = node.Attributes["commentid"].Value.ToString();
          string commentJSONURL = "http://www.courstack.com/course/sysu/" + id + "/comment?size=20&start-at=" + commentID;

          HttpResponseMessage resComment = await httpClient.GetAsync(commentJSONURL);
          resComment.EnsureSuccessStatusCode();

          Byte[] getByte = await resComment.Content.ReadAsByteArrayAsync();
          Encoding code = Encoding.GetEncoding("UTF-8");
          string jsonContent = code.GetString(getByte, 0, getByte.Length);

          if (jsonContent == null || jsonContent.Equals("empty")) {
            noneCommentBlock.Visibility = Visibility;
            CommentListView.Visibility = Visibility.Collapsed;
            //await new MessageDialog("不好意思，该课程暂时没有同学评论~~").ShowAsync();
            httpClient.Dispose();
            return;
          }
          commentViewModel.ClearAllItems();
          var array = JsonConvert.DeserializeObject<JArray>(jsonContent);
          foreach (var item in array) {
            var comment = JsonConvert.DeserializeObject<JArray>(item.ToString());
            string username = comment[1].ToString();
            string avatar = comment[2].ToString();
            string body = comment[3].ToString();
            string time = comment[4].ToString();
            string score = comment[9].ToString().Equals("") ? "无" : comment[9].ToString();
            if (avatar.Contains("/static")) {
              avatar = "ms-appx:///Assets/default.png";
            }
            this.commentViewModel.AddCommentItem(username, avatar, body, time, score);
          }
          CommentListView.Visibility = Windows.UI.Xaml.Visibility.Visible;
        } else {
          CommentListView.Visibility = Visibility.Collapsed;
          noneCommentBlock.Visibility = Visibility;
          //await new MessageDialog("不好意思，该课程暂时没有同学评论~~").ShowAsync();
        }
        httpClient.Dispose();
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        if (ex.Message.Equals("An error occurred while sending the request.")) {
          await new MessageDialog("应用无法联网，请检查您的网络设置").ShowAsync();
        }
      }
    }

    //获取所以公选课及对应评价
    public async void GetCourseList() {
      try {
        HttpClient httpClient = new HttpClient();

        var headers = httpClient.DefaultRequestHeaders;
        string header = "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.87 Safari/537.36";
        if (!headers.UserAgent.TryParseAdd(header)) {
          throw new Exception("Invaild header value: " + header);
        }
        ViewModel.viewItems.Clear();
        ViewModel.ClearAllItems();
        ring.IsActive = true;
        CourseGrid.Opacity = 0;
        CommentGrid.Opacity = 0;
        for (int i = 1; i <= 32; i++) {
          string courseListURL = "http://www.courstack.com/course-list/sysu/ajax?size=20&p=" + i + "&sort-mode=comment_sum";
          HttpResponseMessage res = await httpClient.GetAsync(courseListURL);
          res.EnsureSuccessStatusCode();

          Byte[] getByte = await res.Content.ReadAsByteArrayAsync();
          Encoding code = Encoding.GetEncoding("UTF-8");
          string returnContent = code.GetString(getByte, 0, getByte.Length);

          var array = JsonConvert.DeserializeObject<JArray>(returnContent);
          foreach (var item in array) {
            var course = JsonConvert.DeserializeObject<JArray>(item.ToString());
            string id = course[0].ToString();
            string courseName = course[1].ToString();
            string teacher = course[2].ToString();
            string score = course[6].ToString().Equals("") ? "无" : course[6].ToString();
            ViewModel.AddCourseItem(id, courseName, score, teacher);
          }
        }

        ring.IsActive = false;
        CourseGrid.Opacity = 1;
        CommentGrid.Opacity = 1;
        for (int j = (currentPage - 1) * 20; j < 20 * currentPage; j++) {
          ViewModel.viewItems.Add(ViewModel.AllItems[j]);
        }
        buttonsGrid.Visibility = Visibility.Visible;
        CourseListView.SelectedIndex = 0;
        ViewModel.SelectedItem = CourseListView.SelectedItem as Model.CourseItem;
        setInfo();
        getComments();

        httpClient.Dispose();
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        if (ex.Message.Equals("An error occurred while sending the request.")) {
          await new MessageDialog("应用无法联网，请检查您的网络设置").ShowAsync();
        }
      }

    }

    //点击某个公选课
    private void CourseListView_ItemClick(object sender, ItemClickEventArgs e) {
      noneCommentBlock.Visibility = Visibility.Collapsed;
      ViewModel.SelectedItem = (Model.CourseItem)(e.ClickedItem);

      int width = (int)Math.Floor(Window.Current.Bounds.Width);
      if (width <= 950) {
        ((App)App.Current).myframe.Navigate(typeof(CommentPage), ViewModel);
      } else {
        setInfo();
        getComments();
        CommentScrollViewer.ChangeView(null, 0, null);
      }
    }

    //前8页
    private void forward_Click(object sender, RoutedEventArgs e) {
      backward.IsEnabled = true;
      Button first = this.FindName("button_1") as Button;
      int content = int.Parse(first.Content as string);

      if (content == 9) {
        forward.IsEnabled = false;
      }

      for (int i = 1; i <= 8; i++) {
        Button temp = this.FindName("button_" + i.ToString()) as Button;
        int num = int.Parse(temp.Content as string) - 8;
        temp.Content = num.ToString();
        if (currentPage == num) {
          temp.IsEnabled = false;
        } else {
          temp.IsEnabled = true;
        }
      }
    }

    //后8页
    private void backward_Click(object sender, RoutedEventArgs e) {
      forward.IsEnabled = true;
      Button first = this.FindName("button_1") as Button;
      int content = int.Parse(first.Content as string);

      if (content == 17) {
        backward.IsEnabled = false;
      }

      for (int i = 1; i <= 8; i++) {
        Button temp = this.FindName("button_" + i.ToString()) as Button;
        int num = int.Parse(temp.Content as string) + 8;
        temp.Content = num.ToString();
        if (currentPage == num) {
          temp.IsEnabled = false;
        } else {
          temp.IsEnabled = true;
        }
      }
    }

    //选页
    private void button_Click(object sender, RoutedEventArgs e) {
      Button newBtn = sender as Button;
      int target = (currentPage - 1) % 8 + 1;
      Button oldBtn = this.FindName("button_" + target.ToString()) as Button;

      oldBtn.IsEnabled = true;
      newBtn.IsEnabled = false;

      currentPage = int.Parse(newBtn.Content as string);

      ViewModel.viewItems.Clear();
      for (int j = (currentPage - 1) * 20; j < currentPage * 20; j++) {
        if (ViewModel.AllItems.Count > j) {
          ViewModel.viewItems.Add(ViewModel.AllItems[j]);
        }
      }
      CourseScrollViewer.ChangeView(null, 0, null);
    }

    private void setInfo() {
      courseNameBlock.Text = ViewModel.SelectedItem.courseName;
      teacherBlock.Text = "任课老师： " + ViewModel.SelectedItem.teacher;
      scoreBlock.Text = "综合评分： " + ViewModel.SelectedItem.score;
    }
  }
}