namespace Xunkong.Desktop.Messages;

internal class ProtocolMessage
{

    public string Message { get; set; }


    public Dictionary<string, string> Data { get; set; } = new();



    public const string ChangeSelectedUidInAchievementPage = nameof(ChangeSelectedUidInAchievementPage);


}
