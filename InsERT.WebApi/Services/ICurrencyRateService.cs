using InsERT.WebApi.Models;

namespace InsERT.WebApi.Services
{
	public interface ICurrencyRateService
	{
		Task<IEnumerable<CurrencyRate>?> GetRates();
	}
}
