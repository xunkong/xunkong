using Microsoft.Win32;
using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.Models;

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
        _endPoint = new ExplicitProxyEndPoint(IPAddress.Loopback, 10086);
        _proxy.BeforeRequest += ProxyServer_BeforeRequest;
        _proxy.AddEndPoint(_endPoint);
    }



    private Task ProxyServer_BeforeRequest(object sender, Titanium.Web.Proxy.EventArguments.SessionEventArgs e)
    {
        var request = e.HttpClient.Request;
        Debug.WriteLine(request.Url);
        if (request.Host is "webstatic.mihoyo.com" or "webstatic-sea.hoyoverse.com" && request.RequestUri.AbsolutePath is "/hk4e/event/e20190909gacha-v2/index.html" or "/genshin/event/e20190909gacha-v2/index.html")
        {
            GotWishlogUrl(this, request.Url);
        }
        return Task.CompletedTask;
    }


    public bool StartProxy()
    {
        bool result = false;
        if (!ProxyRunning)
        {
            _proxy.Start();
            result = true;
        }
        SetSystemProxy(true);
        return result;
    }



    public bool StopProxy()
    {
        bool result = false;
        if (ProxyRunning)
        {
            _proxy.Stop();
            result = true;
        }
        SetSystemProxy(false);
        return result;
    }



    [DllImport("wininet.dll")]
    private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);


    private void SetSystemProxy(bool enable)
    {
        // 参考注册表虚拟化 https://docs.microsoft.com/zh-cn/windows/msix/desktop/flexible-virtualization
        if (enable)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "reg",
                Arguments = """add "HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings" /v ProxyEnable /t REG_DWORD /d 1 /f""",
                CreateNoWindow = true,
            })?.WaitForExit();
            Process.Start(new ProcessStartInfo
            {
                FileName = "reg",
                Arguments = """add "HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings" /v ProxyServer /d "http=localhost:10086;https=localhost:10086" /f""",
                CreateNoWindow = true,
            })?.WaitForExit();
        }
        else
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "reg",
                Arguments = """add "HKCU\Software\Microsoft\Windows\CurrentVersion\Internet Settings" /v ProxyEnable /t REG_DWORD /d 0 /f""",
                CreateNoWindow = true,
            })?.WaitForExit();
        }
        InternetSetOption(IntPtr.Zero, 39, IntPtr.Zero, 0);
        InternetSetOption(IntPtr.Zero, 37, IntPtr.Zero, 0);
    }



    public bool CheckSystemProxy()
    {
        var enable = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxyEnable", 0) is 1;
        var server = Registry.GetValue(@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Internet Settings", "ProxyServer", "") is "http=localhost:10086;https=localhost:10086";
        return enable && server;
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
            if (CheckSystemProxy())
            {
                SetSystemProxy(false);
            }
            _proxy.Dispose();
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
