namespace CurrencyBot.Models
{
    public class CurrencyRate
    {
        public string Date { get; set; }
        public string Bank { get; set; }
        public int BaseCurrency { get; set; }
        public string BaseCurrencyLit { get; set; }
        public CurrencyInfo[] ExchangeRate { get; set; }
    }
}
