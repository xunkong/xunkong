using System.ComponentModel;

namespace Xunkong.Core;

[Flags]
public enum ChannelType
{

    [Description("正式版")]
    Stable = 1,


    [Description("预览版")]
    Preview = 2,


    [Description("开发版")]
    Development = 4,


    [Description("商店版")]
    Store = 8,


    [Description("侧载版")]
    Sideload = 16,



}
