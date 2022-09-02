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
                    var color = Convert.FromHexString(desc.Slice(i + 8, colorLength));
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
                        FontFamily = new FontFamily("楷体"),
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
