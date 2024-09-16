using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Material;
using Xunkong.GenshinData.Text;
using Xunkong.GenshinData.Weapon;
using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.ApiClient.GenshinData;

public class AllGenshinData
{

    public List<CharacterInfo> Characters { get; set; }

    public List<WeaponInfo> Weapons { get; set; }

    public List<WishEventInfo> WishEvents { get; set; }

    public Achievement Achievement { get; set; }

    public List<MaterialItem> Materials { get; set; }

    public List<TextMapItem> TextMaps { get; set; }

}
