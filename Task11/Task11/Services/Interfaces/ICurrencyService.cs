using Task11.Models;

namespace Task11.Services.Interfaces
{
    internal interface ICurrencyService
    {
        Task<CurrencyRate> GetCurrencyRatesAsync(string date);
        Task<CurrencyInfo> GetCurrencyInfoAsync(string currencyCode, string date);
    }
}