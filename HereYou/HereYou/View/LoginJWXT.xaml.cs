using Windows.UI.Xaml.Navigation;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Diagnostics;
using Windows.UI.Xaml;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using Windows.UI.Core;

namespace HereYou {
  public sealed partial class LoginJWXT {
    public string rno = "", j_username, j_password;
    public string topage;
    public LoginJWXT() {
      InitializeComponent();
      var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
      titleBar.ForegroundColor = Colors.White;
      titleBar.BackgroundColor = Colors.DodgerBlue;
      titleBar.ButtonBackgroundColor = Colors.DodgerBlue;
      NavigationCacheMode = NavigationCacheMode.Enabled;
      webview.NavigationCompleted += Webview_NavigationCompleted;
    }

    private async void Webview_NavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args) {
      j_username = await sender.InvokeScriptAsync("eval", new string[] { "document.getElementById(\'j_username\').value" });
      j_password = await sender.InvokeScriptAsync("eval", new string[] { "document.getElementById(\'pwd\').value" });
      ((App)App.Current).isLogin = true;
      ApplicationDataContainer Item = null;
      if (ApplicationData.Current.LocalSettings.Containers.ContainsKey("Item"))
        Item = ApplicationData.Current.LocalSettings.Containers["Item"];
      else
        Item = ApplicationData.Current.LocalSettings.CreateContainer("Item", ApplicationDataCreateDisposition.Always);
      Item.Values["j_username"] = j_username;
      Item.Values["pwd"] = j_password;
      webview.NavigationStarting -= Webview_NavigationStarting;
      if (topage == "score")
        Frame.Navigate(typeof(Scores), "LoginJWXT");
      if (topage == "coursetable")
        Frame.Navigate(typeof(CourseTable), "LoginJWXT");
    }

    private async void Webview_NavigationCompleted(WebView sender, WebViewNavigationCompletedEventArgs args) {
      ApplicationDataContainer Item = null;
      if (ApplicationData.Current.LocalSettings.Containers.ContainsKey("Item"))
        Item = ApplicationData.Current.LocalSettings.Containers["Item"];
      if (Item != null) {
        await webview.InvokeScriptAsync("eval", new string[] { "document.getElementById(\'j_username\').value=" + Item.Values["j_username"] });
        Debug.WriteLine(Item.Values["pwd"]);
        await webview.InvokeScriptAsync("eval", new string[] { "document.getElementById(\'pwd\').value=\'" + Item.Values["pwd"] + "\'"});
      }
      webview.NavigationCompleted -= Webview_NavigationCompleted;
      webview.NavigationStarting += Webview_NavigationStarting;
    }

    protected override void OnNavigatedTo(NavigationEventArgs e) {
      topage = e.Parameter.ToString();
      Frame rootFrame = Window.Current.Content as Frame;
      if (rootFrame != null && rootFrame.CanGoBack) {
        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Visible;
      } else {
        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Collapsed;
      }
      if (((App)App.Current).isLogin) {
        if (topage == "score")
          Frame.Navigate(typeof(Scores), "LoginJWXT");
        if (topage == "coursetable")
          Frame.Navigate(typeof(CourseTable), "LoginJWXT");
      }
    }
    //try {
    //  // 如果已登录教务系统,直接跳转
    //  Uri home = new Uri(@"http://uems.sysu.edu.cn/jwxt/edp/index.jsp");
    //  var httpClient = new HttpClient();
    //  HttpResponseMessage httpResponse = await httpClient.GetAsync(home);
    //  httpResponse.EnsureSuccessStatusCode();
    //  string res = await httpResponse.Content.ReadAsStringAsync();
    //  if (res.IndexOf("会话过期,请重新登录") == -1) {
    //  }
    //} catch (Exception ex) {
    //  Debug.WriteLine(ex.Message);
    //}

    // 将密码进行MD5加密
    private string getMD5() {
      //可以选择MD5 Sha1 Sha256 Sha384 Sha512
      string strAlgName = HashAlgorithmNames.Md5;
      // 创建一个 HashAlgorithmProvider 对象
      HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);
      // 创建一个可重用的CryptographicHash对象           
      CryptographicHash objHash = objAlgProv.CreateHash();

      string psw = password.Password;
      IBuffer buff = CryptographicBuffer.ConvertStringToBinary(psw, BinaryStringEncoding.Utf8);
      objHash.Append(buff);
      IBuffer buffHash = objHash.GetValueAndReset();
      string strHash = CryptographicBuffer.EncodeToHexString(buffHash);
      return strHash.ToUpper();
    }

    // 获取验证码
    private async void getCode() {
      Uri requestUri = new Uri(@"http://uems.sysu.edu.cn/jwxt");
      Uri codeurl = new Uri(@"http://uems.sysu.edu.cn/jwxt/jcaptcha");
      try {
        //先访问教务系统获取cookie
        var httpClient = new HttpClient();
        HttpResponseMessage httpResponse = await httpClient.GetAsync(requestUri);
        httpResponse.EnsureSuccessStatusCode();
        string html = await httpResponse.Content.ReadAsStringAsync();
        Regex reg = new Regex("name=\"rno\" value=(.+)></");
        Match match = reg.Match(html);
        rno = match.Groups[1].Value; //获取隐藏域
                                     //带着cookie访问验证码图片
        httpResponse = await httpClient.GetAsync(codeurl);
        httpResponse.EnsureSuccessStatusCode();
        // 显示验证码图片
        InMemoryRandomAccessStream randomAccess = new InMemoryRandomAccessStream();
        DataWriter writer = new DataWriter(randomAccess.GetOutputStreamAt(0));
        writer.WriteBytes(await httpResponse.Content.ReadAsByteArrayAsync());
        await writer.StoreAsync();
        BitmapImage bm = new BitmapImage();
        await bm.SetSourceAsync(randomAccess);
        j_codepic.Source = bm;
        // store InMemoryRandomAccessStream into a file
        //var codefile = await ApplicationData.Current.LocalFolder.CreateFileAsync("code.jpeg", CreationCollisionOption.ReplaceExisting);
        //using (var fileStream = await codefile.OpenAsync(FileAccessMode.ReadWrite)) {
        //  await RandomAccessStream.CopyAndCloseAsync(randomAccess.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
        //}
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        if (ex.Message.Equals("An error occurred while sending the request.")) {
          await new MessageDialog("应用无法联网，请检查您的网络设置").ShowAsync();
        }
      }
    }

    //刷新验证码
    private void freshCode(object sender, RoutedEventArgs e) {
      getCode();
    }

    //登录教务系统
    private async void postSignin(object sender, RoutedEventArgs e) {
      Uri signin = new Uri(@"http://uems.sysu.edu.cn/jwxt/j_unieap_security_check.do");
      try {
        var postData = new List<KeyValuePair<string, string>>();
        postData.Add(new KeyValuePair<string, string>("j_username", stuNum.Text.Trim()));
        postData.Add(new KeyValuePair<string, string>("j_password", getMD5()));
        postData.Add(new KeyValuePair<string, string>("rno", rno));
        postData.Add(new KeyValuePair<string, string>("jcaptcha_response", j_code.Text.Trim()));
        var httpClient = new HttpClient();
        HttpContent httpcontent = new FormUrlEncodedContent(postData);
        HttpResponseMessage httpResponse = await httpClient.PostAsync(signin, httpcontent);
        httpResponse.EnsureSuccessStatusCode();
        string res = await httpResponse.Content.ReadAsStringAsync();
        Regex reg = new Regex("<span style=\"color:#D7E1EC\">(.+)</span>");
        Match match = reg.Match(res);
        string flag = match.Groups[1].Value;
        if (flag != "") { // 登录不成功
          j_code.Text = "";
          await new MessageDialog(flag).ShowAsync();
          getCode();
        } else {
          if (remember.IsChecked == true)
            rememberMe();
          else
            forgetMe();
          ((App)App.Current).isLogin = true;
          if (topage == "score")
            Frame.Navigate(typeof(Scores), "LoginJWXT");
          if (topage == "coursetable")
            Frame.Navigate(typeof(CourseTable), "LoginJWXT");
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        if (ex.Message.Equals("An error occurred while sending the request.")) {
          await new MessageDialog("应用无法联网，请检查您的网络设置").ShowAsync();
        }
      }
    }

    //记住密码
    private void rememberMe() {
      ApplicationDataContainer Item = null;
      if (ApplicationData.Current.LocalSettings.Containers.ContainsKey("Item"))
        Item = ApplicationData.Current.LocalSettings.Containers["Item"];
      else
        Item = ApplicationData.Current.LocalSettings.CreateContainer("Item", ApplicationDataCreateDisposition.Always);
      Item.Values["stuNum"] = stuNum.Text;
      Item.Values["password"] = password.Password;
      Item.Values["check"] = true;
    }

    //忘记密码
    private void forgetMe() {
      if (ApplicationData.Current.LocalSettings.Containers.ContainsKey("Item"))
        ApplicationData.Current.LocalSettings.DeleteContainer("Item");
    }

    //实现输完验证码回车登录
    private void j_code_KeyUp(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e) {
      if (e.Key == Windows.System.VirtualKey.Enter)
        postSignin(null, null);
    }
  }
}
