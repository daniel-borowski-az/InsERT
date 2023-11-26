using InsERT.WebApi.Controllers;
using InsERT.WebApi.Database;
using InsERT.WebApi.Models;
using InsERT.WebApi.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace InsERT.WebApi.IntegrationTests
{
	[TestFixture]
	public class WalletControllerTests
	{
		const string testDatabase = "InsERTTemp";
		const string masterDatabase = "master";
		const string connectionStringTest = $"server=.; database={testDatabase}; Integrated Security=true; Encrypt=false";
		const string connectionStringMaster = $"server=.; database={masterDatabase}; Integrated Security=true; Encrypt=false";

		private readonly DbContext _dbContext;
		private readonly IWalletRepository _walletRepository;
		private readonly Mock<ILogger<WalletController>> _loggerWalletControllerMock = new Mock<ILogger<WalletController>>();
		private readonly Mock<ILogger<WalletRepository>> _loggerWalletRepositoryMock = new Mock<ILogger<WalletRepository>>();
		private readonly ICurrencyRateRepository _currencyRateRepository;

		public WalletControllerTests()
		{
			Mock<IConfiguration> configurationMock = MockConfiguration(connectionStringTest);

			_dbContext = new DbContext(configurationMock.Object);
			_walletRepository = new WalletRepository(_dbContext, _loggerWalletRepositoryMock.Object);
			_currencyRateRepository = new CurrencyRateRepository(_dbContext);
		}

		private static Mock<IConfiguration> MockConfiguration(string connectionString)
		{
			var confSectionMock = new Mock<IConfigurationSection>();
			confSectionMock.SetupGet(m => m[It.Is<string>(s => s == "SqlConnection")]).Returns(connectionString);

			var configurationMock = new Mock<IConfiguration>();
			configurationMock.Setup(x => x.GetSection(It.Is<string>(s => s == "ConnectionStrings"))).Returns(confSectionMock.Object);
			return configurationMock;
		}

		[OneTimeSetUp]
		public void TestFixtureSetup()
		{
			Mock<IConfiguration> configurationMock = MockConfiguration(connectionStringMaster);
			var context = new DbContext(configurationMock.Object);
			context.CreateTestDatabase();
			_dbContext.EnsureCreated();
			var rates = new List<CurrencyRate>
			{
				new CurrencyRate{  Code = "CR1", Currency = "CR1", Mid = 1},
				new CurrencyRate{  Code = "CR2", Currency = "CR2", Mid = 2}
			};
			_currencyRateRepository.SaveRates(rates);
		}

		[OneTimeTearDown]
		public void TestFixtureTearDown()
		{
			Mock<IConfiguration> configurationMock = MockConfiguration(connectionStringMaster);
			var context = new DbContext(configurationMock.Object);
			context.DropTestDatabase();
		}

		[Test]
		public async Task CreateWallet_WithName_Succeed()
		{
			var controller = new WalletController(_loggerWalletControllerMock.Object, _walletRepository);
			var name = "testWallet";
			
			var response = await controller.Create(name) as JsonResult;

			Assert.IsTrue(response?.StatusCode == StatusCodes.Status201Created);
			Guid? walletId = response?.Value as Guid?;

			Assert.IsNotNull(walletId);
		}


		[Test]
		public async Task DepositBalance_WithCodeAndAmount_Succeed()
		{
			var controller = new WalletController(_loggerWalletControllerMock.Object, _walletRepository);
			var name = "testWallet";
			var amount = 100;
			var code = "CR1";

			var createWalletResponse = await controller.Create(name) as JsonResult;

			Assert.IsNotNull(createWalletResponse);
			Assert.IsTrue(createWalletResponse?.StatusCode == StatusCodes.Status201Created);
			
			Guid? walletId = createWalletResponse?.Value as Guid?;
			Assert.IsNotNull(walletId);

			var depositResponse = await controller.Deposit(walletId.Value, code, amount) as OkResult;
			Assert.IsNotNull(depositResponse);
			Assert.IsTrue(depositResponse?.StatusCode == StatusCodes.Status200OK);

			var getWalletResponse = await controller.Get(walletId.Value) as JsonResult;
			Assert.IsNotNull(getWalletResponse);
			Assert.IsTrue(getWalletResponse?.StatusCode == StatusCodes.Status200OK);

			var wallet = getWalletResponse?.Value as Wallet;
			Assert.IsNotNull(wallet);
			Assert.AreEqual(wallet?.Name, name);
			Assert.AreEqual(wallet?.WalletId, walletId.Value);
			Assert.True(wallet?.Balances?.Count == 1);
			
			var balance = wallet.Balances.Single();
			Assert.AreEqual(balance.Amount, amount);
			Assert.AreEqual(balance.Code, code);
		}
	}
}