using System.ComponentModel;

namespace Xunkong.Desktop.Models;

public partial class GameAccount : ObservableObject
{

    public string SHA256 { get; set; }


    [ObservableProperty]
    private string name;


    public GameServer Server { get; set; }


    public byte[] Value { get; set; }


    public DateTime Time { get; set; } = DateTime.Now;


    public enum GameServer
    {

        [Description("官服")]
        CN = 0,

        [Description("国际服")]
        Global = 1,

        [Description("云原神")]
        CNCloud = 2,

    }
}
