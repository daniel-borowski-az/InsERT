using InsERT.WebApi.Models;

namespace InsERT.WebApi.Repositories
{
	public static class CurrencyRateSql
	{
		public const string CurrencyRateTableName = "CurrencyRates";

		public const string CreateCurrencyRateTable = $@"
                IF NOT EXISTS (select * from sysobjects where name='CurrencyRates' and xtype='U')
				BEGIN
                CREATE TABLE CurrencyRates (
					Id INT NOT NULL IDENTITY(1,1) PRIMARY KEY,
                    {nameof(CurrencyRate.Currency)} VARCHAR(128) NOT NULL,
                    {nameof(CurrencyRate.Code)}  CHAR(3) NOT NULL,
                    {nameof(CurrencyRate.Mid)} Decimal(10,6) NOT NULL
                );
				CREATE UNIQUE INDEX {CurrencyRateTableName}_{nameof(CurrencyRate.Code)}_INDEX ON {CurrencyRateTableName}({nameof(CurrencyRate.Code)});
				END";

		public const string TruncateCurrencyRateTableSql = $@"TRUNCATE TABLE {CurrencyRateTableName}";
		public const string GetCurrencyRateByCode = $@"SELECT Code, Currency, Mid FROM {CurrencyRateTableName} WHERE Code = @Code";
		public const string GetCurrencyPair = $@"SELECT Code, Currency, Mid FROM {CurrencyRateTableName} WHERE Code in (@Code1, @Code2);";
	}
}
