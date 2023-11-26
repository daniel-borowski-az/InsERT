using Dapper;
using InsERT.WebApi.Database;

namespace InsERT.WebApi.IntegrationTests
{
	internal static class DbContextExtensions
	{
		public static void CreateTestDatabase(this DbContext context)
		{
			using var conn = context.CreateConnection();
			conn.Open();
			conn.Execute(DatabaseSql.CreateDatabaseSql);
			conn.Close();
		}

		public static void DropTestDatabase(this DbContext context)
		{
			using var conn = context.CreateConnection();
			conn.Open();
			conn.Execute(DatabaseSql.DropDatabaseSql);
			conn.Close();
		}
	}
}
