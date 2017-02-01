using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace HereYou {
  /// <summary>
  /// 可用于自身或导航至 Frame 内部的空白页。
  /// </summary>
  public sealed partial class CourseTable {
    string table;
    public CourseTable() {
      InitializeComponent();
      var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
      titleBar.ForegroundColor = Colors.White;
      titleBar.BackgroundColor = Colors.DodgerBlue;
      titleBar.ButtonBackgroundColor = Colors.DodgerBlue;
      NavigationCacheMode = NavigationCacheMode.Enabled;

      //显示今天星期几
      string[] day = new string[] { "星期日", "星期一", "星期二", "星期三", "星期四", "星期五", "星期六" };
      string week = day[Convert.ToInt32(DateTime.Now.DayOfWeek.ToString("d"))].ToString();
      today.Text += week;
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
      queryCourseTable(null, null);
    }

    // 查询指定学期课表
    private void queryCourseTable(object sender, object e) {
      string xnd_ = (xnd.SelectedItem as ComboBoxItem).Content.ToString();
      string xq_ = (xq.SelectedItem as ComboBoxItem).Content.ToString();
      getCourseTable(xnd_, xq_);
    }

    // 导出课程表
    private void exportCourseTable(object sender, RoutedEventArgs e) {
      string xnd_ = (xnd.SelectedItem as ComboBoxItem).Content.ToString();
      string xq_ = (xq.SelectedItem as ComboBoxItem).Content.ToString();
      exportCourseTable(xnd_, xq_);
    }

    // 获取课程表具体实现
    private async void getCourseTable(string xnd, string xq) {
      try {
        string json = @"{
                    body:{
                        dataStores:{},
                        parameters:{
                            args: [""" + xq + @""",""" + xnd + @"""],
                            responseParam = ""rs""
                        }
                    }
                }";
        HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("ajaxRequest", "true");
        httpClient.DefaultRequestHeaders.Add("render", "unieap");
        httpClient.DefaultRequestHeaders.Add("__clienttype", "unieap");
        httpClient.DefaultRequestHeaders.Add("workitemid", "null");
        httpClient.DefaultRequestHeaders.Add("resourceid", "null");
        httpClient.DefaultRequestHeaders.Add("ContentType", "multipart/form-data");
        httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");

        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage httpResponse = await httpClient.PostAsync("http://uems.sysu.edu.cn/jwxt/KcbcxAction/KcbcxAction.action?method=getList", stringContent);
        httpResponse.EnsureSuccessStatusCode();
        string res = await httpResponse.Content.ReadAsStringAsync();
        // 获取整个table的html内容
        res = res.Substring(res.IndexOf("<"), res.IndexOf("dataStores") - 3 - res.IndexOf("<"));
        var timetable = "<html><head><meta http-equiv=Content-Type content=text/html charset=utf-8>" +
            "<link rel=stylesheet type=text/css href=\"ms-appx-web:///Assets/kb.css\"/>" +
            "</head><body>" + res + "</body></html>";
        table = "<html><head><meta http-equiv=Content-Type content=text/html charset=utf-8><style type='text/css'>#subprinttable1{border: 1px solid #d3dffa;border-collapse: collapse;height:100%;font-size:12px;}#subprinttable1 td {	border: 1px solid #d3dffa;	text-align:center;	}#subprinttable1 td.tab_1 {background-color:#b9cbfa;font: 宋体;}</style>" +
            "</head><body>" + res + "</body></html>";
        webview.NavigateToString(timetable);
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        if (ex.Message.Equals("An error occurred while sending the request.")) {
          await new MessageDialog("应用无法联网，请检查您的网络设置").ShowAsync();
        }
      }
    }

    // 导出课程表具体实现
    private async void exportCourseTable(string xnd, string xq) {
      try {
        string json = @"{
                    body:{
                        dataStores:{},
                        parameters:{
                            args: [""" + xq + @""",""" + xnd + @"""],
                            responseParam = ""rs""
                        }
                    }
                }";
        HttpClient httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("Referer", "http://uems.sysu.edu.cn/jwxt/forward.action?path=/sysu/xk/xskbcx/xskb");
        httpClient.DefaultRequestHeaders.Add("render", "export");
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage httpResponse = await httpClient.PostAsync("http://uems.sysu.edu.cn/jwxt/KcbcxAction/KcbcxAction.action?method=getList", stringContent);
        httpResponse.EnsureSuccessStatusCode();

        var downloadUrl = "http://uems.sysu.edu.cn/jwxt/ExportToExcel.do?method=exportExcel&xq=" + xq + "&xnd=" + xnd;
        httpClient.DefaultRequestHeaders.Referrer = new Uri("http://uems.sysu.edu.cn/jwxt/forward.action?path=/sysu/xk/xskbcx/xskb");
        httpClient.DefaultRequestHeaders.Add("Accept", "image/gif, image/jpeg, image/pjpeg, application/x-ms-application, application/xaml+xml, application/x-ms-xbap, */*");
        httpResponse = await httpClient.GetAsync(downloadUrl);
        httpResponse.EnsureSuccessStatusCode();
        InMemoryRandomAccessStream randomAccess = new InMemoryRandomAccessStream();
        DataWriter writer = new DataWriter(randomAccess.GetOutputStreamAt(0));
        // Write and save the data into the stream
        writer.WriteBytes(await httpResponse.Content.ReadAsByteArrayAsync());
        await writer.StoreAsync();

        var savePicker = new FileSavePicker();
        savePicker.SuggestedStartLocation = PickerLocationId.Desktop;
        savePicker.FileTypeChoices.Add("网页", new List<string>() { ".html" });
        savePicker.SuggestedFileName = xnd + "-" + xq + ".html";
        StorageFile file = await savePicker.PickSaveFileAsync();
        if (file != null) {
          CachedFileManager.DeferUpdates(file);
          await FileIO.WriteTextAsync(file, table);
          //store InMemoryRandomAccessStream into a file
          //using (var fileStream = await file.OpenAsync(FileAccessMode.ReadWrite)) {
          //  await RandomAccessStream.CopyAndCloseAsync(randomAccess.GetInputStreamAt(0), fileStream.GetOutputStreamAt(0));
          //}
          //httpClient.Dispose();
          FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
          if (status == FileUpdateStatus.Complete) {
            await new MessageDialog("课表导出成功!").ShowAsync();
          } else {
            await new MessageDialog("课表导出失败!").ShowAsync();
          }
        } else {
          return;
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        if (ex.Message.Equals("An error occurred while sending the request.")) {
          await new MessageDialog("应用无法联网，请检查您的网络设置").ShowAsync();
        }
      }
    }
  }
}
