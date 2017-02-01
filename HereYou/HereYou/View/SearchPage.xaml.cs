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
using HereYou.Model;
using HereYou.ViewModel;
using Windows.UI;
using System.Diagnostics;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace HereYou {
  /// <summary>
  /// 可用于自身或导航至 Frame 内部的空白页。
  /// </summary>
  public sealed partial class SearchPage {
    public SearchPage() {
      this.InitializeComponent();
      var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
      titleBar.ForegroundColor = Colors.White;
      titleBar.BackgroundColor = Colors.DodgerBlue;
      titleBar.ButtonBackgroundColor = Colors.DodgerBlue;
      this.commentViewModel = new CommentItemViewModel();
      this.ViewModel = new CourseItemViewModel();
    }
    CourseItemViewModel ViewModel { get; set; }
    private CommentItemViewModel commentViewModel;

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
          AppViewBackButtonVisibility.Visible;

      ViewModel = ((CourseItemViewModel)e.Parameter);
      this.Loaded += Page_Loaded;
    }

    private async void Page_Loaded(object sender, RoutedEventArgs e) {
      if (ViewModel.AllItems.Count > 0) {
        CourseListView.SelectedIndex = 0;
        ViewModel.SelectedItem = CourseListView.SelectedItem as CourseItem;
        setInfo();
        getComments();
      } else {
        CommentGrid.Visibility = Visibility.Collapsed;
        await new MessageDialog("不好意思，没有搜索到与该关键词有关的公选课~~").ShowAsync();
      }

        ((Page)sender).Loaded -= Page_Loaded;
    }

    //点击某个公选
    private void CourseListView_ItemClick(object sender, ItemClickEventArgs e) {
      noneCommentBlock.Visibility = Visibility.Collapsed;
      ViewModel.SelectedItem = (Model.CourseItem)(e.ClickedItem);

      int width = (int)Math.Floor(Window.Current.Bounds.Width);
      if (width < 900) {
        Frame.Navigate(typeof(CommentPage), ViewModel);

      } else {
        commentViewModel.ClearAllItems();
        setInfo();
        getComments();
      }
    }

    private void setInfo() {
      courseNameBlock.Text = ViewModel.SelectedItem.courseName;
      teacherBlock.Text = "任课老师： " + ViewModel.SelectedItem.teacher;
      scoreBlock.Text = "综合评分： " + ViewModel.SelectedItem.score;
    }

    //获取评论
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
            noneCommentBlock.Visibility = Visibility.Visible;
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
          noneCommentBlock.Visibility = Visibility.Visible;
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
  }
}
