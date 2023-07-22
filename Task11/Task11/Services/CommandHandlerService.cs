using System.Globalization;
using System.Resources;
using System.Text.RegularExpressions;
using Task11.Models;
using Task11.Resources;
using Task11.Services.Interfaces;

namespace Task11.Services
{
    internal class CommandHandlerService : ICommandHandlerService
    {
        private readonly ICurrencyService _currencyService;
        private readonly IUserDataService _userDataService;
        private readonly ResourceManager _resourceManager;

        public CommandHandlerService(ICurrencyService currencyService, IUserDataService userDataService, ResourceManager resourceManager)
        {
            _currencyService = currencyService;
            _userDataService = userDataService;
            _resourceManager = resourceManager;
        }

        public async Task<string> HandleCommand(string command, long chatId, string messageText)
        {
            switch (command)
            {
                case "/start":
                    return await HandleStartCommand(chatId);
                default:
                    return await HandleUserInput(chatId, messageText);
            }
        }

        private Task<string> HandleStartCommand(long chatId)
        {
            var userData = _userDataService.GetUserData(chatId);
            return Task.FromResult(_resourceManager.GetString(ResourceKeys.WelcomeMessage, new CultureInfo(userData.LanguageCode)));
        }


        public async Task<string> HandleUserInput(long chatId, string messageText)
        {
            var userData = _userDataService.GetUserData(chatId);

            switch (userData)
            {
                case null:
                    return await HandleEmptyState(chatId);
                case var userDataState when userDataState.SelectedCurrency == null:
                    return await HandleInputCurrency(chatId, messageText);
                case var userDataState when userDataState.SelectedCurrency != null:
                    return await HandleInputData(chatId, messageText);
                default:
                    return @"Please use the command '/start' to get started";
            }
        }

        private Task<string> HandleEmptyState(long chatId)
        {
            _userDataService.SaveUserData(chatId, new UserData());
            return Task.FromResult("Привет! Бот позволяет получить курсы валют Приват Банка. \n Выберите валюту. Например USD");
        }

        private Task<string> HandleInputCurrency(long chatId, string messageText)
        {
            var userData = _userDataService.GetUserData(chatId);

            if (!Regex.IsMatch(messageText, "^[A-Za-z]{3}$"))
                return Task.FromResult(_resourceManager.GetString(ResourceKeys.InvalidCurrencyMessage, new CultureInfo(userData.LanguageCode)));

            userData.SelectedCurrency = messageText.ToUpper();
            _userDataService.SaveUserData(chatId, userData);

            return Task.FromResult(_resourceManager.GetString(ResourceKeys.DatePromptMessage, new CultureInfo(userData.LanguageCode)));
        }

        private async Task<string> HandleInputData(long chatId, string messageText)
        {
            var userData = _userDataService.GetUserData(chatId);

            if (!DateTime.TryParseExact(messageText, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var selectedDate))
                return await Task.FromResult(_resourceManager.GetString(ResourceKeys.InvalidDateMessage, new CultureInfo(userData.LanguageCode)));

            var selectedCurrency = userData.SelectedCurrency;
            var currencyRateInfo = await _currencyService.GetCurrencyInfoAsync(selectedCurrency, messageText);

            var exchangeCourseMessage = _resourceManager.GetString(ResourceKeys.ExchangeCourseMessage, new CultureInfo(userData.LanguageCode));

            if (currencyRateInfo is null)
                return await Task.FromResult(_resourceManager.GetString(ResourceKeys.PlaceholderMessage, new CultureInfo(userData.LanguageCode)));

            var responseRateMessage = string.Format
                (exchangeCourseMessage
                , selectedDate.ToString("dd.MM.yyyy")
                , selectedCurrency
                , FormatRate(currencyRateInfo.PurchaseRate, 2)
                , FormatRate(currencyRateInfo.SaleRate, 2));

            userData.SelectedCurrency = null;
            _userDataService.SaveUserData(chatId, userData);

            return responseRateMessage;
        }

        private static string FormatRate(decimal? rate, int decimalPoint)
        {
            if (rate.HasValue)
                return Math.Round(rate.Value, decimalPoint).ToString();

            return "Нет данных";
        }
    }
}
