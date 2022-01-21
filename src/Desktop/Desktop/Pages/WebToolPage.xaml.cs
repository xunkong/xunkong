#pragma warning disable CA1416 // 验证平台兼容性
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
using Microsoft.Web.WebView2.Core;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class WebToolPage : Page
    {


        private readonly Dictionary<int, WebToolContentControl> _contentDictionary = new();

        private int _lastItemId;

        public WebToolPage()
        {
            this.InitializeComponent();
        }




        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter is WebToolItem webToolItem)
            {
                var id = webToolItem.Id;
                if (id == _lastItemId)
                {
                    return;
                }
                if (_contentDictionary.TryGetValue(id, out var control))
                {
                    _webContent.Content = control;
                    _lastItemId = id;
                }
                else
                {
                    control = new WebToolContentControl(webToolItem);
                    _webContent.Content = control;
                    _contentDictionary.Add(id, control);
                    _lastItemId = id;
                }
            }

        }




    }
}
