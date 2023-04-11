using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using Xunkong.Hoyolab;
using Xunkong.Hoyolab.Account;
using Xunkong.Hoyolab.Activity;
using Xunkong.Hoyolab.Avatar;
using Xunkong.Hoyolab.DailyNote;
using Xunkong.Hoyolab.GameRecord;
using Xunkong.Hoyolab.SpiralAbyss;
using Xunkong.Hoyolab.TravelNotes;

namespace Xunkong.Desktop.Services;

internal class HoyolabService
{


    private readonly HoyolabClient _hoyolabClient;

    private static JsonSerializerOptions JsonSerializerOptions = new JsonSerializerOptions { Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };


    public HoyolabService(HoyolabClient hoyolabClient)
    {
        _hoyolabClient = hoyolabClient;
    }


    public HoyolabUserInfo? GetLastSelectedOrFirstHoyolabUserInfo()
    {
        var lastUid = AppSetting.GetValue<int>(SettingKeys.LastSelectUserInfoUid);
        using var dapper = DatabaseProvider.CreateConnection();
        const string sql = $"SELECT * FROM HoyolabUserInfo WHERE Uid=@Uid LIMIT 1;";
        var info = dapper.QueryFirstOrDefault<HoyolabUserInfo>(sql, new { Uid = lastUid });
        if (info is not null)
        {
            return info;
        }
        else
        {
            return dapper.QueryFirstOrDefault<HoyolabUserInfo>($"SELECT * FROM HoyolabUserInfo LIMIT 1;");
        }
    }


    public GenshinRoleInfo? GetLastSelectedOrFirstGenshinRoleInfo()
    {
        var lastUid = AppSetting.GetValue<int>(SettingKeys.LastSelectGameRoleUid);
        using var dapper = DatabaseProvider.CreateConnection();
        const string sql = $"SELECT * FROM GenshinRoleInfo WHERE Uid=@Uid LIMIT 1;";
        var info = dapper.QueryFirstOrDefault<GenshinRoleInfo>(sql, new { Uid = lastUid });
        if (info is not null)
        {
            return info;
        }
        else
        {
            return dapper.QueryFirstOrDefault<GenshinRoleInfo>($"SELECT * FROM GenshinRoleInfo LIMIT 1;");
        }
    }



    public IEnumerable<HoyolabUserInfo> GetHoyolabUserInfoList()
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<HoyolabUserInfo>("SELECT * FROM HoyolabUserInfo;");
    }


    public IEnumerable<GenshinRoleInfo> GetGenshinRoleInfoList(bool onlyEnableDailyNote = false)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        if (onlyEnableDailyNote)
        {
            return dapper.Query<GenshinRoleInfo>("SELECT * FROM GenshinRoleInfo WHERE DisableDailyNote = 0 ORDER BY Sort DESC;");
        }
        else
        {
            return dapper.Query<GenshinRoleInfo>("SELECT * FROM GenshinRoleInfo;");
        }
    }



    public async Task<HoyolabUserInfo> GetHoyolabUserInfoAsync(string cookie)
    {
        var user = await _hoyolabClient.GetHoyolabUserInfoAsync(cookie);
        using var dapper = DatabaseProvider.CreateConnection();
        const string sql = """
                INSERT OR REPLACE INTO HoyolabUserInfo
                (Uid, Nickname, Introduce, Avatar, Gender, AvatarUrl, Pendant, Cookie)
                VALUES (@Uid, @Nickname, @Introduce, @Avatar, @Gender, @AvatarUrl, @Pendant, @Cookie);
                """;
        dapper.Execute(sql, user);
        return user;
    }



    public async Task<List<GenshinRoleInfo>> GetGenshinRoleInfoListAsync(string cookie)
    {
        var roles = await _hoyolabClient.GetGenshinRoleInfosAsync(cookie);
        using var dapper = DatabaseProvider.CreateConnection();
        const string sql = """
                INSERT INTO GenshinRoleInfo (Uid, GameBiz, Region, Nickname, Level, IsChosen, RegionName, IsOfficial, Cookie)
                VALUES (@Uid, @GameBiz, @Region, @Nickname, @Level, @IsChosen, @RegionName, @IsOfficial, @Cookie)
                ON CONFLICT DO UPDATE SET Nickname=@Nickname, Level=@Level, IsChosen=@IsChosen, Cookie=@Cookie;
                """;
        dapper.Execute(sql, roles);
        return roles;
    }



    public async Task UpdateAllAccountsAsync()
    {
        foreach (var item in GetHoyolabUserInfoList())
        {
            await GetHoyolabUserInfoAsync(item.Cookie!);
            await GetGenshinRoleInfoListAsync(item.Cookie!);
        }
    }



    public HoyolabUserInfo? GetHoyolabUserInfoFromDatabaseByCookie(string cookie)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.QueryFirstOrDefault<HoyolabUserInfo>("SELECT * FROM HoyolabUserInfo WHERE Cookie=@Cookie;", new { Cookie = cookie });
    }




    public HoyolabUserInfo? GetHoyolabUserInfo(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.QueryFirstOrDefault<HoyolabUserInfo>("SELECT * FROM HoyolabUserInfo WHERE Uid=@Uid;", new { Uid = uid });
    }

    public GenshinRoleInfo? GetGenshinRoleInfo(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.QueryFirstOrDefault<GenshinRoleInfo>("SELECT * FROM GenshinRoleInfo WHERE Uid=@Uid;", new { Uid = uid });
    }


    public HoyolabUserInfo? GetHoyolabUserInfoOrFirst(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var info = GetHoyolabUserInfo(uid);
        if (info is not null)
        {
            return info;
        }
        else
        {
            return dapper.QueryFirstOrDefault<HoyolabUserInfo>("SELECT * FROM HoyolabUserInfo LIMIT 1;");
        }
    }



    public async Task<GenshinRoleInfo?> GetGenshinRoleInfoOrFirstAsync(GenshinRoleInfo role)
    {
        if (role == null || string.IsNullOrWhiteSpace(role.Cookie))
        {
            return null;
        }
        var list = await GetGenshinRoleInfoListAsync(role.Cookie);
        var newRole = list?.FirstOrDefault(x => x.Uid == role.Uid);
        if (newRole != null)
        {
            return newRole;
        }
        else
        {
            return list?.FirstOrDefault();
        }
    }



    public void DeleteHoyolabUserInfo(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        dapper.Execute($"DELETE FROM HoyolabUserInfo WHERE Uid=@Uid;", new { Uid = uid });
    }


    public void DeleteHoyolabUserInfo(string cookie)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        dapper.Execute($"DELETE FROM HoyolabUserInfo WHERE Cookie=@Cookie;", new { Cookie = cookie });
    }



    public void DeleteGenshinRoleInfo(int uid)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        dapper.Execute($"DELETE FROM GenshinRoleInfo WHERE Uid=@Uid;", new { Uid = uid });
    }


    public void DeleteGenshinRoleInfo(string cookie)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        dapper.Execute($"DELETE FROM GenshinRoleInfo WHERE Cookie=@Cookie;", new { Cookie = cookie });
    }



    public async Task<SignInInfo> GetSignInInfoAsync(GenshinRoleInfo role)
    {
        return await _hoyolabClient.GetSignInInfoAsync(role);
    }



    /// <summary>
    /// 签到
    /// </summary>
    /// <param name="role"></param>
    /// <returns>true 签到成功，false 无需签到</returns>
    public async Task<bool> SignInAsync(GenshinRoleInfo role)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var localLastSignDate = dapper.QueryFirstOrDefault<string>("SELECT Date FROM DailySignInHistory WHERE Uid=@Uid ORDER BY Id DESC LIMIT 1;", new { Uid = role.Uid });
        var nowDate = DateTimeOffset.UtcNow.AddHours(8).ToString("yyyy-MM-dd");
        if (nowDate == localLastSignDate)
        {
            // 今日已签到，并且有记录
            return false;
        }
        // 签到
        bool result = await _hoyolabClient.SignInAsync(role);
        dapper.Execute("INSERT INTO DailySignInHistory (Uid, Date, Time) VALUES (@Uid, @Date, @Time);", new { Uid = role.Uid, Date = nowDate, Time = DateTimeOffset.Now });
        return result;
    }


    /// <summary>
    /// 深境螺旋
    /// </summary>
    /// <param name="role"></param>
    /// <param name="schedule">1当期，2上期</param>
    /// <returns></returns>
    public async Task<SpiralAbyssInfo> GetSpiralAbyssInfoAsync(GenshinRoleInfo role, int schedule = 1)
    {
        var info = await _hoyolabClient.GetSpiralAbyssInfoAsync(role, schedule);
        using var dapper = DatabaseProvider.CreateConnection();
        using var t = dapper.BeginTransaction();
        dapper.Execute($"DELETE FROM SpiralAbyssInfo WHERE Uid=@Uid AND ScheduleId=@ScheduleId;", new { Uid = info.Uid, ScheduleId = info.ScheduleId }, t);
        const string sql = """
                INSERT OR REPLACE INTO SpiralAbyssInfo
                (Uid, ScheduleId, StartTime, EndTime, TotalBattleCount, TotalWinCount, MaxFloor, TotalStar, Value)
                VALUES (@Uid, @ScheduleId, @StartTime, @EndTime, @TotalBattleCount, @TotalWinCount, @MaxFloor, @TotalStar, @Value);
                """;
        var obj = new
        {
            Uid = info.Uid,
            ScheduleId = info.ScheduleId,
            StartTime = info.StartTime,
            EndTime = info.EndTime,
            TotalBattleCount = info.TotalBattleCount,
            TotalWinCount = info.TotalWinCount,
            MaxFloor = info.MaxFloor,
            TotalStar = info.TotalStar,
            Value = JsonSerializer.Serialize(info),
        };
        dapper.Execute(sql, obj, t);
        t.Commit();
        return info;
    }



    public List<SpiralAbyssPageModel_LeftPanel> GetSpiralAbyssInfos(int uid)
    {
        const string sql = "SELECT Id, Uid, ScheduleId, StartTime, EndTime, TotalBattleCount, TotalWinCount, MaxFloor, TotalStar FROM SpiralAbyssInfo WHERE Uid=@Uid AND TotalBattleCount>0 ORDER BY ScheduleId DESC;";
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<SpiralAbyssPageModel_LeftPanel>(sql, new { Uid = uid }).ToList();
    }


    public SpiralAbyssInfo? GetSpiralAbyssInfo(int id)
    {
        const string sql = "SELECT Value FROM SpiralAbyssInfo WHERE Id=@Id LIMIT 1;";
        using var dapper = DatabaseProvider.CreateConnection();
        var value = dapper.QueryFirstOrDefault<string>(sql, new { Id = id });
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }
        else
        {
            return JsonSerializer.Deserialize<SpiralAbyssInfo>(value);
        }
    }


    public SpiralAbyssInfo? GetSpiralAbyssInfo(int uid, int scheduleId)
    {
        const string sql = "SELECT Value FROM SpiralAbyssInfo WHERE Uid=@Uid AND ScheduleId=@ScheduleId LIMIT 1;";
        using var dapper = DatabaseProvider.CreateConnection();
        var value = dapper.QueryFirstOrDefault<string>(sql, new { Uid = uid, ScheduleId = scheduleId });
        return JsonSerializer.Deserialize<SpiralAbyssInfo>(value);
    }




    public async void SaveAvatarDetailsAsync(GenshinRoleInfo role, List<AvatarDetail>? details = null)
    {
        try
        {
            using var dapper = DatabaseProvider.CreateConnection();
            var history = dapper.QueryFirstOrDefault<OperationHistory>("SELECT * FROM OperationHistory WHERE Operation='AvatarDetails' AND Key=@Uid ORDER BY Id DESC LIMIT 1;", new { role.Uid });
            if (history is not null && DateTimeOffset.Now - history.Time < TimeSpan.FromDays(7))
            {
                return;
            }
            if (details is null)
            {
                details = await _hoyolabClient.GetAvatarDetailsAsync(role);
            }
            OperationHistory.AddToDatabase("AvatarDetails", role.Uid.ToString(), details);
        }
        catch { }
    }




    public async Task<GameRecordSummary> GetGameRecordSummaryAsync(GenshinRoleInfo role)
    {
        return await _hoyolabClient.GetGameRecordSummaryAsync(role);
    }



    public async Task<List<AvatarDetail>> GetAvatarDetailsAsync(GenshinRoleInfo role)
    {
        return await _hoyolabClient.GetAvatarDetailsAsync(role);
    }



    public async Task<List<AvatarSkill>> GetAvatarSkillLevelAsync(GenshinRoleInfo role, int characterId)
    {
        return await _hoyolabClient.GetAvatarSkillLevelAsync(role, characterId);
    }


    public async Task<DailyNoteInfo?> GetDailyNoteAsync(GenshinRoleInfo role, bool disableCache = false)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        if (!disableCache)
        {
            var cacheMinutes = AppSetting.GetValue<int>(SettingKeys.DailyNoteCacheMinutes);
            var value = dapper.QueryFirstOrDefault<string>("SELECT Value FROM DailyNoteInfo WHERE Uid=@Uid AND Time>=@Time ORDER BY Id DESC LIMIT 1;", new { role.Uid, Time = DateTimeOffset.Now.AddMinutes(-cacheMinutes) });
            if (!string.IsNullOrWhiteSpace(value))
            {
                return JsonSerializer.Deserialize<DailyNoteInfo>(value);
            }
        }
        var note = await _hoyolabClient.GetDailyNoteAsync(role);
        AppSetting.SetValue(SettingKeys.DoNotRemindDailyNoteTaskError, false);
        dapper.Execute("INSERT INTO DailyNoteInfo (Uid, Time, Value) VALUES (@Uid, @Time, @Value);", new { note.Uid, Time = note.UpdateTime, Value = JsonSerializer.Serialize(note, JsonSerializerOptions) });
        return note;
    }



    public async Task<TravelNotesSummary> GetTravelNotesSummaryAsync(GenshinRoleInfo role, int month = 0)
    {
        var summary = await _hoyolabClient.GetTravelNotesSummaryAsync(role, month);
        if (summary.MonthData is null)
        {
            return summary;
        }
        var m = summary.MonthData;
        using var dapper = DatabaseProvider.CreateConnection();
        var existData = dapper.QueryFirstOrDefault<TravelNotesMonthData>("SELECT * FROM TravelNotesMonthData WHERE Uid=@Uid AND Year=@Year AND Month=@Month LIMIT 1;", m);
        if (existData is not null)
        {
            if (existData.CurrentPrimogems == m.CurrentPrimogems && existData.CurrentMora == m.CurrentMora)
            {
                return summary;
            }
        }
        dapper.Execute("""
            INSERT OR REPLACE INTO TravelNotesMonthData
            (Uid, Year, Month, CurrentPrimogems, CurrentMora, LastPrimogems, LastMora, CurrentPrimogemsLevel, PrimogemsChangeRate, MoraChangeRate, PrimogemsGroupBy)
            VALUES (@Uid, @Year, @Month, @CurrentPrimogems, @CurrentMora, @LastPrimogems, @LastMora, @CurrentPrimogemsLevel, @PrimogemsChangeRate, @MoraChangeRate, @PrimogemsGroupBy);
            """, m);
        return summary;
    }



    public async Task<int> GetTravelRecordDetailAsync(GenshinRoleInfo role, int month, TravelNotesAwardType type, int limit = 100)
    {
        var detail = await _hoyolabClient.GetTravelNotesDetailAsync(role, month, type, limit);
        if (detail.List is null || !detail.List.Any())
        {
            return 0;
        }
        var list = detail.List;
        var dataYear = list[0].Year;
        var dataMonth = list[0].Month;
        if (list.Any(x => x.Year != dataYear) || list.Any(x => x.Month != dataMonth))
        {
            // 检查是否有超出本月的记录项
            throw new XunkongException("Some of travel record items is out of the request month.");
        }
        using var dapper = DatabaseProvider.CreateConnection();
        var existCount = dapper.QuerySingleOrDefault<int>("SELECT COUNT(*) FROM TravelNotesAwardItem WHERE Uid=@Uid AND Year=@Year AND Month=@Month AND Type=@Type;", list.FirstOrDefault());
        if (existCount == list.Count)
        {
            return 0;
        }
        using var t = dapper.BeginTransaction();
        dapper.Execute($"DELETE FROM TravelNotesAwardItem WHERE Uid=@Uid AND Year=@Year AND Month=@Month AND Type=@Type;", list.FirstOrDefault(), t);
        dapper.Execute("""
                INSERT INTO TravelNotesAwardItem (Uid, Year, Month, Type, ActionId, ActionName, Time, Number)
                VALUES (@Uid, @Year, @Month, @Type, @ActionId, @ActionName, @Time, @Number);
                """, list, t);
        t.Commit();
        return list.Count - existCount;
    }



    /// <summary>
    /// 获取旅行原石和摩拉奖励项，不分原石和摩拉
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="type"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public IEnumerable<TravelNotesAwardItem> GetTravelNotesAwardItems(int uid, DateTime? startTime = null, DateTime? endTime = null)
    {
        const string sql = "SELECT * FROM TravelNotesAwardItem WHERE Uid=@Uid AND Time>=@StartTime AND Time<=@EndTime;";
        var obj = new { Uid = uid, StartTime = startTime ?? DateTime.MinValue, EndTime = endTime ?? DateTime.MaxValue };
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<TravelNotesAwardItem>(sql, obj);
    }



    /// <summary>
    /// 获取旅行原石和摩拉奖励项，区分原石和摩拉
    /// </summary>
    /// <param name="uid"></param>
    /// <param name="type"></param>
    /// <param name="startTime"></param>
    /// <param name="endTime"></param>
    /// <returns></returns>
    public IEnumerable<TravelNotesAwardItem> GetTravelNotesAwardItems(int uid, TravelNotesAwardType type, DateTime? startTime = null, DateTime? endTime = null)
    {
        const string sql = "SELECT * FROM TravelNotesAwardItem WHERE Uid=@Uid AND Type=@Type AND Time>=@StartTime AND Time<=@EndTime;";
        var obj = new { Uid = uid, Type = type, StartTime = startTime ?? DateTime.MinValue, EndTime = endTime ?? DateTime.MaxValue };
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<TravelNotesAwardItem>(sql, obj);
    }



    /// <summary>
    /// 获取旅行札记月信息
    /// </summary>
    /// <param name="uid"></param>
    /// <returns></returns>
    public IEnumerable<TravelNotesMonthData> GetTravelNotesMonthData(int uid)
    {
        const string sql = "SELECT * FROM TravelNotesMonthData WHERE Uid=@Uid;";
        using var dapper = DatabaseProvider.CreateConnection();
        return dapper.Query<TravelNotesMonthData>(sql, new { Uid = uid });
    }







    public async Task<List<CharacterInfoPage2_CharacterInfo>> GetCharacterInfosAsync(GenshinRoleInfo role)
    {
        var details = await _hoyolabClient.GetAvatarDetailsAsync(role);
        var list = new BlockingCollection<CharacterInfoPage2_CharacterInfo>(details.Count);
        await Parallel.ForEachAsync(details, async (detail, _) =>
        {
            var skills = await _hoyolabClient.GetAvatarSkillLevelAsync(role, detail.Id);
            var info = new CharacterInfoPage2_CharacterInfo
            {
                Id = detail.Id,
                Constellations = detail.ActivedConstellationNumber,
                Fetter = detail.Fetter,
                Icon = detail.Icon,
                Level = detail.Level,
                Name = detail.Name,
                Rarity = detail.Rarity,
                Element = detail.Element,
            };
            info.Weapon = new CharacterInfoPage2_WeaponInfo
            {
                Rarity = detail.Weapon.Rarity,
                Name = detail.Weapon.Name,
                Level = detail.Weapon.Level,
                AffixLevel = detail.Weapon.AffixLevel,
                Icon = detail.Weapon.Icon,
                Id = detail.Weapon.Id,
                Type = detail.Weapon.Type,
            };
            info.Reliquaries = Enumerable.Range(1, 5).Select(x => new CharacterInfoPage2_ReliquaryInfo()).ToList();
            foreach (var item in detail.Reliquaries)
            {
                info.Reliquaries.RemoveAt(item.Position - 1);
                info.Reliquaries.Insert(item.Position - 1, new CharacterInfoPage2_ReliquaryInfo
                {
                    Id = item.Id,
                    Icon = item.Icon,
                    Level = item.Level,
                    Name = item.Name,
                    Position = item.Position,
                    Rarity = item.Rarity,
                });
            }

            if (info.Id == 10000005)
            {
                info.Name = "空";
                info.Fetter = 10;
            }
            if (info.Id == 10000007)
            {
                info.Name = "荧";
                info.Fetter = 10;
            }

            if (info.Id == 10000033)
            {
                // 达达利亚
                info.SkillBuff_A_Icon = "https://file.xunkong.cc/genshin/talent/UI_Talent_S_Tartaglia_07.png";
            }

            if (detail.ActivedConstellationNumber >= 3)
            {
                var con = detail.Constellations[2];
                var name = Regex.Match(con.Effect, @">(.+)<").Groups[1].Value;
                var index = skills.FindIndex(x => x.Name == name);
                if (index < 2)
                {
                    info.SkillBuff_E_Icon = con.Icon;
                }
                else
                {
                    info.SkillBuff_Q_Icon = con.Icon;
                }
            }

            if (detail.ActivedConstellationNumber >= 5)
            {
                var con = detail.Constellations[4];
                var name = Regex.Match(con.Effect, @">(.+)<").Groups[1].Value;
                var index = skills.FindIndex(x => x.Name == name);
                if (index < 2)
                {
                    info.SkillBuff_E_Icon = con.Icon;
                }
                else
                {
                    info.SkillBuff_Q_Icon = con.Icon;
                }
            }

            int temp = 1;
            foreach (var skill in skills)
            {
                if (skill.MaxLevel > 1)
                {
                    switch (temp)
                    {
                        case 1:
                            info.SkillLevel_A = skill.CurrentLevel;
                            break;
                        case 2:
                            info.SkillLevel_E = skill.CurrentLevel;
                            break;
                        case 3:
                            info.SkillLevel_Q = skill.CurrentLevel;
                            break;
                        default:
                            break;
                    }
                    temp++;
                }
            }

            list.Add(info);
        });
        var result = new List<CharacterInfoPage2_CharacterInfo>(list.Count);
        foreach (var item in details)
        {
            var info = list.FirstOrDefault(x => x.Id == item.Id);
            if (info != null)
            {
                result.Add(info);
            }
        }
        for (int i = 0; i < result.Count; i++)
        {
            result[i].Index = i + 1;
        }
        return result;
    }




    /// <summary>
    /// 留影叙佳期
    /// </summary>
    /// <param name="role"></param>
    /// <returns></returns>
    public async Task<BirthdayStarIndex?> CheckBirthdayStarAsync(GenshinRoleInfo role)
    {
        using var dapper = DatabaseProvider.CreateConnection();
        var history = dapper.QueryFirstOrDefault<OperationHistory>("SELECT * FROM OperationHistory WHERE Operation='TakeBirthdayStarAlbum' AND Key=@Uid ORDER BY Id DESC LIMIT 1;", new { Uid = role.Uid.ToString() });
        if (history?.Time.LocalDateTime > DateTimeOffset.UtcNow.AddHours(8).LocalDateTime.Date)
        {
            return null;
        }
        var index = await _hoyolabClient.GetBirthdayStarIndexAsync(role);
        if (index.IsShowRemind)
        {
            return index;
        }
        else
        {
            OperationHistory.AddToDatabase("TakeBirthdayStarAlbum", role.Uid.ToString());
            return null;
        }
    }





}
