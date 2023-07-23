using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using Task11.Models;
using Task11.Services.Interfaces;
using static Task11.Resources.ResourceKeys;

namespace Task11.Services
{
    internal class CommandHandlerService : ICommandHandlerService
    {
        private const string PATTERN_CURRENCY_CODE = @"^[A-Za-z]{3}$";
        private const string DATE_FORMAT = "dd.MM.yyyy";
        private const int DECIMAL_POINT = 2;
        private readonly ICurrencyService _currencyService;
        private readonly IUserDataService _userDataService;
        private readonly ResourceManager _resourceManager;

        public CommandHandlerService(ICurrencyService currencyService, IUserDataService userDataService, ResourceManager resourceManager)
        {
            _currencyService = currencyService;
            _userDataService = userDataService;
            _resourceManager = resourceManager;
        }

        public async Task<string> HandleCommand(string command, long chatId, string messageText, UserData userData)
        {
            if (string.IsNullOrWhiteSpace(command))
                return await HandleUserInput(chatId, messageText, userData);

            return command switch
            {
                "/start" => HandleStartCommand(userData),
                "/help" => GetLocalizedMessage(RKeys.HelpMessage, userData.LanguageCode), //GetResponse(ResourceKeys.HelpMessage, userData.LanguageCode),
                _ => $"{GetLocalizedMessage(RKeys.UnknownСommandMessage, userData.LanguageCode)} {GetLocalizedMessage(RKeys.HelpMessage, userData.LanguageCode)}" //$"{GetResponse(ResourceKeys.UnknownСommandMessage, userData.LanguageCode)} {GetResponse(ResourceKeys.HelpMessage, userData.LanguageCode)}",
            }; ;
        }

        private string HandleStartCommand(UserData userData)
        {
            return GetLocalizedMessage(RKeys.WelcomeMessage, userData.LanguageCode); //GetResponse(ResourceKeys.WelcomeMessage, userData.LanguageCode);
        }

        public async Task<string> HandleUserInput(long chatId, string messageText, UserData userData)
        {
            return userData switch
            {
                var userDataState when userDataState.SelectedCurrency == string.Empty => HandleInputCurrency(chatId, messageText, userData),
                var userDataState when userDataState.SelectedCurrency != string.Empty => await HandleInputDate(chatId, messageText, userData),
                _ => GetLocalizedMessage(RKeys.DefaultMessage, userData.LanguageCode) //GetResponse(ResourceKeys.DefaultMessage, userData.LanguageCode),
            };
        }

        private string HandleInputCurrency(long chatId, string messageText, UserData userData)
        {
            if (!Regex.IsMatch(messageText, PATTERN_CURRENCY_CODE))
                return GetLocalizedMessage(RKeys.InvalidCurrencyMessage, userData.LanguageCode); //GetResponse(ResourceKeys.InvalidCurrencyMessage, userData.LanguageCode);

            userData.SelectedCurrency = messageText.ToUpper();
            _userDataService.SaveUserData(chatId, userData);

            return GetLocalizedMessage(RKeys.DatePromptMessage, userData.LanguageCode); //GetResponse(ResourceKeys.DatePromptMessage, userData.LanguageCode);
        }

        private async Task<string> HandleInputDate(long chatId, string messageText, UserData userData)
        {
            if (!DateTime.TryParseExact(messageText, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                return GetLocalizedMessage(RKeys.InvalidDateMessage, userData.LanguageCode); //GetResponse(ResourceKeys.InvalidDateMessage, userData.LanguageCode);

            var selectedCurrency = userData.SelectedCurrency;
            var currencyRateInfo = await _currencyService.GetCurrencyInfoAsync(selectedCurrency, messageText, userData.LanguageCode);

            var exchangeCourseMessage = GetLocalizedMessage(RKeys.ExchangeCourseMessage, userData.LanguageCode); //GetResponse(ResourceKeys.ExchangeCourseMessage, userData.LanguageCode);

            var formattedResponseMessage = string.Format
                (exchangeCourseMessage
                , messageText
                , selectedCurrency
                , FormatRate(currencyRateInfo.PurchaseRate, DECIMAL_POINT, userData.LanguageCode)
                , FormatRate(currencyRateInfo.SaleRate, DECIMAL_POINT, userData.LanguageCode)
                , currencyRateInfo.BaseCurrency);

            userData.SelectedCurrency = string.Empty;
            _userDataService.SaveUserData(chatId, userData);

            return formattedResponseMessage;
        }

        //private string GetResponse(string resourceKey, string languageCode)
        //{
        //    return _resourceManager.GetString(resourceKey, new CultureInfo(languageCode))
        //        ?? $"Something's wrong! {resourceKey}";
        //}

        private string FormatRate(decimal? rate, int decimalPoint, string languageCode)
        {
            if (rate.HasValue)
                return Math.Round(rate.Value, decimalPoint).ToString();

            return GetLocalizedMessage(RKeys.NoDataCaution, languageCode); //GetResponse(ResourceKeys.NoDataCaution, languageCode);
        }
    }
}
