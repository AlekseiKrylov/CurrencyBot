namespace Task11.Models
{
    internal class UserData
    {
        public string SelectedCurrency { get; set; } = string.Empty;
        public string LanguageCode { get; set; } = string.Empty;

        public UserData Copy() => new()
        {
            SelectedCurrency = this.SelectedCurrency,
            LanguageCode = this.LanguageCode
        };
    }
}
