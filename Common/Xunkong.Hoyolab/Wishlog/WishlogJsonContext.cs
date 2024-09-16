#if NativeAOT

namespace Xunkong.Hoyolab.Wishlog;


[JsonSerializable(typeof(WishType[]))]
[JsonSerializable(typeof(HoyolabBaseWrapper<WishlogWrapper>))]
[JsonSerializable(typeof(WishlogItem))]
internal partial class WishlogJsonContext : JsonSerializerContext { }

#endif
