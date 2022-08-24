using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Models;
using Titanium.Web.Proxy;
using System.Net;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Xunkong.Desktop.Services;

internal class ProxyService : IDisposable
{


    private readonly ProxyServer _proxy;

    private readonly ExplicitProxyEndPoint _endPoint;

    public event EventHandler<string> GotWishlogUrl;

    public bool ProxyRunning => _proxy.ProxyRunning;


    public ProxyService()
    {
        _proxy = new ProxyServer();
        _endPoint = new ExplicitProxyEndPoint(IPAddress.Any, 10086);
        _proxy.BeforeRequest += ProxyServer_BeforeRequest;
        _proxy.AddEndPoint(_endPoint);
    }



    private Task ProxyServer_BeforeRequest(object sender, Titanium.Web.Proxy.EventArguments.SessionEventArgs e)
    {
        var request = e.HttpClient.Request;
        if ((request.Host == "webstatic.mihoyo.com" || request.Host == "webstatic-sea.mihoyo.com") && request.RequestUri.AbsolutePath == "/hk4e/event/e20190909gacha-v2/index.html")
        {
            GotWishlogUrl(this, request.Url);
        }
        return Task.CompletedTask;
    }


    public void StartProxy()
    {
        if (!ProxyRunning)
        {
            _proxy.Start();
        }
        SetSystemProxy(true);
    }



    public void StopProxy()
    {
        if (ProxyRunning)
        {
            _proxy.Stop();
        }
        SetSystemProxy(false);
    }



    [DllImport("wininet.dll")]
    private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);


    private void SetSystemProxy(bool enable)
    {
        if (enable)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "reg",
                Arguments = $@"add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings"" /v ProxyEnable /t REG_DWORD /d 1 /f",
                CreateNoWindow = true,
            });
            Process.Start(new ProcessStartInfo
            {
                FileName = "reg",
                Arguments = $@"add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings"" /v ProxyServer /d ""127.0.0.1:10086"" /f",
                CreateNoWindow = true,
            });
        }
        else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "reg",
                Arguments = $@"add ""HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings"" /v ProxyEnable /t REG_DWORD /d 0 /f",
                CreateNoWindow = true,
            });
        }
        InternetSetOption(IntPtr.Zero, 39, IntPtr.Zero, 0);
        InternetSetOption(IntPtr.Zero, 37, IntPtr.Zero, 0);
    }



    private bool disposedValue;

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            _proxy.Dispose();
            _proxy.DisableAllSystemProxies();
            disposedValue = true;
        }
    }

    // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
    ~ProxyService()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: false);
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

}
