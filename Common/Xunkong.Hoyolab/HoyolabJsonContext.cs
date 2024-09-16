#if NativeAOT

using System.Text.Json.Nodes;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.Avatar;
using Xunkong.Hoyolab.DailyNote;
using Xunkong.Hoyolab.GameRecord;
using Xunkong.Hoyolab.News;
using Xunkong.Hoyolab.SpiralAbyss;
using Xunkong.Hoyolab.TravelNotes;

namespace Xunkong.Hoyolab;


[JsonSerializable(typeof(ElementType[]))]
[JsonSerializable(typeof(RegionType[]))]
[JsonSerializable(typeof(HoyolabBaseWrapper<JsonNode>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<HoyolabUserInfoWrapper>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<HoyolabUserInfo>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<GenshinRoleInfoWrapper>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<GenshinRoleInfo>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<SignInInfo>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<SignInRisk>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<GameRecordSummary>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<AvatarInfo>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<PlayerRiskStats>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<WorldExploration>))]
[JsonSerializable(typeof(WorldExplorationRewardType[]))]
[JsonSerializable(typeof(HoyolabBaseWrapper<WorldExplorationOffering>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<PotHome>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<AvatarDetailWrapper>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<AvatarDetail>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<AvatarWeapon>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<AvatarReliquary>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<ReliquarySet>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<ReliquaryAffix>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<AvatarConstellation>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<AvatarCostume>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<AvatarCalculate>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<AvatarSkill>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<DailyNoteInfo>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<Expedition>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<Transformer>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<TransformerRecoveryTime>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<TravelNotesSummary>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<TravelNotesDayData>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<TravelNotesMonthData>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<TravelNotesPrimogemsMonthGroupStats>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<TravelNotesDetail>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<TravelNotesAwardItem>))]
[JsonSerializable(typeof(TravelNotesAwardType[]))]
[JsonSerializable(typeof(HoyolabBaseWrapper<SpiralAbyssInfo>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<SpiralAbyssRank>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<SpiralAbyssFloor>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<SpiralAbyssLevel>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<SpiralAbyssBattle>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<SpiralAbyssAvatar>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<NewsListWrapper>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<NewsDetailWrapper>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<NewsItem>))]
[JsonSerializable(typeof(HoyolabBaseWrapper<NewsPost>))]
[JsonSerializable(typeof(NewsType[]))]
internal partial class HoyolabJsonContext : JsonSerializerContext { }

#endif
