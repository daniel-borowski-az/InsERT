using Dapper;
using InsERT.WebApi.Database;
using InsERT.WebApi.Models;
using System.Data;

namespace InsERT.WebApi.Repositories
{
	public class WalletRepository : IWalletRepository
	{
		private readonly DbContext _context;

		private readonly ILogger<WalletRepository> _logger;

		public WalletRepository(DbContext context, ILogger<WalletRepository> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task Convert(Guid walletId, string sourceCurrencyCode, string destinationCurrencyCode, decimal amount)
		{
			using var conn = _context.CreateConnection();
			conn.Open();
			var tran = conn.BeginTransaction();
			try
			{
				ThrowIfWalletNotFound(conn, walletId, tran);
				using var multi = await conn.QueryMultipleAsync(CurrencyRateSql.GetCurrencyPair, new { code1 = sourceCurrencyCode, code2 = destinationCurrencyCode }, tran);
				var results = multi.Read<CurrencyRate>().ToList();
				var sourceCurrencyRate = results.SingleOrDefault(s => s.Code == sourceCurrencyCode);
				if (sourceCurrencyRate == null)
				{
					var errorMsg = $"Unsupported currency code {sourceCurrencyCode}";
					_logger.LogError(errorMsg);
					throw new KeyNotFoundException(errorMsg);
				}
				var destinationCurrencyRate = results.SingleOrDefault(s => s.Code == destinationCurrencyCode);
				if (destinationCurrencyRate == null)
				{
					var errorMsg = $"Unsupported currency code {destinationCurrencyCode}";
					_logger.LogError(errorMsg);
					throw new KeyNotFoundException(errorMsg);
				}
				ThrowIfInsufficientBalance(conn, walletId, sourceCurrencyCode, amount, tran);
				var convertedAmount = amount * sourceCurrencyRate.Mid / destinationCurrencyRate.Mid;
				await conn.ExecuteAsync(WalletSql.Convert, new { walletId, code = sourceCurrencyCode, amount, withdrawCode = sourceCurrencyCode, withdrawAmount = amount, depositCode = destinationCurrencyCode, depositAmount = convertedAmount }, tran);
				tran.Commit();
			}
			catch (Exception)
			{
				tran.Rollback();
				throw;
			}
			finally
			{
				conn.Close();
			}
		}

		public async Task<Guid> Create(string name)
		{
			var walletId = Guid.NewGuid();
			using var conn = _context.CreateConnection();
			conn.Open();
			var rate = await conn.ExecuteAsync(WalletSql.InsertWallet, new { Id = walletId, Name = name });
			conn.Close();
			return walletId;
		}

		public async Task Deposit(Guid walletId, string code, decimal amount)
		{
			using var conn = _context.CreateConnection();
			conn.Open();
			try
			{
				ThrowIfWalletNotFound(conn, walletId);
				ThrowIfInvalidCurrencyCode(conn, code);
				await conn.ExecuteAsync(WalletSql.Deposit, new { walletId, depositCode = code, depositAmount = amount });
			}
			finally
			{
				conn.Close();
			}
		}

		public async Task<Wallet?> Get(Guid walletId)
		{
			var walletDict = new Dictionary<Guid, Wallet>();
			using var conn = _context.CreateConnection();
			conn.Open();
			ThrowIfWalletNotFound(conn, walletId);
			var results = await conn.QueryAsync<Balance, Wallet, Balance>(WalletSql.GetWalletWithBalances, (balance, wallet) =>
			{
				Wallet? w;
				if (!walletDict.TryGetValue(wallet.WalletId, out w))
				{
					w = wallet;
					w.Balances = new List<Balance>();
					walletDict.Add(w.WalletId, w);
				}
				if (balance != null && balance.BalanceId != 0)
				{
					w.Balances.Add(balance);
				}

				return balance;
			},
			new { Id = walletId },
			splitOn: "walletId");

			return walletDict[walletId];
		}

		public async Task Withdraw(Guid walletId, string code, decimal amount)
		{
			using var conn = _context.CreateConnection();
			conn.Open();
			try
			{
				ThrowIfWalletNotFound(conn, walletId);
				ThrowIfInsufficientBalance(conn, walletId, code, amount);
				ThrowIfInvalidCurrencyCode(conn, code);
				await conn.ExecuteAsync(WalletSql.Withdraw, new { walletId, withdrawCode = code, withdrawAmount = amount });
			}
			finally
			{
				conn.Close();
			}
		}

		private void ThrowIfWalletNotFound(IDbConnection conn, Guid walletId, IDbTransaction? tran = null)
		{
			var id = conn.QuerySingleOrDefault<Guid?>(WalletSql.WalletExists, new { Id = walletId }, tran);
			if (!id.HasValue)
			{
				var errorMsg = $"Wallet {walletId} not found";
				_logger.LogError(errorMsg);
				throw new KeyNotFoundException(errorMsg);
			}
		}
		private void ThrowIfInvalidCurrencyCode(IDbConnection conn, string code, IDbTransaction? tran = null)
		{
			var rate = conn.QuerySingleOrDefault<CurrencyRate>(CurrencyRateSql.GetCurrencyRateByCode, new { code }, tran);
			if (rate == null)
			{
				var errorMsg = $"Unsupported currency code {code}";
				_logger.LogError(errorMsg);
				throw new InvalidOperationException(errorMsg);
			}
		}
		private void ThrowIfInsufficientBalance(IDbConnection conn, Guid walletId, string code, decimal amount, IDbTransaction? tran = null)
		{
			var id = conn.QuerySingleOrDefault<Guid?>(WalletSql.GetBalance, new { walletId, code, amount }, tran);
			if (!id.HasValue)
			{
				var errorMsg = $"Insufficient balance for {walletId} {code} {amount}";
				_logger.LogError(errorMsg);
				throw new InvalidOperationException(errorMsg);
			}
		}
	}
}