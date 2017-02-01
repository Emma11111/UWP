using Windows.UI.Xaml.Navigation;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System;
using HereYou.Model;
using Newtonsoft.Json;
using HereYou.ViewModel;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.Storage;
using System.Collections.Generic;
using Windows.UI.Popups;
using Windows.UI.Core;
using Syncfusion.XlsIO;
using Syncfusion.UI.Xaml.Grid.Converter;

namespace HereYou {
  public sealed partial class Scores {
    private JArray allscores = null;   // 存储大学所有课程成绩
    private ObservableCollection<ScoreItem> items = new ScoreItemViewModel().items;  // 存储要显示的课程成绩
    public Scores() {
      InitializeComponent();
      var titleBar = Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TitleBar;
      titleBar.ForegroundColor = Colors.White;
      titleBar.BackgroundColor = Colors.DodgerBlue;
      titleBar.ButtonBackgroundColor = Colors.DodgerBlue;
      NavigationCacheMode = NavigationCacheMode.Enabled;
    }

    // 导航到该页面时查询大学所有成绩并保存在allscores
    protected override void OnNavigatedTo(NavigationEventArgs e) {
      Frame rootFrame = Window.Current.Content as Frame;
      if (rootFrame != null && rootFrame.CanGoBack) {
        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Visible;
      } else {
        SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
            AppViewBackButtonVisibility.Collapsed;
      }
      getScore();
    }

    // 查询成绩
    private async void getScore() {
      try {
        string json = @"{
          body:{
            dataStores:{
              kccjStore:{
                rowSet:{
                  primary:[]
                },
                rowSetName:""pojo_com.neusoft.education.sysu.xscj.xscjcx.model.KccjModel""
              }
            },
            parameters:{
              args: [""student""]
            }
          }
        }";
        var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("__clienttype", "unieap");
        httpClient.DefaultRequestHeaders.Add("render", "unieap");
        httpClient.DefaultRequestHeaders.Add("workitemid", "null");
        httpClient.DefaultRequestHeaders.Add("resourceid", "null");
        httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
        httpClient.DefaultRequestHeaders.Add("ajaxRequest", "true");
        httpClient.DefaultRequestHeaders.Add("ContentType", "multipart/form-data");
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage httpResponse = await httpClient.PostAsync("http://uems.sysu.edu.cn/jwxt/xscjcxAction/xscjcxAction.action?method=getKccjList", stringContent);
        httpResponse.EnsureSuccessStatusCode();
        string res = await httpResponse.Content.ReadAsStringAsync();
        res = res.Substring(res.IndexOf('['), res.IndexOf(']') - res.IndexOf('[') + 1);
        allscores = JsonConvert.DeserializeObject(res) as Newtonsoft.Json.Linq.JArray;
        showScore();
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
        if (ex.Message.Equals("An error occurred while sending the request.")) {
          await new MessageDialog("应用无法联网，请检查您的网络设置").ShowAsync();
        }
      }
    }

    // 过滤并显示成绩
    private void showScore() {
      items.Clear();
      string _xnd = (xnd.SelectedItem as ComboBoxItem).Content.ToString();
      string _xq = (xq.SelectedItem as ComboBoxItem).Content.ToString();
      foreach (var item in allscores) {
        string jsxm = item["jsxm"] != null ? item["jsxm"].ToString() : "";
        if (item["xnd"].ToString() == _xnd && (item["xq"].ToString() == _xq || _xq == "学年")) {
          items.Add(new ScoreItem(
            item["kcmc"].ToString(),
            item["kclb"].ToString(),
            jsxm,
           item["xf"].ToString(),
            item["zzcj"].ToString(),
           item["jd"].ToString(),
            item["jxbpm"].ToString())
          );
        }
      }
    }

    //改变学年度和学期就更新显示
    private void selectXndXq(object sender, object e) {
      showScore();
    }

    // 导出成绩
    private async void exportScore(object sender, RoutedEventArgs e) {
      var options = new ExcelExportingOptions();
      options.ExcelVersion = ExcelVersion.Excel97to2003;
      var excelEngine = scoreGrid.ExportToExcel(scoreGrid.View, options);
      var workBook = excelEngine.Excel.Workbooks[0];
      // 选择文件保存的位置和名称
      string _xnd = (xnd.SelectedItem as ComboBoxItem).Content.ToString();
      string _xq = (xq.SelectedItem as ComboBoxItem).Content.ToString();
      var savePicker = new Windows.Storage.Pickers.FileSavePicker();
      savePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Desktop;
      savePicker.FileTypeChoices.Add("Excel", new List<string>() { ".xls" });
      savePicker.SuggestedFileName = _xnd + "-" + _xq + ".xls";
      StorageFile file = await savePicker.PickSaveFileAsync();

      if (file != null) {
        CachedFileManager.DeferUpdates(file);
        await workBook.SaveAsAsync(file);
        Windows.Storage.Provider.FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
        if (status == Windows.Storage.Provider.FileUpdateStatus.Complete) {
          await new MessageDialog("成绩导出成功!").ShowAsync();
        } else {
          await new MessageDialog("成绩导出失败!").ShowAsync();
        }
      } else
        return;
    }
  }
}
