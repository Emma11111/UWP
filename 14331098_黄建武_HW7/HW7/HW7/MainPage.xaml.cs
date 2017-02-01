using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Net.Http;
using System.Diagnostics;
using Newtonsoft.Json;
using Windows.UI.Popups;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using System.Text.RegularExpressions;
using System.Text;
using System.Collections.ObjectModel;

//“空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409 上有介绍

namespace HW7 {
  /// <summary>
  /// 可用于自身或导航至 Frame 内部的空白页。
  /// </summary>
  public sealed partial class MainPage : Page {
    //Create an HTTP client object
    HttpClient httpClient = new HttpClient();
    ObservableCollection<Item> score = new ObservableCollection<Item>();
    string rno = "";  // 登录教务系统的隐藏域
    public MainPage() {
      this.InitializeComponent();
      getMD5();
      //添加头部，才能请求到成绩
      httpClient.DefaultRequestHeaders.Add("__clienttype", "unieap");
      httpClient.DefaultRequestHeaders.Add("render", "unieap");
      httpClient.DefaultRequestHeaders.Add("workitemid", "null");
      httpClient.DefaultRequestHeaders.Add("resourceid", "null");
      httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
      httpClient.DefaultRequestHeaders.Add("ajaxRequest", "true");
      httpClient.DefaultRequestHeaders.Add("ContentType", "multipart/form-data");
    }

    // 将密码进行MD5加密
    private string getMD5() {
      //可以选择MD5 Sha1 Sha256 Sha384 Sha512
      string strAlgName = HashAlgorithmNames.Md5;
      // 创建一个 HashAlgorithmProvider 对象
      HashAlgorithmProvider objAlgProv = HashAlgorithmProvider.OpenAlgorithm(strAlgName);
      // 创建一个可重用的CryptographicHash对象           
      CryptographicHash objHash = objAlgProv.CreateHash();

      string psw = password.Password;
      IBuffer buff = CryptographicBuffer.ConvertStringToBinary(psw, BinaryStringEncoding.Utf16LE);
      objHash.Append(buff);
      IBuffer buffHash = objHash.GetValueAndReset();
      string strHash = CryptographicBuffer.EncodeToHexString(buffHash);
      Debug.WriteLine(strHash);
      return strHash.ToUpper();
    }

    //获取验证码
    private async void getCode() {
      Uri requestUri = new Uri(@"http://uems.sysu.edu.cn/jwxt");
      Uri codeurl = new Uri(@"http://uems.sysu.edu.cn/jwxt/jcaptcha");
      try {
        //先访问教务系统获取cookie
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
        // A memory stream where write the image data
        InMemoryRandomAccessStream randomAccess = new InMemoryRandomAccessStream();
        DataWriter writer = new DataWriter(randomAccess.GetOutputStreamAt(0));
        // Write and save the data into the stream
        writer.WriteBytes(await httpResponse.Content.ReadAsByteArrayAsync());
        await writer.StoreAsync();
        BitmapImage bm = new BitmapImage();
        await bm.SetSourceAsync(randomAccess);
        j_codepic.Source = bm;
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
    }

    private async void queryWeather(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
      if (queryweather.Text == "") {
        await new MessageDialog("城市名不能为空！").ShowAsync();
        return;
      }

      Uri requestUri = new Uri(@"http://www.thinkpage.cn/weatherapi/getWeather?city=" + queryweather.Text.Trim() + "&language=zh-CHS&provider=SMART&unit=C&aqi=city");
      try {
        //Send the GET request
        HttpResponseMessage httpResponse = await httpClient.GetAsync(requestUri);
        httpResponse.EnsureSuccessStatusCode();

        var httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
        var body = new Dictionary<string, object>();
        body = JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResponseBody);
        var arr = body["Weathers"] as Newtonsoft.Json.Linq.JArray;
        try {
          var obj = arr[0]["Forecast"][0];
          date_day.Text = obj["Date"].ToString().Substring(5) + " " + obj["Day"];
          weatherPic.Source = new BitmapImage(new Uri("ms-appx:///Assets/weather/" + obj["Code"] + ".png"));
          weather.Text = obj["Text"].ToString();
          temperature.Text = obj["Low"] + "℃ ~ " + obj["High"] + "℃";
          weather_detail.Visibility = Visibility.Visible;
        } catch (Exception) {
          queryweather.Text = "";
          weather_detail.Visibility = Visibility.Collapsed;
          await new MessageDialog("找不到该城市的天气数据！请重新输入").ShowAsync();
          return;
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
    }

    private async void queryPhone(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args) {
      if (queryphone.Text == "") {
        await new MessageDialog("手机号码不能为空！").ShowAsync();
        return;
      }
      Uri requestUri = new Uri(@"http://opendata.baidu.com/api.php?query=" + queryphone.Text.Trim() + "&co=&resource_id=6004&t=1460125093359&ie=utf8&oe=gbk&cb=op_aladdin_callback&format=json&tn=baidu&cb=jQuery11020106542830829915_1460112569047&_=1460112569072");
      try {
        //Send the GET request
        Windows.Web.Http.HttpClient httpclient = new Windows.Web.Http.HttpClient();
        var httpResponse = await httpclient.GetAsync(requestUri);
        httpResponse.EnsureSuccessStatusCode();
        var httpResponseBody = await httpResponse.Content.ReadAsStringAsync();
        int start = httpResponseBody.IndexOf('{'), end = httpResponseBody.LastIndexOf(')');
        httpResponseBody = httpResponseBody.Substring(start, end - start);
        var body = new Dictionary<string, object>();
        body = JsonConvert.DeserializeObject<Dictionary<string, object>>(httpResponseBody);
        var arr = body["data"] as Newtonsoft.Json.Linq.JArray;
        try {
          var obj = arr[0];
          position.Text = obj["prov"] + " " + obj["city"];
          phonetype.Text = obj["type"].ToString();
          phonenum.Text = queryphone.Text.Trim();
          phone_detail.Visibility = Visibility.Visible;
        } catch (Exception) {
          queryphone.Text = "";
          phone_detail.Visibility = Visibility.Collapsed;
          await new MessageDialog("该号码不存在！请重新输入").ShowAsync();
          return;
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
    }

    private void check_weather(object sender, RoutedEventArgs e) {
      tableHeader.Visibility = Visibility.Collapsed;
      phone_detail.Visibility = Visibility.Collapsed;
      login.Visibility = Visibility.Collapsed;
      score_detail.Visibility = Visibility.Collapsed;
      queryphone.Visibility = Visibility.Collapsed;
      queryweather.Text = "";
      queryweather.Visibility = Visibility.Visible;
      queryweather.Focus(FocusState.Programmatic);
    }

    private void check_phone(object sender, RoutedEventArgs e) {
      tableHeader.Visibility = Visibility.Collapsed;
      weather_detail.Visibility = Visibility.Collapsed;
      login.Visibility = Visibility.Collapsed;
      score_detail.Visibility = Visibility.Collapsed;
      queryweather.Visibility = Visibility.Collapsed;
      queryphone.Text = "";
      queryphone.Visibility = Visibility.Visible;
      queryphone.Focus(FocusState.Programmatic);
    }

    private void check_score(object sender, RoutedEventArgs e) {
      weather_detail.Visibility = Visibility.Collapsed;
      phone_detail.Visibility = Visibility.Collapsed;
      queryweather.Visibility = Visibility.Collapsed;
      queryphone.Visibility = Visibility.Collapsed;
      login.Visibility = Visibility.Visible;
      stuNum.Focus(FocusState.Programmatic);
      getCode();
    }

    private async void postSignin(object sender, RoutedEventArgs e) {
      Uri signin = new Uri(@"http://uems.sysu.edu.cn/jwxt/j_unieap_security_check.do");
      try {
        var postData = new List<KeyValuePair<string, string>>();
        postData.Add(new KeyValuePair<string, string>("j_username", stuNum.Text.Trim()));
        postData.Add(new KeyValuePair<string, string>("j_password", getMD5()));
        postData.Add(new KeyValuePair<string, string>("rno", rno));
        postData.Add(new KeyValuePair<string, string>("jcaptcha_response", j_code.Text.Trim()));
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
          getScore();
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
    }

    private async void getScore() {
      try {
        string json = @"{
          body:{
            dataStores:{
              kccjStore:{
                rowSet:{
                  primary:[],
                  filter:[],
                  delete:[]
                },
                name:""kccjStore"",
                rowSetName:""pojo_com.neusoft.education.sysu.xscj.xscjcx.model.KccjModel""
              }
            },
            parameters:{
              args: [""student""]
            }
          }
        }";
        var stringContent = new StringContent(json, Encoding.UTF8, "application/json");
        HttpResponseMessage httpResponse = await httpClient.PostAsync("http://uems.sysu.edu.cn/jwxt/xscjcxAction/xscjcxAction.action?method=getKccjList", stringContent);
        httpResponse.EnsureSuccessStatusCode();
        string res = await httpResponse.Content.ReadAsStringAsync();
        res = res.Substring(res.IndexOf('['), res.IndexOf(']') - res.IndexOf('[') + 1);
        var arr = JsonConvert.DeserializeObject(res) as Newtonsoft.Json.Linq.JArray;
        score.Clear();
        login.Visibility = Visibility.Collapsed;
        j_code.Text = "";
        tableHeader.Visibility = Visibility.Visible;
        score_detail.Visibility = Visibility.Visible;
        foreach (var item in arr) {
          if (item["xnd"].ToString() == "2015-2016" && item["xq"].ToString() == "2") {
            score.Add(new Item(item["jxbpm"].ToString(), item["zpcj"].ToString(), item["kcmc"].ToString(), item["jd"].ToString()));
          }
        }
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
    }

    private void freshCodePic(object sender, RoutedEventArgs e) {
      getCode();
    }
  }
}
