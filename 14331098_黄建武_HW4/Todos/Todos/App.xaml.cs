﻿using System;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace Todos {
  /// <summary>
  /// 提供特定于应用程序的行为，以补充默认的应用程序类。
  /// </summary>
  sealed partial class App : Application {
    /// <summary>
    /// 初始化单一实例应用程序对象。这是执行的创作代码的第一行，
    /// 已执行，逻辑上等同于 main() 或 WinMain()。
    /// </summary>
    public bool IsSuspending = false;

    public App() {
      Microsoft.ApplicationInsights.WindowsAppInitializer.InitializeAsync(
          Microsoft.ApplicationInsights.WindowsCollectors.Metadata |
          Microsoft.ApplicationInsights.WindowsCollectors.Session);
      this.InitializeComponent();
      this.Suspending += OnSuspending;
      this.Resuming += OnResuming;
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
        rootFrame.Navigate(typeof(MainPage), e.Arguments);
      }
      // 确保当前窗口处于活动状态
      Window.Current.Activate();

      SystemNavigationManager.GetForCurrentView().BackRequested += OnBackRequested;

      rootFrame.Navigated += (s, a) => {
        if (rootFrame.CanGoBack) {
          // Setting this visible is ignored on Mobile and when in tablet mode!     
          SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Visible;
        } else {
          SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility = AppViewBackButtonVisibility.Collapsed;
        }
      };
    }

    /// <summary>
    /// 导航到特定页失败时调用
    /// </summary>
    ///<param name="sender">导航失败的框架</param>
    ///<param name="e">有关导航失败的详细信息</param>
    void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
      throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
    }

    /// <summary>
    /// 在将要挂起应用程序执行时调用。  在不知道应用程序
    /// 无需知道应用程序会被终止还是会恢复，
    /// 并让内存内容保持不变。
    /// </summary>
    /// <param name="sender">挂起的请求的源。</param>
    /// <param name="e">有关挂起请求的详细信息。</param>
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
        if (frame.CanGoBack) {
          frame.GoBack();
          e.Handled = true;
        }
      }
    }
  }
}
