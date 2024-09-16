namespace Xunkong.GenshinData.Character;

/// <summary>
/// 角色衣装
/// </summary>
public class CharacterOutfit
{

    public int Id { get; set; }

    public int CharacterId { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    /// <summary>
    /// 是否为默认
    /// </summary>
    public bool IsDefault { get; set; }

    public string? Card { get; set; }

    public string? FaceIcon { get; set; }

    public string? SideIcon { get; set; }

    public string? GachaSplash { get; set; }
}
