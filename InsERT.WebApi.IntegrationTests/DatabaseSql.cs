namespace InsERT.WebApi.IntegrationTests
{
	public static class DatabaseSql
	{
		public const string DropDatabaseSql = $"ALTER DATABASE InsERTTemp SET SINGLE_USER WITH ROLLBACK IMMEDIATE drop database InsERTTemp;";
		public const string CreateDatabaseSql = $"IF NOT EXISTS (SELECT name FROM master.sys.databases WHERE name = 'InsERTTemp') create database InsERTTemp";
	}
}
