using System;
using Windows.Storage.Pickers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Popups;
using Windows.UI.Text;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Core;

// “空白页”项模板在 http://go.microsoft.com/fwlink/?LinkId=234238 上提供

namespace Todos {
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class AddTodo : Page {
        public AddTodo() {
            this.InitializeComponent();
        }

        private async void Create_Click(object sender, RoutedEventArgs e) {
            string alert = "";
            ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
            if (title.Text.Trim() == "")
                alert += "Title can't be empty!\n";
            if (range.Text.Trim() == "")
                alert += "Detail can't be empty!\n";
            if (ddl.Date.Date.CompareTo(DateTime.Today) < 0)
                alert += "The due date has passed!";
            if (alert != "")
                await new MessageDialog(alert).ShowAsync();
        }

        private void Cancle_Click(object sender, RoutedEventArgs e) {
            title.Text = "";
            ITextRange range = detail.Document.GetRange(0, TextConstants.MaxUnitCount);
            range.Text = "";
            ddl.Date = DateTime.Today;

            BitmapImage bitmapImage = new BitmapImage();
            bitmapImage.UriSource = new Uri(pic.BaseUri, "Assets/todo.png");
            pic.Source = bitmapImage;
        }

        private async void selectPic(object sender, RoutedEventArgs e) {
            var fop = new FileOpenPicker();
            fop.ViewMode = PickerViewMode.Thumbnail;
            fop.SuggestedStartLocation = PickerLocationId.PicturesLibrary;
            fop.FileTypeFilter.Add(".jpg");
            fop.FileTypeFilter.Add(".jpeg");
            fop.FileTypeFilter.Add(".png");
            fop.FileTypeFilter.Add(".gif");

            Windows.Storage.StorageFile file = await fop.PickSingleFileAsync();
            try {
                using (IRandomAccessStream fileStream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read)) {
                    BitmapImage bitmapImage = new BitmapImage();
                    await bitmapImage.SetSourceAsync(fileStream);
                    pic.Source = bitmapImage;
                }
            } catch (Exception) {
                return;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e) {
            SystemNavigationManager.GetForCurrentView().AppViewBackButtonVisibility =
                AppViewBackButtonVisibility.Visible;
        }
    }
}