using InsERT.WebApi.Repositories;
using InsERT.WebApi.Services;
using Quartz;

namespace InsERT.WebApi.Jobs
{
	public class CurrencyRateUpdaterJob : IJob
	{
		private readonly ICurrencyRateService _currencyRateService;
		private readonly ICurrencyRateRepository _repository;
		private readonly ILogger<CurrencyRateUpdaterJob> _logger;

		public CurrencyRateUpdaterJob(ICurrencyRateService currencyRateService, ICurrencyRateRepository repository, ILogger<CurrencyRateUpdaterJob> logger)
		{
			_currencyRateService = currencyRateService;
			_repository = repository;
			_logger = logger;
		}

		public async Task Execute(IJobExecutionContext context)
		{
			var rates = await _currencyRateService.GetRates();
			if(rates == null || !rates.Any())
			{
				_logger.LogWarning("No currency rates to update");
				return;
			}
			_repository.SaveRates(rates);
		}
	}
}
