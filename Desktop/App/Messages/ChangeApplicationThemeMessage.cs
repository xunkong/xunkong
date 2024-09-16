using System.Numerics;

namespace Xunkong.Desktop.Messages;

[Obsolete("", true)]
internal class ChangeApplicationThemeMessage
{
    /// <summary>
    /// 0：跟随系统，1：浅色模式，2：深色模式
    /// </summary>
    public int Theme { get; set; }

    public Vector2 Center { get; set; }

    [Obsolete("", true)]
    public ChangeApplicationThemeMessage(int theme, Vector2 center)
    {
        Theme = theme;
        Center = center;
    }

}
