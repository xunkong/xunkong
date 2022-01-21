using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls
{
    public sealed partial class WebToolContentControl : UserControl
    {

        private readonly WebToolItem _webToolItem;


        //public WebToolContentControl()
        //{
        //    this.InitializeComponent();
        //    Loaded += WebToolContentControl_LoadedAsync;
        //}


        internal WebToolContentControl(WebToolItem item)
        {
            this.InitializeComponent();
            Loaded += WebToolContentControl_LoadedAsync;
            _webToolItem = item;
            _WebView2.Source = new Uri(item.Url);
        }

        private async void WebToolContentControl_LoadedAsync(object sender, RoutedEventArgs e)
        {
            await _WebView2.EnsureCoreWebView2Async();
            _WebView2.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            _WebView2.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }


        private void CoreWebView2_NewWindowRequested(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NewWindowRequestedEventArgs args)
        {
            args.Handled = true;
        }


        private async void CoreWebView2_NavigationCompleted(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs args)
        {
            if (args.IsSuccess && !string.IsNullOrWhiteSpace(_webToolItem?.JavaScript))
            {
                await _WebView2.CoreWebView2.ExecuteScriptAsync(_webToolItem.JavaScript);
            }
        }


        public void NavigateTo(string url)
        {
            _WebView2.Source = new Uri(url);
        }



    }
}
