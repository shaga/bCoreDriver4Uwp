using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using bCoreDriver.ViewModels;
using LibBcore;

// 空白ページのアイテム テンプレートについては、http://go.microsoft.com/fwlink/?LinkId=234238 を参照してください

namespace bCoreDriver.Views
{
    /// <summary>
    /// それ自体で使用できる空白ページまたはフレーム内に移動できる空白ページ。
    /// </summary>
    public sealed partial class ControlPage : Page
    {
        public ControlPage()
        {
            this.InitializeComponent();
            NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            App.SetAppBackButtonVisibility();

            var info = e.Parameter as DeviceInformation;

            var viewModel = DataContext as ControlPageViewModel;

            if (info == null || viewModel == null) return;

            viewModel.Init(info);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            var viewModel = DataContext as ControlPageViewModel;

            if (viewModel == null) return;

            if (e.Parameter is BcoreManager) return;

            var frame = Window.Current.Content as Frame;

            viewModel.Finish();

            if (frame != null)
            {
                var size = frame.CacheSize;
                frame.CacheSize = 0;
                frame.CacheSize = size;
            }
        }
    }
}
