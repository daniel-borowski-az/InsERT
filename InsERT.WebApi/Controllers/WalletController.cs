using InsERT.WebApi.Filters;
using InsERT.WebApi.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime;

namespace InsERT.WebApi.Controllers
{
	[ApiController]
	[Route("/[controller]/[action]")]
	[ServiceFilter(typeof(WalletExceptionFilter))]
	public class WalletController : ControllerBase
	{
		private readonly ILogger<WalletController> _logger;
		private readonly IWalletRepository _walletRepository;

		public WalletController(ILogger<WalletController> logger, IWalletRepository walletRepository)
		{
			_logger = logger;
			_walletRepository = walletRepository;
		}

		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status201Created)]
		[CreateWalletValidationFilter]
		public async Task<IActionResult> Create(string name)
		{
			var id = await _walletRepository.Create(name);
			var result = new JsonResult(id)
			{
				StatusCode = StatusCodes.Status201Created, 
				ContentType= "application/json"
			};
			return result;
		}

		[HttpGet]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> Get(Guid id)
		{
			var wallet = await _walletRepository.Get(id);
			var result = new JsonResult(wallet)
			{
				StatusCode = StatusCodes.Status200OK,
				ContentType = "application/json"
			};
			return result;
		}

		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[DepositWithrawValidationFilter]
		public async Task<IActionResult> Deposit(Guid walletId, string code, decimal amount)
		{
			await _walletRepository.Deposit(walletId, code, amount);
			return Ok();
		}

		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[DepositWithrawValidationFilter]
		public async Task<IActionResult> Withdraw(Guid walletId, string code, decimal amount)
		{
			await _walletRepository.Withdraw(walletId, code, amount);
			return Ok();
		}

		[HttpPost]
		[Consumes(MediaTypeNames.Application.Json)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ConvertValidationFilter]
		public async Task<IActionResult> Convert(Guid walletId, string sourceCurrencyCode, string destinationCurrencyCode, decimal amount)
		{
			await _walletRepository.Convert(walletId, sourceCurrencyCode, destinationCurrencyCode, amount);
			return Ok();
		}
	}
}