using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Helpers
{
    internal static class NavigationHelper
    {

        public static NavigationView NavigationView { get; private set; }


        public static Frame RootFrame { get; private set; }


        public static void Initialize(NavigationView navigationView,Frame frame)
        {
            NavigationView = navigationView;
            RootFrame = frame;
        }



    }
}
