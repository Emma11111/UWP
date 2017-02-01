using HtmlAgilityPack;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Text;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;
using HereYou.ViewModel;
using Windows.UI;
using System.Diagnostics;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace HereYou {
  /// <summary>
  /// 可用于自身或导航至 Frame 内部的空白页。
  /// </summary>
  public sealed partial class CommentPage {
    public CommentPage() {
      this.InitializeComponent();
      this.commentViewModel = new CommentItemViewModel();
      var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
      titleBar.ForegroundColor = Colors.White;
      titleBar.BackgroundColor = Colors.DodgerBlue;
      titleBar.ButtonBackgroundColor = Colors.DodgerBlue;
    }

    private CourseItemViewModel ViewModel;
    private CommentItemViewModel commentViewModel;

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
          AppViewBackButtonVisibility.Visible;
      ViewModel = ((CourseItemViewModel)e.Parameter);
      setInfo();
      getComments();
    }

    private void setInfo() {
      courseNameBlock.Text = ViewModel.SelectedItem.courseName;
      teacherBlock.Text = "任课老师： " + ViewModel.SelectedItem.teacher;
      scoreBlock.Text = "综合评分： " + ViewModel.SelectedItem.score;
    }

    //获取公选评论
    private async void getComments() {
      noneCommentBlock.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
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
            noneCommentBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
            CommentListView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            //await new MessageDialog("不好意思，该课程暂时没有同学评论~~").ShowAsync();
            httpClient.Dispose();
            return;
          }

          var array = JsonConvert.DeserializeObject<JArray>(jsonContent);
          commentViewModel.ClearAllItems();
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
          CommentListView.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
          noneCommentBlock.Visibility = Windows.UI.Xaml.Visibility.Visible;
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
