using Dapper;
using InsERT.WebApi.Repositories;
using Microsoft.Data.SqlClient;
using System.Data;

namespace InsERT.WebApi.Database
{
    public class DbContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public DbContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("SqlConnection");
        }

        public IDbConnection CreateConnection()
            => new SqlConnection(_connectionString);

		public void EnsureCreated()
        {
			using var conn = CreateConnection();
			conn.Open();
            conn.Execute(CurrencyRateSql.CreateCurrencyRateTable);
            conn.Execute(WalletSql.CreateWalletTable);
			conn.Execute(WalletSql.CreateBalanceTable);
			conn.Close();
		}
    }
}
