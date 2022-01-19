using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunkong.Core.Hoyolab;
using Xunkong.Core.SpiralAbyss;
using Xunkong.Core.TravelRecord;

namespace Xunkong.Desktop.Services
{
    internal class HoyolabService
    {

        private readonly IDbContextFactory<XunkongDbContext> _contextFactory;

        private readonly DbConnectionFactory<SqliteConnection> _connectionFactory;

        private readonly HoyolabClient _hoyolabClient;

        private readonly ILogger<HoyolabService> _logger;


        public HoyolabService(IDbContextFactory<XunkongDbContext> dbFactory, DbConnectionFactory<SqliteConnection> connectionFactory, HoyolabClient hoyolabClient, ILogger<HoyolabService> logger)
        {
            _contextFactory = dbFactory;
            _connectionFactory = connectionFactory;
            _hoyolabClient = hoyolabClient;
            _logger = logger;
        }


        public async Task<UserInfo?> GetLastSelectedOrFirstUserInfoAsync()
        {
            using var con = _connectionFactory.CreateDbConnection();
            const string sql = $"SELECT u.* FROM UserSettings AS s LEFT JOIN Hoyolab_Users AS u ON s.Value=u.Uid WHERE s.Key='{SettingKeys.LastSelectUserInfoUid}';";
            var info = await con.QueryFirstOrDefaultAsync<UserInfo>(sql);
            if (info is not null)
            {
                return info;
            }
            else
            {
                return await con.QueryFirstOrDefaultAsync<UserInfo>($"SELECT * FROM Hoyolab_Users;");
            }
        }


        public async Task<UserGameRoleInfo?> GetLastSelectedOrFirstUserGameRoleInfoAsync()
        {
            using var con = _connectionFactory.CreateDbConnection();
            const string sql = $"SELECT u.* FROM UserSettings AS s LEFT JOIN Genshin_Users AS u ON s.Value=u.Uid WHERE s.Key='{SettingKeys.LastSelectGameRoleUid}';";
            var info = await con.QueryFirstOrDefaultAsync<UserGameRoleInfo>(sql);
            if (info is not null)
            {
                return info;
            }
            else
            {
                return await con.QueryFirstOrDefaultAsync<UserGameRoleInfo>($"SELECT * FROM Genshin_Users;");
            }
        }



        public async Task<UserInfo> GetUserInfoAsync(string cookie)
        {
            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new ArgumentNullException(nameof(cookie));
            }
            var user = await _hoyolabClient.GetUserInfoAsync(cookie);
            using var con = _connectionFactory.CreateDbConnection();
            const string sql = "INSERT OR REPLACE INTO Hoyolab_Users"
                               + "(Uid,Nickname,Introduce,Avatar,Gender,AvatarUrl,Pendant,Cookie)"
                               + "VALUES (@Uid,@Nickname,@Introduce,@Avatar,@Gender,@AvatarUrl,@Pendant,@Cookie);";
            await con.ExecuteAsync(sql, user);
            return user;
        }




        public async Task<IEnumerable<UserInfo>> GetUserInfoListAsync()
        {
            using var con = _connectionFactory.CreateDbConnection();
            return await con.QueryAsync<UserInfo>("SELECT * FROM Hoyolab_Users;");
        }


        public async Task<UserInfo?> GetUserInfoAsync(int uid)
        {
            using var con = _connectionFactory.CreateDbConnection();
            return await con.QueryFirstOrDefaultAsync<UserInfo>($"SELECT * FROM Hoyolab_Users WHERE Uid={uid};");
        }


        public async Task<UserInfo?> GetUserInfoOrFirstAsync(int uid)
        {
            using var con = _connectionFactory.CreateDbConnection();
            var info = await con.QueryFirstOrDefaultAsync<UserInfo>($"SELECT * FROM Hoyolab_Users WHERE Uid={uid};");
            if (info is not null)
            {
                return info;
            }
            else
            {
                return await con.QueryFirstOrDefaultAsync<UserInfo>("SELECT * FROM Hoyolab_Users;");
            }
        }



        public async Task<UserInfo?> GetUserInfoAsync(UserInfo user)
        {
            if (user is null || string.IsNullOrWhiteSpace(user.Cookie))
            {
                return null;
            }
            return await GetUserInfoAsync(user.Cookie);
        }



        public async Task DeleteUserInfoAsync(int uid)
        {
            using var con = _connectionFactory.CreateDbConnection();
            await con.ExecuteAsync($"DELETE FROM Hoyolab_Users WHERE Uid={uid};");
        }



        public async Task<IEnumerable<UserGameRoleInfo>> GetUserGameRoleInfoListAsync()
        {
            using var con = _connectionFactory.CreateDbConnection();
            return await con.QueryAsync<UserGameRoleInfo>("SELECT * FROM Genshin_Users;");
        }



        public async Task<UserGameRoleInfo?> GetUserGameRoleInfoAsync(int uid)
        {
            using var con = _connectionFactory.CreateDbConnection();
            return await con.QueryFirstOrDefaultAsync<UserGameRoleInfo>($"SELECT * FROM Genshin_Users WHERE Uid={uid};");
        }


        public async Task<UserGameRoleInfo?> GetUserGameRoleInfoOrFirstAsync(int uid)
        {
            using var con = _connectionFactory.CreateDbConnection();
            var info = await con.QueryFirstOrDefaultAsync<UserGameRoleInfo>($"SELECT * FROM Genshin_Users WHERE Uid={uid};");
            if (info is not null)
            {
                return info;
            }
            else
            {

                return await con.QueryFirstOrDefaultAsync<UserGameRoleInfo>("SELECT * FROM Genshin_Users;");
            }
        }


        public async Task<UserGameRoleInfo?> GetUserGameRoleInfoOrFirstAsync(UserGameRoleInfo role)
        {
            if (role == null || string.IsNullOrWhiteSpace(role.Cookie))
            {
                return null;
            }
            var list = await GetUserGameRoleInfoListAsync(role.Cookie);
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


        public async Task<List<UserGameRoleInfo>> GetUserGameRoleInfoListAsync(string cookie)
        {
            if (string.IsNullOrWhiteSpace(cookie))
            {
                throw new ArgumentNullException(nameof(cookie));
            }
            var roles = await _hoyolabClient.GetUserGameRoleInfosAsync(cookie);
            using var con = _connectionFactory.CreateDbConnection();
            const string sql = "INSERT OR REPLACE INTO Genshin_Users"
                               + "(Uid,GameBiz,Region,Nickname,Level,IsChosen,RegionName,IsOfficial,Cookie)"
                               + "VALUES (@Uid,@GameBiz,@Region,@Nickname,@Level,@IsChosen,@RegionName,@IsOfficial,@Cookie);";
            await con.ExecuteAsync(sql, roles);
            return roles;
        }



        public async Task DeleteUserGameRoleInfoAsync(int uid)
        {
            using var con = _connectionFactory.CreateDbConnection();
            await con.ExecuteAsync($"DELETE FROM Genshin_Users WHERE Uid={uid};");
        }




        public async Task<SignInInfo> GetSignInInfoAsync(UserGameRoleInfo role)
        {
            return await _hoyolabClient.GetSignInInfoAsync(role);
        }



        public async Task<SignInResult> SignInAsync(UserGameRoleInfo role)
        {
            return await _hoyolabClient.SignInAsync(role);
        }



        public async Task<SpiralAbyssInfo> GetSpiralAbyssInfoAsync(UserGameRoleInfo role, int schedule = 1)
        {
            var info = await _hoyolabClient.GetSpiralAbyssInfoAsync(role, schedule);
            using var context = _contextFactory.CreateDbContext();
            var dbInfo = await context.SpiralAbyssInfos.FirstOrDefaultAsync(x => x.Uid == info.Uid && x.ScheduleId == info.ScheduleId);
            if (dbInfo is not null)
            {
                context.Remove(dbInfo);
            }
            context.Add(info);
            await context.SaveChangesAsync();
            return info;
        }



        public async Task<PlayerSummaryInfo> GetPlayerSummaryInfoAsync(UserGameRoleInfo role)
        {
            return await _hoyolabClient.GetPlayerSummaryInfoAsync(role);
        }



        public async Task<List<AvatarDetail>> GetAvatarDetailsAsync(UserGameRoleInfo role, PlayerSummaryInfo player)
        {
            return await _hoyolabClient.GetAvatarDetailsAsync(role, player);
        }




        public async Task<DailyNoteInfo?> GetDailyNoteInfoAsync(UserGameRoleInfo role)
        {
            var info = await _hoyolabClient.GetDailyNoteInfoAsync(role);
            if (info == null)
            {
                return null;
            }
            using var con = _connectionFactory.CreateDbConnection();
            const string sql = "INSERT INTO DailyNote_Items"
                               + "(Uid,Time,CurrentResin,MaxResin,ResinRecoveryTime,FinishedTaskNumber,TotalTaskNumber,IsExtraTaskRewardReceived,RemainResinDiscountNumber,ResinDiscountLimitedNumber,CurrentExpeditionNumber,MaxExpeditionNumber,CurrentHomeCoin,MaxHomeCoin,HomeCoinRecoveryTime)"
                               + "VALUES (@Uid,@Time,@CurrentResin,@MaxResin,@ResinRecoveryTime,@FinishedTaskNumber,@TotalTaskNumber,@IsExtraTaskRewardReceived,@RemainResinDiscountNumber,@ResinDiscountLimitedNumber,@CurrentExpeditionNumber,@MaxExpeditionNumber,@CurrentHomeCoin,@MaxHomeCoin,@HomeCoinRecoveryTime);";
            await con.ExecuteAsync(sql, info);
            return info;
        }



        public async Task<TravelRecordSummary> GetTravelRecordSummaryAsync(UserGameRoleInfo role, int month)
        {
            var summary = await _hoyolabClient.GetTravelRecordSummaryAsync(role, month);
            if (summary.MonthData is null)
            {
                return summary;
            }
            var m = summary.MonthData;
            using var context = _contextFactory.CreateDbContext();
            using var t = await context.Database.BeginTransactionAsync();
            try
            {
                await context.Database.ExecuteSqlRawAsync($"DELETE FROM TravelRecord_MonthDatas WHERE Uid={m.Uid} AND Year={m.Year} AND Month={m.Month};");
                context.Add(summary);
                await context.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                throw;
            }
            return summary;
        }



        public async Task<TravelRecordDetail> GetTravelRecordDetailAsync(UserGameRoleInfo role, int month, TravelRecordAwardType type, int limit = 10)
        {
            var detail = await _hoyolabClient.GetTravelRecordDetailAsync(role, month, type, limit);
            if (detail.List is null || !detail.List.Any())
            {
                return detail;
            }
            var list = detail.List;
            var dataYear = list[0].Year;
            var dataMonth = list[0].Month;
            using var context = _contextFactory.CreateDbContext();
            using var t = await context.Database.BeginTransactionAsync();
            try
            {
                await context.Database.ExecuteSqlRawAsync($"DELETE FROM TravelRecord_AwardItems WHERE Uid={detail.Uid} AND Year={dataYear} AND Month={dataMonth};");
                context.AddRange(list);
                await context.SaveChangesAsync();
                await t.CommitAsync();
            }
            catch (Exception ex)
            {
                await t.RollbackAsync();
                throw;
            }
            return detail;
        }


    }
}
