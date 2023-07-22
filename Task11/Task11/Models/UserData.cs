namespace Task11.Models
{
    internal class UserData
    {
        public string? SelectedCurrency { get; set; }
        public string? LanguageCode { get; set; }

        public UserData Copy() => new()
        {
            SelectedCurrency = this.SelectedCurrency,
            LanguageCode = this.LanguageCode
        };
    }
}
