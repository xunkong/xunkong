using Xunkong.Hoyolab.Account;

namespace Xunkong.Desktop.Messages;

internal class SelectedGameRoleChangedMessage
{

    public int Uid { get; set; }

    public GenshinRoleInfo Role { get; set; }

}
