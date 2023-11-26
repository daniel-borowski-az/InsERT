using InsERT.WebApi.Models;

namespace InsERT.WebApi.Repositories
{
	public interface IWalletRepository
	{
		Task<Guid> Create(string name);
		Task<Wallet?> Get(Guid walletId);
		Task Deposit(Guid walletId, string currencyCode, decimal amount);
		Task  Withdraw(Guid walletId, string currencyCode, decimal amount);
		Task Convert(Guid walletId, string sourceCurrencyCode, string destinationCurrencyCode, decimal amount);
	}
}
