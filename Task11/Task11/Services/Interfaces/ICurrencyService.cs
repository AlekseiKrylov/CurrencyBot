using Task11.Models;

namespace Task11.Services.Interfaces
{
    public interface ICurrencyService
    {
        Task<CurrencyRate> GetCurrencyRatesAsync(DateTime date, string userLanguage);
        Task<CurrencyInfo> GetCurrencyInfoAsync(string currencyCode, DateTime date, string userLanguage);
    }
}