using MySqlConnector;

namespace Xunkong.ApiServer.Services
{
    public class DbConnectionFactory
    {

        private readonly string _connectionString;

        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }


        public MySqlConnection CreateDbConnection()
        {
            return new MySqlConnection { ConnectionString = _connectionString };
        }

    }
}
