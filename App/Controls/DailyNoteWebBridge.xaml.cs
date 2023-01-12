// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Xunkong.Hoyolab.Account;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;


// https://github.com/DGP-Studio/Snap.Hutao/blob/main/src/Snap.Hutao/Snap.Hutao/Web/Bridge/MiHoYoJSInterface.cs

public sealed partial class DailyNoteWebBridge : UserControl
{

    private const string AppVersion = "2.43.1";

    private static string DeviceId = Guid.NewGuid().ToString("D");

    private const string MiHoYoJSInterface = """
        window.MiHoYoJSInterface = {
            postMessage: function(arg) { chrome.webview.postMessage(arg) },
            closePage: function() { this.postMessage('{"method":"closePage"}') },
        };
        """;


    private const string HideScrollBarScript = """
        let st = document.createElement('style');
        st.innerHTML = '::-webkit-scrollbar{display:none}';
        document.querySelector('body').appendChild(st);
        """;


    private bool loaded;

    private HoyolabUserInfo user;

    private GenshinRoleInfo role;

    private Dictionary<string, string> cookieDic = new();


    public DailyNoteWebBridge(HoyolabUserInfo user, GenshinRoleInfo role)
    {
        this.InitializeComponent();
        this.user = user;
        this.role = role;
        Loaded += MihoyoWebBridge_Loaded;
        Unloaded += MihoyoWebBridge_Unloaded;
    }



    private async void MihoyoWebBridge_Loaded(object sender, RoutedEventArgs e)
    {
        if (loaded)
        {
            return;
        }
        try
        {
            await webview2.EnsureCoreWebView2Async();
            var coreWebView2 = webview2.CoreWebView2;
            coreWebView2.Settings.UserAgent = $"Mozilla/5.0 (Linux; Android 12) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/106.0.5249.126 Mobile Safari/537.36 miHoYoBBS/{AppVersion}";
            var manager = coreWebView2.CookieManager;
            var cookies = await manager.GetCookiesAsync("https://webstatic.mihoyo.com");
            foreach (var cookie in cookies)
            {
                manager.DeleteCookie(cookie);
            }

            await Task.Delay(60);
            ParseCookie();
            foreach (var cookie in cookieDic)
            {
                manager.AddOrUpdateCookie(manager.CreateCookie(cookie.Key, cookie.Value, ".mihoyo.com", "/"));
            }

            coreWebView2.NavigationStarting += Corewebview2_NavigationStarting;
            coreWebView2.DOMContentLoaded += Corewebview2_DOMContentLoaded;
            coreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            coreWebView2.Navigate($"https://webstatic.mihoyo.com/app/community-game-records/index.html?bbs_presentation_style=fullscreen#/ys/daily?role_id={role.Uid}&server={role.Region}");

            loaded = true;
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }


    private async void MihoyoWebBridge_Unloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            await webview2.CoreWebView2.TrySuspendAsync();
        }
        catch { }
    }


    private void ParseCookie()
    {
        var cookies = user.Cookie?.Split(';');
        if (cookies != null)
        {
            foreach (var item in cookies)
            {
                if (item.Contains('='))
                {
                    var key = item.Split("=")[0].Trim();
                    var value = item.Split("=")[1].Trim();
                    if (!string.IsNullOrWhiteSpace(key) && !string.IsNullOrWhiteSpace(value))
                    {
                        cookieDic[key] = value;
                    }
                }
            }
        }
    }



    private async void Corewebview2_NavigationStarting(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationStartingEventArgs args)
    {
        try
        {
            await webview2.CoreWebView2.AddScriptToExecuteOnDocumentCreatedAsync(MiHoYoJSInterface);
            await webview2.ExecuteScriptAsync(MiHoYoJSInterface);
        }
        catch { }
    }


    private async void Corewebview2_DOMContentLoaded(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2DOMContentLoadedEventArgs args)
    {
        try
        {
            await webview2.ExecuteScriptAsync(HideScrollBarScript);
        }
        catch { }
    }


    private async void CoreWebView2_WebMessageReceived(Microsoft.Web.WebView2.Core.CoreWebView2 sender, Microsoft.Web.WebView2.Core.CoreWebView2WebMessageReceivedEventArgs args)
    {
        try
        {
            string message = args.TryGetWebMessageAsString();
            Debug.WriteLine(message);
            JsParam param = JsonSerializer.Deserialize<JsParam>(message)!;

            JsResult? result = param.Method switch
            {
                "closePage" => null,
                "configure_share" => null,
                "eventTrack" => null,
                //"getActionTicket" => await GetActionTicketAsync(param).ConfigureAwait(false),
                "getCookieInfo" => GetCookieInfo(param),
                //"getCookieToken" => await GetCookieTokenAsync(param).ConfigureAwait(false),
                "getDS" => GetDynamicSecrectV1(param),
                "getDS2" => GetDynamicSecrectV2(param),
                "getHTTPRequestHeaders" => GetHttpRequestHeader(param),
                "getStatusBarHeight" => GetStatusBarHeight(param),
                "getUserInfo" => GetUserInfo(param),
                "hideLoading" => null,
                "login" => ResetState(param),
                "pushPage" => PushPage(param),
                "showLoading" => null,
                _ => null,
            };

            await CallbackAsync(param.Callback, result);
        }
        catch (Exception ex)
        {

        }
    }



    private async Task CallbackAsync(string? callback, JsResult? result)
    {
        if (callback == null)
        {
            return;
        }
        var js = $"""
            javascript:mhyWebBridge("{callback}"{(result == null ? "" : "," + result.ToString())})
            """;

        await webview2.ExecuteScriptAsync(js);
    }


    private JsResult? GetDynamicSecrectV2(JsParam param)
    {
        const string ApiSalt2 = "xV8v4Qu54lUKrEYFZkJhB8cuOh9Asafs";

        int t = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string r = Random.Shared.Next(100000, 200000).ToString();
        var d = JsonSerializer.Deserialize<Dictionary<string, object>>(param.Payload?["query"]);
        string? b = param.Payload?["body"]?.ToString();
        string? q = null;
        if (d?.Any() ?? false)
        {
            q = string.Join('&', d.OrderBy(x => x.Key).Select(x => $"{x.Key}={x.Value}"));
        }
        q = q?.Replace("True", "true").Replace("False", "false");
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes($"salt={ApiSalt2}&t={t}&r={r}&b={b}&q={q}"));
        var check = Convert.ToHexString(bytes).ToLower();
        string result = $"{t},{r},{check}";

        return new()
        {
            Data = new()
            {
                ["DS"] = result,
            }
        };
    }

    private JsResult? ResetState(JsParam param)
    {
        loaded = false;
        return null;
    }


    private JsResult? PushPage(JsParam param)
    {
        webview2.CoreWebView2.Navigate(param.Payload?["page"]?.ToString());
        return null;
    }

    private JsResult? GetUserInfo(JsParam param)
    {
        return new()
        {
            Data = new()
            {
                ["id"] = user.Uid,
                ["gender"] = user.Gender,
                ["nickname"] = user.Nickname!,
                ["introduce"] = user.Introduce!,
                ["avatar_url"] = user.AvatarUrl!,
            },
        };
    }

    private JsResult? GetStatusBarHeight(JsParam param)
    {
        return new()
        {
            Data = new()
            {
                ["statusBarHeight"] = 0
            }
        };
    }

    private JsResult? GetDynamicSecrectV1(JsParam param)
    {
        const string ApiSalt = "N50pqm7FSy2AkFz2B3TqtuZMJ5TOl3Ep";

        var t = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string r = GetRandomString(t);
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes($"salt={ApiSalt}&t={t}&r={r}"));
        var check = Convert.ToHexString(bytes).ToLower();
        return new JsResult
        {
            Data = new()
            {
                ["DS"] = $"{t},{r},{check}",
            }
        };
    }

    private static string GetRandomString(int timestamp)
    {
        var sb = new StringBuilder(6);
        var random = new Random((int)timestamp);
        for (int i = 0; i < 6; i++)
        {
            int v8 = random.Next(0, 32768) % 26;
            int v9 = 87;
            if (v8 < 10)
            {
                v9 = 48;
            }
            _ = sb.Append((char)(v8 + v9));
        }
        return sb.ToString();
    }

    private JsResult? GetCookieInfo(JsParam param)
    {
        return new()
        {
            Data = new()
            {
                ["ltuid"] = user.Uid.ToString(),
                ["ltoken"] = cookieDic.GetValueOrDefault("ltoken") ?? cookieDic.GetValueOrDefault("ltoken_v2") ?? "",
                ["login_ticket"] = "",
            },
        };
    }

    private JsResult? GetHttpRequestHeader(JsParam param)
    {
        return new()
        {
            Data = new()
            {
                ["x-rpc-client_type"] = "5",
                ["x-rpc-device_id"] = DeviceId,
                ["x-rpc-app_version"] = AppVersion,
            },
        };
    }


    private class JsParam
    {
        /// <summary>
        /// 方法名称
        /// </summary>
        [JsonPropertyName("method")]
        public string Method { get; set; }

        /// <summary>
        /// 数据 可以为空
        /// </summary>
        [JsonPropertyName("payload")]
        public JsonNode? Payload { get; set; }

        /// <summary>
        /// 回调的名称，调用 JavaScript:mhyWebBridge 时作为首个参数传入
        /// </summary>
        [JsonPropertyName("callback")]
        public string? Callback { get; set; }
    }



    private class JsResult
    {
        /// <summary>
        /// 代码
        /// </summary>
        [JsonPropertyName("retcode")]
        public int Code { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// 数据
        /// </summary>
        [JsonPropertyName("data")]
        public Dictionary<string, object> Data { get; set; } = default!;


        public override string ToString()
        {
            return JsonSerializer.Serialize(this);
        }

    }

}



