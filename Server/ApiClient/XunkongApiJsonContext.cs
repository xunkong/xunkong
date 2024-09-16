#if NativeAOT

using Xunkong.GenshinData.Character;
using Xunkong.GenshinData.Weapon;
using Xunkong.Hoyolab.Wishlog;

namespace Xunkong.ApiClient;


[JsonSerializable(typeof(ApiBaseWrapper<DesktopUpdateVersion>))]
[JsonSerializable(typeof(PlatformType[]))]
[JsonSerializable(typeof(ChannelType[]))]
[JsonSerializable(typeof(ApiBaseWrapper<NotificationWrapper<NotificationModelBase>>))]
[JsonSerializable(typeof(NotificationContentType[]))]
[JsonSerializable(typeof(ApiBaseWrapper<WishlogBackupResult>))]
[JsonSerializable(typeof(ApiBaseWrapper<WishlogItem>))]
[JsonSerializable(typeof(WishType[]))]
[JsonSerializable(typeof(ApiBaseWrapper<GenshinDataWrapper<CharacterInfo>>))]
[JsonSerializable(typeof(CharacterTalentInfo))]
[JsonSerializable(typeof(CharacterConstellationInfo))]
[JsonSerializable(typeof(ElementType[]))]
[JsonSerializable(typeof(WeaponType[]))]
[JsonSerializable(typeof(ApiBaseWrapper<GenshinDataWrapper<WeaponInfo>>))]
[JsonSerializable(typeof(WeaponSkill))]
[JsonSerializable(typeof(ApiBaseWrapper<GenshinDataWrapper<WishEventInfo>>))]
[JsonSerializable(typeof(RegionType[]))]
[JsonSerializable(typeof(ApiBaseWrapper<WallpaperInfo>))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
internal partial class XunkongApiJsonContext : JsonSerializerContext { }

#endif
