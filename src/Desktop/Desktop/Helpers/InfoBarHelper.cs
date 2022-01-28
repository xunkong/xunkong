using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Diagnostics;
using Microsoft.UI.Xaml.Media.Animation;

namespace Xunkong.Desktop.Helpers
{
    internal static class InfoBarHelper
    {

        private static readonly System.Timers.Timer _timer = new(60000);


        private static StackPanel _container;


        public static void Initialize(StackPanel container)
        {
            _container = container;
            _timer.AutoReset = true;
            _timer.Elapsed += _timer_Elapsed;
            _timer.Start();
        }

        private static void _timer_Elapsed(object? sender, System.Timers.ElapsedEventArgs e)
        {
            if (_container is null)
            {
                return;
            }
            _container.DispatcherQueue.TryEnqueue(() =>
            {
                var c = _container.Children;
                Debug.WriteLine(c.Count);
                foreach (var item in c)
                {
                    var size = item.ActualSize.X * item.ActualSize.Y;
                    if (size == 0)
                    {
                        c.Remove(item);
                    }
                }
                Debug.WriteLine(c.Count);
            });

        }

        public static void Information(string message, int delay = 3000)
        {
            _container.DispatcherQueue.TryEnqueue(async () =>
            {
                var bar = new InfoBar
                {
                    Severity = InfoBarSeverity.Informational,
                    Message = message,
                    IsOpen = true,
                };
                _container.Children.Add(bar);
                if (delay > 0)
                {
                    await Task.Delay(delay);
                    bar.IsOpen = false;
                }
            });
        }

        public static void Information(string title, string message, int delay = 3000)
        {
            _container.DispatcherQueue.TryEnqueue(async () =>
           {
               var bar = new InfoBar
               {
                   Severity = InfoBarSeverity.Informational,
                   Title = title,
                   Message = message,
                   IsOpen = true,
               };
               _container.Children.Add(bar);
               if (delay > 0)
               {
                   await Task.Delay(delay);
                   bar.IsOpen = false;
               }
           });

        }


        public static void Success(string message, int delay = 2000)
        {
            var trans = new EntranceThemeTransition();
            _container.DispatcherQueue.TryEnqueue(async () =>
           {
               var bar = new InfoBar
               {
                   Severity = InfoBarSeverity.Success,
                   Message = message,
                   IsOpen = true,
               };
               _container.Children.Add(bar);
               if (delay > 0)
               {
                   await Task.Delay(delay);
                   bar.IsOpen = false;
               }
           });

        }

        public static void Success(string title, string message, int delay = 2000)
        {
            var trans = new EntranceThemeTransition();
            _container.DispatcherQueue.TryEnqueue(async () =>
            {
                var bar = new InfoBar
                {
                    Severity = InfoBarSeverity.Success,
                    Title = title,
                    Message = message,
                    IsOpen = true,
                };
                _container.Children.Add(bar);
                if (delay > 0)
                {
                    await Task.Delay(delay);
                    bar.IsOpen = false;
                }
            });
        }


        public static void Warning(string message)
        {
            _container.DispatcherQueue.TryEnqueue(() =>
            {
                var bar = new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Message = message,
                    IsOpen = true,
                };
                _container.Children.Add(bar);
            });
        }

        public static void Warning(string title, string message)
        {
            _container.DispatcherQueue.TryEnqueue(() =>
            {
                var bar = new InfoBar
                {
                    Severity = InfoBarSeverity.Warning,
                    Title = title,
                    Message = message,
                    IsOpen = true,
                };
                _container.Children.Add(bar);
            });
        }


        public static void Error(string message)
        {
            _container.DispatcherQueue.TryEnqueue(() =>
            {
                var bar = new InfoBar
                {
                    Severity = InfoBarSeverity.Error,
                    Message = message,
                    IsOpen = true,
                };
                _container.Children.Add(bar);

            });
        }

        public static void Error(string title, string message)
        {
            _container.DispatcherQueue.TryEnqueue(() =>
            {
                var bar = new InfoBar
                {
                    Severity = InfoBarSeverity.Error,
                    Title = title,
                    Message = message,
                    IsOpen = true,
                };
                _container.Children.Add(bar);
            });
        }


        public static void Error(Exception ex)
        {
            _container.DispatcherQueue.TryEnqueue(() =>
            {
                var bar = new InfoBar
                {
                    Severity = InfoBarSeverity.Error,
                    Title = ex.GetType().Name,
                    Message = ex.Message,
                    IsOpen = true,
                };
                _container.Children.Add(bar);
            });
        }


        public static void Error(Exception ex, string step)
        {
            _container.DispatcherQueue.TryEnqueue(() =>
            {
                var bar = new InfoBar
                {
                    Severity = InfoBarSeverity.Error,
                    Title = ex.GetType().Name,
                    Message = $"{step}\n{ex.Message}",
                    IsOpen = true,
                };
                _container.Children.Add(bar);
            });
        }


        public static void Show(InfoBar infoBar, int delay = 0)
        {
            _container.DispatcherQueue.TryEnqueue(async () =>
            {
                _container.Children.Add(infoBar);
                if (delay > 0)
                {
                    await Task.Delay(delay);
                    infoBar.IsOpen = false;
                }
            });
        }



    }
}
