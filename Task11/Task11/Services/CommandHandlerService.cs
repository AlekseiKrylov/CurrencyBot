using Newtonsoft.Json;
using System.Globalization;
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

        public CommandHandlerService(ICurrencyService currencyService, IUserDataService userDataService)
        {
            _currencyService = currencyService;
            _userDataService = userDataService;
        }

        public async Task<string> HandleCommand(string command, long chatId, string messageText, UserData userData)
        {
            if (string.IsNullOrWhiteSpace(command))
                return await HandleUserInput(chatId, messageText, userData);

            return command switch
            {
                "/start" => HandleStartCommand(userData),
                "/help" => GetLocalizedMessage(RKeys.HelpMessage, userData.LanguageCode),
                _ => $"{GetLocalizedMessage(RKeys.UnknownСommandMessage, userData.LanguageCode)} {GetLocalizedMessage(RKeys.HelpMessage, userData.LanguageCode)}"
            };
        }

        private string HandleStartCommand(UserData userData)
        {
            return GetLocalizedMessage(RKeys.WelcomeMessage, userData.LanguageCode);
        }

        public async Task<string> HandleUserInput(long chatId, string messageText, UserData userData)
        {
            return userData switch
            {
                var userDataState when userDataState.SelectedCurrency == string.Empty => HandleInputCurrency(chatId, messageText, userData),
                var userDataState when userDataState.SelectedCurrency != string.Empty => await HandleInputDate(chatId, messageText, userData),
                _ => GetLocalizedMessage(RKeys.DefaultMessage, userData.LanguageCode)
            };
        }

        private string HandleInputCurrency(long chatId, string messageText, UserData userData)
        {
            if (!Regex.IsMatch(messageText, PATTERN_CURRENCY_CODE))
                return GetLocalizedMessage(RKeys.InvalidCurrencyMessage, userData.LanguageCode);

            userData.SelectedCurrency = messageText.ToUpper();
            _userDataService.SaveUserData(chatId, userData);

            return GetLocalizedMessage(RKeys.DatePromptMessage, userData.LanguageCode);
        }

        private async Task<string> HandleInputDate(long chatId, string messageText, UserData userData)
        {
            if (!DateTime.TryParseExact(messageText, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out _))
                return GetLocalizedMessage(RKeys.InvalidDateMessage, userData.LanguageCode);

            var selectedCurrency = userData.SelectedCurrency;
            try
            {
                var currencyRateInfo = await _currencyService.GetCurrencyInfoAsync(selectedCurrency, messageText, userData.LanguageCode);

                var exchangeCourseMessage = GetLocalizedMessage(RKeys.ExchangeCourseMessage, userData.LanguageCode);

                var formattedResponseMessage = string.Format
                    (exchangeCourseMessage
                    , messageText
                    , selectedCurrency
                    , FormatRate(currencyRateInfo.PurchaseRate, DECIMAL_POINT, userData.LanguageCode)
                    , FormatRate(currencyRateInfo.SaleRate, DECIMAL_POINT, userData.LanguageCode)
                    , currencyRateInfo.BaseCurrency);

                ClearSelectedCurrensy(chatId, userData);
                
                return formattedResponseMessage;
            }
            catch (Exception ex)
            {
                return HandleException(ex, chatId, messageText, userData);
            }
        }

        private string HandleException(Exception ex, long chatId, string messageText, UserData userData)
        {
            var errorMessage = ex switch
            {
                NullReferenceException => string.Format(ex.Message, messageText, userData.SelectedCurrency),
                HttpRequestException => ex.Message,
                JsonException => ex.Message,
                _ => GetLocalizedMessage(RKeys.RequestProcessingError, userData.LanguageCode)
            };

            ClearSelectedCurrensy(chatId, userData);

            return errorMessage;
        }

        private void ClearSelectedCurrensy(long chatId, UserData userData)
        {
            userData.SelectedCurrency = string.Empty;
            _userDataService.SaveUserData(chatId, userData);
        }

        private string FormatRate(decimal? rate, int decimalPoint, string languageCode)
        {
            if (rate.HasValue)
                return Math.Round(rate.Value, decimalPoint).ToString();
                        
            return GetLocalizedMessage(RKeys.NoDataCaution, languageCode);
        }
    }
}
