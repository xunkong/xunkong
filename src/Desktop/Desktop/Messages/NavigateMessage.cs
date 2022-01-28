using Microsoft.UI.Xaml.Media.Animation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Messages
{
    internal record NavigateMessage(Type Type, object? Parameter = null, NavigationTransitionInfo? TransitionInfo = null);
}
