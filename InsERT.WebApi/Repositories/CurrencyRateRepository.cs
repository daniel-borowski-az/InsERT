using Dapper;
using InsERT.WebApi.Database;
using InsERT.WebApi.Models;
using Microsoft.Data.SqlClient;
using System.Data;

namespace InsERT.WebApi.Repositories
{
	public class CurrencyRateRepository : ICurrencyRateRepository
	{
		private readonly DbContext _context;
		public CurrencyRateRepository(DbContext context)
		{
			_context = context;
		}
		public CurrencyRate? GetRate(string code)
		{
			using var conn = _context.CreateConnection();
			conn.Open();
			var rate = conn.QuerySingleOrDefault<CurrencyRate>(CurrencyRateSql.GetCurrencyRateByCode, new { Code = code });
			conn.Close();
			return rate;
		}

		public void SaveRates(IEnumerable<CurrencyRate> rates)
		{
			var dataTable = CreateDataTable();

			foreach(var rate in rates)
			{
				var dr = dataTable.NewRow();
				dr[nameof(rate.Currency)] = rate.Currency;
				dr[nameof(rate.Code)] = rate.Code;
				dr[nameof(rate.Mid)] = rate.Mid;
				dataTable.Rows.Add(dr);
			}

			using var conn = _context.CreateConnection();
			conn.Open();
			
			conn.Execute(CurrencyRateSql.TruncateCurrencyRateTableSql);

			SqlBulkCopy objbulk = new((SqlConnection)conn)
			{
				DestinationTableName = CurrencyRateSql.CurrencyRateTableName
			};
			objbulk.ColumnMappings.Add(nameof(CurrencyRate.Currency), nameof(CurrencyRate.Currency));
			objbulk.ColumnMappings.Add(nameof(CurrencyRate.Code), nameof(CurrencyRate.Code));
			objbulk.ColumnMappings.Add(nameof(CurrencyRate.Mid), nameof(CurrencyRate.Mid));

			objbulk.WriteToServer(dataTable);

			conn.Close();
		}
		private static DataTable CreateDataTable()
		{
			var table = new DataTable();
			table.Columns.Add(new DataColumn(nameof(CurrencyRate.Currency), typeof(string)));
			table.Columns.Add(new DataColumn(nameof(CurrencyRate.Code), typeof(string)));
			table.Columns.Add(new DataColumn(nameof(CurrencyRate.Mid), typeof(decimal)));
			return table;
		}
	}
}
