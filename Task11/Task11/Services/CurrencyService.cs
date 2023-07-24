using Newtonsoft.Json;
using Task11.Models;
using Task11.Services.Interfaces;
using static Task11.Resources.ResourceKeys;

namespace Task11.Services
{
    internal class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;
        private readonly string _bankApiUrl;

        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _bankApiUrl = httpClient.BaseAddress.ToString();
        }

        public async Task<CurrencyRate> GetCurrencyRatesAsync(string date, string userLanguage)
        {
            try
            {
                string url = $"{_bankApiUrl}?json&date={date}";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var serializer = new JsonSerializer();
                var currencyRate = JsonConvert.DeserializeObject<CurrencyRate>(jsonResponse);

                return currencyRate is null ? throw new NullReferenceException(GetLocalizedMessage(RKeys.RatesNotFoundCaution, userLanguage)) : currencyRate;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error during HTTP request execution: {ex.Message}");
                throw new HttpRequestException(GetLocalizedMessage(RKeys.FailedRetrieveDataError, userLanguage));
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error during JSON parsing: {ex.Message}");
                throw new Exception(GetLocalizedMessage(RKeys.ProcessingDataError, userLanguage));
            }
        }

        public async Task<CurrencyInfo> GetCurrencyInfoAsync(string currencyCode, string date, string userLanguage)
        {
            try
            {
                string url = $"{_bankApiUrl}?json&date={date}";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var serializer = new JsonSerializer();
                var currencyRate = JsonConvert.DeserializeObject<CurrencyRate>(jsonResponse);

                if (currencyRate is null || currencyRate.ExchangeRate.Length < 1)
                    throw new NullReferenceException(GetLocalizedMessage(RKeys.RatesNotFoundCaution, userLanguage));

                var selectedCurrencyRates = currencyRate.ExchangeRate.FirstOrDefault(rate => rate.Currency == currencyCode);

                return selectedCurrencyRates is null ? throw new NullReferenceException(GetLocalizedMessage(RKeys.RateNotFoundCaution, userLanguage)) : selectedCurrencyRates;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Error during HTTP request execution: {ex.Message}");
                throw new HttpRequestException(GetLocalizedMessage(RKeys.FailedRetrieveDataError, userLanguage));
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Error during JSON parsing: {ex.Message}");
                throw new Exception(GetLocalizedMessage(RKeys.ProcessingDataError, userLanguage));
            }
        }
    }
}
