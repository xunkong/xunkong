using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xunkong.Desktop.Services
{
    internal class DbConnectionFactory<TDbConnection> where TDbConnection : DbConnection, new()
    {

        private readonly string _connectionString;

        public DbConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }


        public TDbConnection CreateDbConnection()
        {
            return new TDbConnection { ConnectionString = _connectionString };
        }

    }
}
