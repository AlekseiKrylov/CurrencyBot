using Newtonsoft.Json;
using Task11.Models;
using Task11.Services.Interfaces;

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

        public async Task<CurrencyRate> GetCurrencyRatesAsync(string date)
        {
            try
            {
                string url = $"{_bankApiUrl}?json&date={date}";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var serializer = new JsonSerializer();
                var currencyRate = JsonConvert.DeserializeObject<CurrencyRate>(jsonResponse);

                return currencyRate;
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка при выполнении HTTP-запроса: {ex.Message}");
                throw new Exception("Не удалось получить данные о курсах валют.");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Ошибка при парсинге JSON: {ex.Message}");
                throw new Exception("Ошибка при обработке данных о курсах валют.");
            }
        }

        public async Task<CurrencyInfo> GetCurrencyInfoAsync(string currencyCode, string date)
        {
            try
            {
                string url = $"{_bankApiUrl}?json&date={date}";

                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();

                var jsonResponse = await response.Content.ReadAsStringAsync();

                var serializer = new JsonSerializer();
                var currencyRate = JsonConvert.DeserializeObject<CurrencyRate>(jsonResponse);

                if (currencyRate is not null && currencyRate.ExchangeRate.Length > 0)
                {
                    var selectedCurrencyRates = currencyRate.ExchangeRate.FirstOrDefault(rate => rate.Currency == currencyCode);
                    if (selectedCurrencyRates != null)
                        return selectedCurrencyRates;

                    throw new Exception("Курс валюты не найден на указанную дату.");
                }

                throw new Exception("Курсы валют не найдены.");
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine($"Ошибка при выполнении HTTP-запроса: {ex.Message}");
                throw new Exception("Не удалось получить данные о курсах валют.");
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Ошибка при парсинге JSON: {ex.Message}");
                throw new Exception("Ошибка при обработке данных о курсах валют.");
            }
        }
    }
}
