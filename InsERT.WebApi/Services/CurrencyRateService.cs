using InsERT.WebApi.Models;
using System.Net.Http.Headers;
using System.Text.Json;

namespace InsERT.WebApi.Services
{
	public class CurrencyRateService : ICurrencyRateService
	{
		private readonly IConfiguration _configuration;
		private readonly ILogger<CurrencyRateService> _logger;

		public CurrencyRateService(IConfiguration configuration, ILogger<CurrencyRateService> logger)
		{
			_configuration = configuration;
			_logger = logger;
		}

		public async Task<IEnumerable<CurrencyRate>?> GetRates()
		{
			var currencyRateUri = _configuration.GetValue<string>("Settings:CurrencyRateUrl");
			var client = new HttpClient();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			var response = await client.GetAsync(currencyRateUri);
			response.EnsureSuccessStatusCode();
			var responseBody = await response.Content.ReadAsStringAsync();
			var currencyTable = JsonSerializer.Deserialize<CurrencyRateTable[]>(responseBody, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

			var rates = currencyTable?.SingleOrDefault()?.Rates;
			if(rates == null || !rates.Any())
			{
				_logger.LogWarning($"Received empty CurrencyRate response for endpoint {currencyRateUri}");
			}
			return rates;
		}
	}
}
