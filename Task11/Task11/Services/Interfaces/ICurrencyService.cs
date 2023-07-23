using Task11.Models;

namespace Task11.Services.Interfaces
{
    internal interface ICurrencyService
    {
        Task<CurrencyRate> GetCurrencyRatesAsync(string date, string userLanguage);
        Task<CurrencyInfo> GetCurrencyInfoAsync(string currencyCode, string date, string userLanguage);
    }
}