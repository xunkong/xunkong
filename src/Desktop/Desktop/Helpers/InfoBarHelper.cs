using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;
using System.Diagnostics;

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

        public static void Information(string? message)
        {
            _container.DispatcherQueue.TryEnqueue(() =>
            {
                var bar = new InfoBar
                {
                    Severity = InfoBarSeverity.Informational,
                    Message = message,
                    IsOpen = true,
                };
                _container.Children.Add(bar);

            });
        }

        public static void Information(string? title, string? message)
        {
            _container.DispatcherQueue.TryEnqueue(() =>
            {
                var bar = new InfoBar
                {
                    Severity = InfoBarSeverity.Informational,
                    Title = title,
                    Message = message,
                    IsOpen = true,
                };
                _container.Children.Add(bar);
            });

        }


        public static void Success(string? message, TimeSpan? timeSpan = null)
        {
            _container.DispatcherQueue.TryEnqueue(async () =>
           {
               var bar = new InfoBar
               {
                   Severity = InfoBarSeverity.Success,
                   Message = message,
                   IsOpen = true,
               };
               _container.Children.Add(bar);
               if (timeSpan is null)
               {
                   await Task.Delay(2000);
               }
               else
               {
                   await Task.Delay((int)timeSpan.Value.TotalMilliseconds);
               }
               bar.IsOpen = false;
           });

        }

        public static void Success(string? title, string? message)
        {
            _container.DispatcherQueue.TryEnqueue(() =>
            {
                var bar = new InfoBar
                {
                    Severity = InfoBarSeverity.Success,
                    Title = title,
                    Message = message,
                    IsOpen = true,
                };
                _container.Children.Add(bar);
            });
        }


        public static void Warning(string? message)
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

        public static void Warning(string? title, string? message)
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


        public static void Error(string? message)
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

        public static void Error(string? title, string? message)
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


    }
}
