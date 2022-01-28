using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using Xunkong.Desktop.Models;

namespace Xunkong.Desktop.Services
{

    [InjectService]
    internal class UserSettingService
    {


        private readonly DbConnectionFactory<SqliteConnection> _connectionFactory;

        private readonly ILogger<UserSettingService> _logger;


        public UserSettingService(DbConnectionFactory<SqliteConnection> connectionFactory, ILogger<UserSettingService> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }


        public async Task<T?> GetSettingAsync<T>(string key)
        {
            using var con = _connectionFactory.CreateDbConnection();
            var value = await con.QueryFirstOrDefaultAsync<string>($"SELECT Value FROM UserSettings WHERE Key='{key}';");
            _logger.LogTrace("Query UserSetting by key {Key} with value {Value}", key, value);
            if (value == null)
            {
                return default(T);
            }
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter == null)
                {
                    return default(T);
                }
                return (T?)converter.ConvertFromString(value);
            }
            catch (NotSupportedException)
            {
                return default(T);
            }
        }



        public async Task SaveSettingAsync<T>(string key, T value)
        {
            _logger.LogTrace("Save UserSetting with key {Key}, value {Value}", key, value);
            using var con = _connectionFactory.CreateDbConnection();
            var setting = new UserSettingModel
            {
                Key = key,
                Value = value?.ToString(),
            };
            await con.ExecuteAsync("INSERT OR REPLACE INTO UserSettings VALUES (@Key,@Value);", setting);
        }




    }
}
