using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Controls;

[INotifyPropertyChanged]
public sealed partial class GenshinDescTextBlock : UserControl
{


    [ObservableProperty]
    private string description;


    /// <summary>
    /// 浅色模式下的颜色映射
    /// </summary>
    private readonly static Dictionary<string, string> ColorMap = new Dictionary<string, string>
    {
        // 技能名称 RGBA
        {"FFD780FF","C5904EFF" },
        // 火
        {"FF9999FF","EE6946FF" },
        // 水
        {"80C0FFFF","478DCDFF" },
        // 风
        {"80FFD7FF","59A4A7FF" },
        // 雷
        {"FFACFFFF","8575CBFF" },
        // 草
        {"99FF88FF","7FB345FF" },
        // 冰
        {"99FFFFFF","47C1D9FF" },
        // 岩
        {"FFE699FF","CF9A58FF" },
    };



    public GenshinDescTextBlock()
    {
        this.InitializeComponent();
    }



    partial void OnDescriptionChanged(string value)
    {
        try
        {
            var text = ThisTextBlock;
            text.Inlines.Clear();
            if (string.IsNullOrWhiteSpace(value))
            {
                return;
            }
            var desc = value.AsSpan();
            int lastIndex = 0;
            bool mapColor = this.ActualTheme == Microsoft.UI.Xaml.ElementTheme.Light;
            for (int i = 0; i < desc.Length; i++)
            {
                // 换行
                if (desc[i] == '\\' && desc[i + 1] == 'n')
                {
                    text.Inlines.Add(new Run { Text = desc[lastIndex..i].ToString() });
                    text.Inlines.Add(new LineBreak());
                    i += 1;
                    lastIndex = i + 1;
                }
                // 颜色
                if (desc[i] == '<' && desc[i + 1] == 'c')
                {
                    text.Inlines.Add(new Run { Text = desc[lastIndex..i].ToString() });
                    var colorLength = desc.Slice(i + 8).IndexOf('>');
                    var colorString = desc.Slice(i + 8, colorLength);
                    if (mapColor)
                    {
                        colorString = ColorMap.GetValueOrDefault(colorString.ToString());
                    }
                    var color = Convert.FromHexString(colorString);
                    var textLength = desc.Slice(i + 9 + colorLength).IndexOf('<');
                    if (colorLength == 8)
                    {
                        text.Inlines.Add(new Run
                        {
                            Text = desc.Slice(i + 9 + colorLength, textLength).ToString(),
                            Foreground = new SolidColorBrush(Color.FromArgb(color[3], color[0], color[1], color[2])),
                        });
                    }
                    else
                    {
                        text.Inlines.Add(new Run
                        {
                            Text = desc.Slice(i + 9 + colorLength, textLength).ToString(),
                        });
                    }
                    i += 16 + colorLength + textLength;
                    lastIndex = i + 1;
                }
                // 引用
                if (desc[i] == '<' && desc[i + 1] == 'i')
                {
                    text.Inlines.Add(new Run { Text = desc[lastIndex..i].ToString() });
                    var length = desc.Slice(i + 3).IndexOf('<');
                    text.Inlines.Add(new Run
                    {
                        Text = desc.Slice(i + 3, length).ToString(),
                        FontStyle = Windows.UI.Text.FontStyle.Italic,
                    });
                    i += length + 6;
                    lastIndex = i + 1;
                }
                // 结尾
                if (i == desc.Length - 1)
                {
                    text.Inlines.Add(new Run { Text = desc.Slice(lastIndex).ToString() });
                }
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
        }
    }


}
