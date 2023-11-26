using InsERT.WebApi.Models;

namespace InsERT.WebApi.Repositories
{
	public static class WalletSql
	{
		public const string WalletTableName = "Wallets";
		public const string BalanceTableName = "Balances";
		public const string CreateWalletTable = $@"
                IF NOT EXISTS (select * from sysobjects where name='{WalletTableName}' and xtype='U')
                CREATE TABLE {WalletTableName} (
					WalletId uniqueidentifier NOT NULL PRIMARY KEY,
                    {nameof(Wallet.Name)} VARCHAR(MAX) NOT NULL
                );";
		public const string CreateBalanceTable = $@"
                IF NOT EXISTS (select * from sysobjects where name='{BalanceTableName}' and xtype='U')
                CREATE TABLE {BalanceTableName} (
					BalanceId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
					WalletId uniqueidentifier NOT NULL,
                    {nameof(Balance.Code)} CHAR(3) NOT NULL,
					{nameof(Balance.Amount)} Decimal(18,6) NOT NULL,
					CONSTRAINT FK_Balances_Wallets FOREIGN KEY (WalletId) REFERENCES {WalletTableName}(WalletId)
                );";
		public const string InsertWallet = $@"INSERT INTO {WalletTableName} (WalletId, Name)VALUES(@Id, @Name);";
		public const string GetWalletWithBalances = $@"SELECT b.BalanceId, b.Amount, b.Code, b.WalletId, w.WalletId, w.Name  FROM {WalletTableName} w LEFT JOIN {BalanceTableName} b ON w.WalletId = b.WalletId WHERE w.WalletId= @Id";
		public const string WalletExists = $@"SELECT TOP 1 WalletId FROM {WalletTableName} WHERE WalletId= @Id";
		public const string GetBalance = $@"SELECT TOP 1 WalletId FROM {BalanceTableName} WHERE walletId = @walletId and code = @code and amount > @amount";
		public const string Withdraw = $@"UPDATE {BalanceTableName} set {nameof(Balance.Amount)} = {nameof(Balance.Amount)} - @withdrawAmount WHERE walletId = @walletId and code = @withdrawCode and amount >= @withdrawAmount;";

		public const string Deposit = $@"
					MERGE
						{BalanceTableName} AS target
					USING 
						(VALUES(@walletId, @depositCode, @depositAmount)) AS s(walletId, code, amount)
					ON 
						(target.walletId = s.walletId and target.code = s.code)
					WHEN MATCHED 
					THEN UPDATE
					SET
						target.{nameof(Balance.Amount)} = target.{nameof(Balance.Amount)} + s.amount
					WHEN NOT MATCHED
					THEN INSERT 
						(WalletId, {nameof(Balance.Code)},{nameof(Balance.Amount)}) 
						VALUES (s.walletId, s.code,s.amount);";

		public const string Convert = $"IF EXISTS({GetBalance}) BEGIN {Withdraw}{Deposit} END";
	}
}
