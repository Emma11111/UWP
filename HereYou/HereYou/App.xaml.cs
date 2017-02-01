using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Core;
using SQLitePCL;
using System.Diagnostics;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI;

namespace HereYou {
  sealed partial class App {
    public bool isLogin = false;
    public bool IsSuspending = false;
    public Frame myframe;
    public static SQLiteConnection conn;
    public App() {
      InitializeComponent();
      Suspending += OnSuspending;
      this.Resuming += OnResuming;
      try {
        LoadDatabase();
      } catch (Exception ex) {
        Debug.WriteLine(ex.Message);
      }
    }
    private void LoadDatabase() { // Get a reference to the SQLite database
      conn = new SQLiteConnection("Todos.db", SQLiteOpen.READWRITE);
      string sql = @"CREATE TABLE IF NOT EXISTS [Todo](
                    [ID] INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
                    [date] VARCHAR(30),
                    [imgname] VARCHAR(140), 
                    [title] VARCHAR(140), 
                    [detail] VARCHAR(140), 
                    [finish] VARCHAR(10)
                  );";
      using (var statement = conn.Prepare(sql)) { statement.Step(); }
    }
    /// <summary>
    /// 在应用程序由最终用户正常启动时进行调用。
    /// 将在启动应用程序以打开特定文件等情况下使用。
    /// </summary>
    /// <param name="e">有关启动请求和过程的详细信息。</param>
    protected override void OnLaunched(LaunchActivatedEventArgs e) {

#if DEBUG
      if (System.Diagnostics.Debugger.IsAttached) {
        this.DebugSettings.EnableFrameRateCounter = true;
      }
#endif
      //// How did the app exit the last time it was run (if at all)
      //ApplicationExecutionState previousState = e.PreviousExecutionState;
      //// What kind of launch is this?
      //ActivationKind activationKind = e.Kind;
      Frame rootFrame = Window.Current.Content as Frame;

      // 不要在窗口已包含内容时重复应用程序初始化，
      // 只需确保窗口处于活动状态
      if (rootFrame == null) {
        // 创建要充当导航上下文的框架，并导航到第一页
        rootFrame = new Frame();
        rootFrame.NavigationFailed += OnNavigationFailed;
        if (e.PreviousExecutionState == ApplicationExecutionState.Terminated) {
          //TODO: 从之前挂起的应用程序加载状态
          if (ApplicationData.Current.LocalSettings.Values.ContainsKey("NavigationState")) {
            rootFrame.SetNavigationState((string)ApplicationData.Current.LocalSettings.Values["NavigationState"]);
          }
        }
        // 将框架放在当前窗口中
        Window.Current.Content = rootFrame;
      }

      if (rootFrame.Content == null) {
        // 当导航堆栈尚未还原时，导航到第一页，
        // 并通过将所需信息作为导航参数传入来配置
        // 参数
        rootFrame.Navigate(typeof(Shell), "todo");
      }
      // 确保当前窗口处于活动状态
      Window.Current.Activate();

      SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

      rootFrame.Navigated += (s, a) => {
        if (rootFrame != null && rootFrame.CanGoBack) {
          // Setting this visible is ignored on Mobile and when in tablet mode!     
          SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        } else {
          SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }
      };
    }

    /// <summary>
    /// Invoked when Navigation to a certain page fails
    /// </summary>
    /// <param name="sender">The Frame which failed navigation</param>
    /// <param name="e">Details about the navigation failure</param>
    void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
      throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    /// <summary>
    /// Invoked when application execution is being suspended.  Application state is saved
    /// without knowing whether the application will be terminated or resumed with the contents
    /// of memory still intact.
    /// </summary>
    /// <param name="sender">The source of the suspend request.</param>
    /// <param name="e">Details about the suspend request.</param>
    private void OnSuspending(object sender, SuspendingEventArgs e) {
      //TODO: 保存应用程序状态并停止任何后台活动
      // You MUST ensure the app's permanent data is safely stored at this point
      // Set this flag - this happens before the page navigatedfrom
      var deferral = e.SuspendingOperation.GetDeferral();
      IsSuspending = true;

      // Get the frame navigation state serialized as a string and save in settings
      Frame frame = Window.Current.Content as Frame;
      ApplicationData.Current.LocalSettings.Values["NavigationState"] = frame.GetNavigationState();
      deferral.Complete();
    }
    private void OnResuming(object sender, object e) {
      // TODO: whatever you need to do to resume your app
      // Clear the IsSuspending flag
      IsSuspending = false;
    }
    public event EventHandler<BackRequestedEventArgs> BackRequested;
    private void OnBackRequested(object sender, BackRequestedEventArgs e) {
      // Fire the event - allows pages/Viewmodels to override/augment default back navigation behavior
      if (BackRequested != null) {
        BackRequested(this, e);
      }
      if (!e.Handled) {
        Frame frame = Window.Current.Content as Frame;
        if (myframe != null && myframe.CanGoBack && (myframe.SourcePageType == typeof(HereYou.AddTodo) || myframe.SourcePageType == typeof(HereYou.CommentPage) || myframe.SourcePageType == typeof(HereYou.SearchPage))) {
          myframe.GoBack();
          e.Handled = true;
        } else if (frame.CanGoBack) {
          frame.GoBack();
          e.Handled = true;
        }
      }
    }
  }
}
