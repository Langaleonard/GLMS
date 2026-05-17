using System.Text.Json;

namespace GLMS.Web.Services
{
    public class CurrencyService
    {
        private readonly HttpClient _httpClient;

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<decimal> GetUsdToZarRateAsync()
        {
            var url = "https://open.er-api.com/v6/latest/USD";

            var response = await _httpClient.GetAsync(url);

            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();

            using var document = JsonDocument.Parse(json);

            var zarRate = document
                .RootElement
                .GetProperty("rates")
                .GetProperty("ZAR")
                .GetDecimal();

            return zarRate;
        }
    }
}