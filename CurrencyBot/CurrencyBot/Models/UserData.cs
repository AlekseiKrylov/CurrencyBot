namespace CurrencyBot.Models
{
    public class UserData
    {
        public string SelectedCurrency { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;

        public UserData Copy() => new()
        {
            SelectedCurrency = SelectedCurrency,
            LanguageCode = LanguageCode
        };
    }
}
