using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Helpers
{
    internal static class NavigationHelper
    {


        public static void NavigateTo(Type type, object? parameter = null, NavigationTransitionInfo? transitionInfo = null)
        {
            var message = new NavigateMessage(type, parameter, transitionInfo);
            WeakReferenceMessenger.Default.Send(message);
        }


    }
}
