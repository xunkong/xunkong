// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using Xunkong.GenshinData.Character;
using Xunkong.Hoyolab.SpiralAbyss;
using Xunkong.Hoyolab.Wishlog;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Xunkong.Desktop.Summaries;

[INotifyPropertyChanged]
public sealed partial class Summary2022View : UserControl
{



    public Summary2022View()
    {
        this.InitializeComponent();
        Loaded += Summary2022View_Loaded;
    }


    private readonly DateTimeOffset time2022 = new DateTimeOffset(2022, 1, 1, 0, 0, 0, TimeSpan.FromHours(8));
    private readonly DateTimeOffset time2023 = new DateTimeOffset(2023, 1, 1, 0, 0, 0, TimeSpan.FromHours(8));

    private List<CharacterInfo> list_wiki_character;
    private List<WishEventInfo> list_wiki_wishevent;


    [ObservableProperty]
    private List<int> allUids;



    [ObservableProperty]
    private Summary2022_Report report;



    [ObservableProperty]
    private bool showReport;



    private async void Summary2022View_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(30);
        try
        {
            using var dapper = DatabaseProvider.CreateConnection();
            AllUids = dapper.Query<int>("SELECT DISTINCT Uid FROM (SELECT DISTINCT Uid FROM WishlogItem UNION SELECT DISTINCT Uid FROM TravelNotesAwardItem UNION SELECT DISTINCT Uid FROM SpiralAbyssInfo);").ToList();
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }


    [RelayCommand]
    private void Close()
    {
        MainWindow.Current.CloseFullWindowContent();
    }





    [RelayCommand]
    private async Task GenerateReportAsync()
    {
        try
        {
            if (int.TryParse(ComboBox_SelectUid.SelectedItem?.ToString(), out int uid))
            {
#if DEBUG
                var sw = Stopwatch.StartNew();
#endif

                list_wiki_wishevent = XunkongApiService.GetGenshinData<WishEventInfo>().Where(x => x.EndTime >= time2022 && x.StartTime < time2023).ToList();
                list_wiki_character = XunkongApiService.GetGenshinData<CharacterInfo>();
                if (list_wiki_character.Count == 0 || list_wiki_wishevent.Count == 0)
                {
                    NotificationProvider.Warning("缺少原神数据，无法生成报告，请在设置页面手动更新");
                    return;
                }

                var report = await Task.Run(() =>
                {
                    return new Summary2022_Report
                    {
                        WishlogReport = GenerateWishlogReport(uid),
                        TravelNotesReport = GenerateTravelNotesReport(uid),
                        AbyssReport = GenerateAbyssReport(uid),
                    };
                });
                Report = report;
                ShowReport = true;
#if DEBUG
                sw.Stop();
                Debug.WriteLine("----------");
                Debug.WriteLine($"计算耗时 {sw.ElapsedMilliseconds} ms");
                Debug.WriteLine("----------");
#endif
            }
        }
        catch (Exception ex)
        {
            Logger.Error(ex);
            NotificationProvider.Error(ex);
        }
    }





    private Summary2022_WishlogReport? GenerateWishlogReport(int uid)
    {
        var total = WishlogService.GetWishlogCount(uid);
        if (total > 0)
        {
            var originList = WishlogService.GetWishlogItemExByUid(uid);
            var firstTime = originList.FirstOrDefault()?.Time ?? default;
            var isFirstTimeIn2022 = firstTime >= time2022;
            var list_2022 = originList.Where(x => x.Time >= time2022 && x.Time < time2023).ToList();
            var count_2022 = list_2022.Count;
            if (count_2022 > 0)
            {
                int count_200_常驻 = list_2022.Count(static x => x.WishType == WishType.Permanent);
                int count_301_角色 = list_2022.Count(predicate: static x => x.WishType == WishType.CharacterEvent || x.WishType == WishType.CharacterEvent_2);
                int count_302_武器 = list_2022.Count(static x => x.WishType == WishType.WeaponEvent);

                var list_rank5 = list_2022.Where(static x => x.RankType == 5).ToList();

                int count_xd_限定 = list_rank5.Count(static x => x.WishType == WishType.CharacterEvent || x.WishType == WishType.CharacterEvent_2 || x.WishType == WishType.WeaponEvent);
                int count_xd_noup_限定歪 = list_rank5.Count(static x => (x.WishType == WishType.CharacterEvent || x.WishType == WishType.CharacterEvent_2 || x.WishType == WishType.WeaponEvent) && !x.IsUp);

                List<WishlogItemEx> list_zo_最欧 = list_rank5.Where(x => x.GuaranteeIndex == list_rank5.Min(static y => y.GuaranteeIndex)).ToList();
                List<WishlogItemEx> list_zf_最非 = list_rank5.Where(x => x.GuaranteeIndex == list_rank5.Max(static y => y.GuaranteeIndex)).ToList();

                List<List<WishlogItemEx>> list_sl_十连 = new();
                List<WishlogItemEx> list_sln_十连内 = new(10);
                DateTimeOffset lastTime = DateTimeOffset.UnixEpoch;
                for (int i = 1; i < list_2022.Count; i++)
                {
                    var item = list_2022[i];
                    if (item.Time - lastTime > TimeSpan.FromSeconds(1))
                    {
                        lastTime = item.Time;
                        if (list_sln_十连内.Count <= 1)
                        {
                            list_sln_十连内.Clear();
                        }
                        else
                        {
                            list_sl_十连.Add(list_sln_十连内);
                            list_sln_十连内 = new(10);
                        }
                    }
                    list_sln_十连内.Add(item);
                }
                if (list_sln_十连内.Count > 1)
                {
                    list_sl_十连.Add(list_sln_十连内);
                }
                int count_sl_十连 = list_sl_十连.Count;
                int count_dc_单抽 = count_2022 - list_sl_十连.Sum(static x => x.Count);

                Dictionary<int, int> dic_sldj_十连多金 = new(9);
                for (int i = 2; i < 11; i++)
                {
                    dic_sldj_十连多金[i] = list_sl_十连.Count(x => x.Count(static y => y.RankType == 5) == i);
                }

                WishEventInfo.RegionType = WishlogService.UidToRegionType(uid);



                var list_wishevent_report = new List<Summary2022_WishlogReport_WishEvent>(list_wiki_wishevent.Count);
                foreach (var wishevent in list_wiki_wishevent)
                {
                    var list = list_2022.Where(x => x.WishType == wishevent.WishType && x.Time >= wishevent.StartTime && x.Time <= wishevent.EndTime).ToList();
                    if (list.Count == 0)
                    {
                        continue;
                    }
                    list_wishevent_report.Add(new Summary2022_WishlogReport_WishEvent
                    {
                        Count = list.Count,
                        Count_Rank_4 = list.Count(static x => x.RankType == 4),
                        Count_Rank_5 = list.Count(static x => x.RankType == 5),
                        WishEvent = wishevent,
                    });
                }

                var list_wiki_character_2022 = list_wiki_character.Where(x => x.BeginTime >= time2022.DateTime && x.BeginTime < time2023.DateTime).ToList();
                var list_wiki_character_2022_name = list_wiki_character_2022.Select(x => x.Name).ToList();

                var count_character_2022_total = list_wiki_character_2022.Count;

                var list_rank_4_5 = list_2022.Where(static x => x.RankType > 3).ToList();
                var list_new_character_name = new List<string>();
                foreach (var item in list_rank_4_5)
                {
                    if (list_wiki_character_2022_name.Contains(item.Name))
                    {
                        list_new_character_name.Add(item.Name);
                    }
                }
                int count_character_2022_new = list_new_character_name.Distinct().Count();

                var max_byname_character = list_2022.Where(x => x.ItemType == "角色" && x.RankType == 4)
                                                    .GroupBy(x => x.Name)
                                                    .OrderByDescending(x => x.Count())
                                                    .Select(x => new Summary2022_WishlogReport_MaxByName { Count = x.Count(), Name = x.Key })
                                                    .FirstOrDefault();

                var max_byname_weapon = list_2022.Where(x => x.ItemType == "武器" && x.RankType == 4)
                                                 .GroupBy(x => x.Name)
                                                 .OrderByDescending(x => x.Count())
                                                 .Select(x => new Summary2022_WishlogReport_MaxByName { Count = x.Count(), Name = x.Key })
                                                 .FirstOrDefault();

                return new Summary2022_WishlogReport
                {
                    Uid = uid,
                    IsFirstTimeIn2022 = isFirstTimeIn2022,
                    FirstTime = firstTime,
                    Count_2022 = count_2022,
                    Count_dc = count_dc_单抽,
                    Count_sl = count_sl_十连,
                    List_Sldj = dic_sldj_十连多金.Where(x => x.Value > 0).Select(x => new Summary2022_WishlogReport_Sldj_十连多金 { Rank5 = x.Key, Count = x.Value }).ToList(),
                    Count_Rank5 = list_2022.Count(x => x.RankType == 5),
                    Count_Rank4 = list_2022.Count(x => x.RankType == 4),
                    Count_200 = count_200_常驻,
                    Count_301 = count_301_角色,
                    Count_302 = count_302_武器,
                    Count_xd = count_xd_限定,
                    Count_xd_noup = count_xd_noup_限定歪,
                    GuaranteeIndex_zo = list_zo_最欧.FirstOrDefault()?.GuaranteeIndex ?? 0,
                    GuaranteeIndex_zf = list_zf_最非.FirstOrDefault()?.GuaranteeIndex ?? 0,
                    List_zo = list_zo_最欧,
                    List_zf = list_zf_最非,
                    TotalCount_WishEvent = list_wiki_wishevent.Count,
                    GetCount_WishEvent = list_wishevent_report.Count,
                    WishEvent_MaxCount = list_wishevent_report.MaxBy(static x => x.Count),
                    Count_Character_2022_Total = count_character_2022_total,
                    Count_Character_2022_New = count_character_2022_new,
                    Max_byname_character = max_byname_character,
                    Max_byname_weapon = max_byname_weapon,
                };
            }
        }
        return null;
    }



    private Summary2022_TravelNotesReport? GenerateTravelNotesReport(int uid)
    {
        var uidObj = new { uid };
        var time2022 = new DateTime(2022, 1, 1);
        var time2023 = new DateTime(2023, 1, 1);

        using var dapper = DatabaseProvider.CreateConnection();
        int total = dapper.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022;", uidObj);
        if (total > 0)
        {
            DateTime firstTime = dapper.QueryFirstOrDefault<DateTime>("SELECT Time FROM TravelNotesAwardItem WHERE Uid=@uid ORDER BY Time LIMIT 1;", uidObj);
            bool isFirstTimeIn2022 = firstTime >= time2022;
            // 原石次数
            int count_p = dapper.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022 AND Type=1;", uidObj);
            // 摩拉次数
            int count_m = dapper.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022 AND Type=2;", uidObj);
            // 原石总数
            int sum_p = dapper.QueryFirstOrDefault<int>("SELECT IFNULL(SUM(Number), 0) FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022 AND Type=1;", uidObj);
            // 摩拉总数
            int sum_m = dapper.QueryFirstOrDefault<int>("SELECT IFNULL(SUM(Number), 0) FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022 AND Type=2;", uidObj);
            // 原石摩拉日期数
            int count_date = dapper.QueryFirstOrDefault<int>("SELECT COUNT(DISTINCT DATE(TIME)) FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022;", uidObj);


            var max_month_p = dapper.QueryFirstOrDefault<Summary2022_TravelNotesReport_SumByMonth>("SELECT Month, IFNULL(SUM(Number), 0) AS Sum FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022 AND Type=1 GROUP BY Month ORDER BY Sum DESC LIMIT 1;", uidObj);
            var max_month_m = dapper.QueryFirstOrDefault<Summary2022_TravelNotesReport_SumByMonth>("SELECT Month, IFNULL(SUM(Number), 0) AS Sum FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022 AND Type=2 GROUP BY Month ORDER BY Sum DESC LIMIT 1;", uidObj);

            var max_day_p = dapper.QueryFirstOrDefault<Summary2022_TravelNotesReport_SumByDay>("SELECT DATE(TIME) AS Date, IFNULL(SUM(Number), 0) AS Sum FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022 AND Type=1 GROUP BY Date ORDER BY Sum DESC LIMIT 1;", uidObj);
            var max_day_m = dapper.QueryFirstOrDefault<Summary2022_TravelNotesReport_SumByDay>("SELECT DATE(TIME) AS Date, IFNULL(SUM(Number), 0) AS Sum FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022 AND Type=2 GROUP BY Date ORDER BY Sum DESC LIMIT 1;", uidObj);

            var list_action_p = dapper.Query<Summary2022_TravelNotesReport_SumByAction>("SELECT ActionName, COUNT(*) AS Count, IFNULL(SUM(Number), 0) AS Sum FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022 AND Type=1 GROUP BY ActionName ORDER BY Sum DESC;", uidObj);
            var list_action_m = dapper.Query<Summary2022_TravelNotesReport_SumByAction>("SELECT ActionName, COUNT(*) AS Count, IFNULL(SUM(Number), 0) AS Sum FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022 AND Type=2 GROUP BY ActionName ORDER BY Sum DESC;", uidObj);

            int count_cj_成就奖励 = list_action_p.FirstOrDefault(x => x.ActionName == "成就奖励")?.Count ?? 0;
            int count_mrwt_每日委托 = list_action_m.FirstOrDefault(x => x.ActionName == "每日委托奖励")?.Count ?? 0;
            int count_tfsj_突发事件 = list_action_m.FirstOrDefault(x => x.ActionName == "突发事件奖励")?.Count ?? 0;

            // 0~4 点完成每日委托的日期数
            var count_date_lc_凌晨 = dapper.QueryFirstOrDefault<int>("SELECT COUNT(DISTINCT DATE(Time)) FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022 AND (ActionName='每日委托奖励' OR ActionName='每日委托完成奖励') AND TIME(Time)>='00:00:00' AND TIME(Time)<'04:00:00';", uidObj);
            var time_lc = dapper.QueryFirstOrDefault<DateTime>("SELECT Time FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022 AND (ActionName='每日委托奖励' OR ActionName='每日委托完成奖励') AND TIME(Time)>='00:00:00' AND TIME(Time)<'04:00:00' ORDER BY TIME(Time) DESC LIMIT 1;", uidObj);

            var max_jsgw_击杀怪物 = dapper.QueryFirstOrDefault<Summary2022_TravelNotesReport_SumByDay>("SELECT DATE(Time) AS Date, COUNT(*) AS Count, IFNULL(SUM(Number), 0) AS Sum FROM TravelNotesAwardItem WHERE Uid=@uid AND Year=2022 AND Type=2 AND ActionName='击杀怪物奖励' GROUP BY Date ORDER BY Count DESC LIMIT 1;", uidObj);

            return new Summary2022_TravelNotesReport
            {
                Uid = uid,
                IsFirstTimeIn2022 = isFirstTimeIn2022,
                FirstTime = firstTime,
                Count_p = count_p,
                Count_m = count_m,
                Sum_p = sum_p,
                Sum_m = sum_m,
                Count_date = count_date,
                Count_cj = count_cj_成就奖励,
                Count_date_lc = count_date_lc_凌晨,
                Time_lc = time_lc,
                Count_mrwt = count_mrwt_每日委托,
                Count_tfsj = count_tfsj_突发事件,
                List_action_m = list_action_m.ToList(),
                List_action_p = list_action_p.ToList(),
                Max_day_m = max_day_m,
                Max_day_p = max_day_p,
                Max_jsgw = max_jsgw_击杀怪物,
                Max_month_m = max_month_m,
                Max_month_p = max_month_p,
            };
        }
        return null;
    }



    private Summary2022_AbyssReport? GenerateAbyssReport(int uid)
    {
        var uidObj = new { uid };
        var time2022 = new DateTime(2022, 1, 1);
        var time2023 = new DateTime(2023, 1, 1);

        using var dapper = DatabaseProvider.CreateConnection();
        int total = dapper.QueryFirstOrDefault<int>("SELECT COUNT(*) FROM SpiralAbyssInfo WHERE Uid=@uid AND ScheduleId>=37 AND ScheduleId<=60;", uidObj);
        if (total > 0)
        {
            var list = dapper.Query<string>("SELECT Value FROM SpiralAbyssInfo WHERE Uid=@uid AND ScheduleId>=37 AND ScheduleId<=60 ORDER BY ScheduleId;", uidObj).Select(x => JsonSerializer.Deserialize<SpiralAbyssInfo>(x)).ToList();
            var firstScheduleId = list.First()!.ScheduleId;
            var firstMiddleTime = list.First()!.StartTime + (list.First()!.EndTime - list.First()!.StartTime) / 2;
            var firstScheduleTimeString = $"{firstMiddleTime.Year}年{firstMiddleTime.Month}月{(firstMiddleTime.Day < 15 ? "上" : "下")}";

            var battles = list.SelectMany(x => x.Floors).SelectMany(x => x.Levels).SelectMany(x => x.Battles).ToList();
            var avatars = battles.SelectMany(x => x.Avatars).ToList();

            var battles_groupby_avatar = avatars.GroupBy(x => x.AvatarId).OrderByDescending(x => x.Count()).ToList();
            var mostUsedAvatars = battles_groupby_avatar.Take(8).Select(x => new Summary2022_AbyssReport_MostUseAvatar { Count = x.Count(), Avatar = x.First() }).ToList();
            var leastUsedAvatars = battles_groupby_avatar.TakeLast(8).Select(x => new Summary2022_AbyssReport_MostUseAvatar { Count = x.Count(), Avatar = x.First() }).ToList();
            foreach (var item in mostUsedAvatars)
            {
                if (list_wiki_character.FirstOrDefault(x => x.Id == item.Avatar.AvatarId) is CharacterInfo c)
                {
                    item.Avatar.Icon = c.SideIcon;
                }
            }
            foreach (var item in leastUsedAvatars)
            {
                if (list_wiki_character.FirstOrDefault(x => x.Id == item.Avatar.AvatarId) is CharacterInfo c)
                {
                    item.Avatar.Icon = c.SideIcon;
                }
            }

            var battles_groupby_team = battles.GroupBy(x => x, new Summary2022_AbyssReport_TeamCompare()).OrderByDescending(x => x.Count()).ToList();
            var mostUsedTeams = battles_groupby_team.Take(2).Select(x => new Summary2022_AbyssReport_MostUseTeam { Count = x.Count(), Avatars = x.First().Avatars }).ToList();
            var leastUsedTeams = battles_groupby_team.TakeLast(2).Select(x => new Summary2022_AbyssReport_MostUseTeam { Count = x.Count(), Avatars = x.First().Avatars }).ToList();
            foreach (var item in mostUsedTeams.SelectMany(x => x.Avatars))
            {
                if (list_wiki_character.FirstOrDefault(x => x.Id == item.AvatarId) is CharacterInfo c)
                {
                    item.Icon = c.SideIcon;
                }
            }
            foreach (var item in leastUsedTeams.SelectMany(x => x.Avatars))
            {
                if (list_wiki_character.FirstOrDefault(x => x.Id == item.AvatarId) is CharacterInfo c)
                {
                    item.Icon = c.SideIcon;
                }
            }

            var maxDamage = list.SelectMany(x => x.DamageRank).MaxBy(x => x.Value);
            var maxTakeDamage = list.SelectMany(x => x.TakeDamageRank).MaxBy(x => x.Value);


            return new Summary2022_AbyssReport
            {
                Uid = uid,
                FirstScheduleId = firstScheduleId,
                FirstScheduleString = firstScheduleTimeString,
                Count_Schedule = list.Count,
                TotalBattle = list.Sum(x => x.TotalBattleCount),
                TotalWin = list.Sum(x => x.TotalWinCount),
                CountStar36 = list.Count(x => x.TotalStar == 36),
                Count_12_3 = list.Count(x => x.MaxFloor == "12-3"),
                MaxDamage = maxDamage,
                MaxTakeDamage = maxTakeDamage,
                MostUsedAvatars = mostUsedAvatars,
                LeastUsedAvatars = leastUsedAvatars,
                MostUsedTeams = mostUsedTeams,
                LeastUsedTeams = leastUsedTeams,
                UsedAvatarCount = avatars.Select(x => x.AvatarId).Distinct().Count(),
                UsedAvatarCountRank5 = avatars.Where(x => x.Rarity == 5).Select(x => x.AvatarId).Distinct().Count(),
                UsedAvatarCountRank4 = avatars.Where(x => x.Rarity == 4).Select(x => x.AvatarId).Distinct().Count(),
                Floor1To8ScheduleCount = list.Count(x => x.Floors.Any(x => x.Index < 9)),
            };

        }
        return null;
    }

}


