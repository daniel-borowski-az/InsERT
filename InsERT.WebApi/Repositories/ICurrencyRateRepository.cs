using InsERT.WebApi.Models;

namespace InsERT.WebApi.Repositories
{
	public interface ICurrencyRateRepository
	{
		void SaveRates(IEnumerable<CurrencyRate> rates);
		CurrencyRate? GetRate(string code);
	}
}
