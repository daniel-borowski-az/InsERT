namespace InsERT.WebApi.Models
{
	public class Wallet
	{
		public Guid WalletId { get; set; }
		public string Name { get; set; }
		public ICollection<Balance> Balances { get; set; }
	}
}
