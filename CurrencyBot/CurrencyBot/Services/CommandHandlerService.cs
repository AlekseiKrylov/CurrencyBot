﻿using CurrencyBot.Models;
using CurrencyBot.Services.Interfaces;
using Newtonsoft.Json;
using System.Globalization;
using static CurrencyBot.Resources.ResourceKeys;
using static CurrencyBot.Utilities.MenuBuilder;

namespace CurrencyBot.Services
{
    public class CommandHandlerService : ICommandHandlerService
    {
        private const string DATE_FORMAT = "dd.MM.yyyy";
        private const int DECIMAL_POINT = 2;
        private readonly ICurrencyService _currencyService;
        private readonly IUserDataService _userDataService;
        private readonly HashSet<string> _availableCurrencies;

        public CommandHandlerService(ICurrencyService currencyService, IUserDataService userDataService, HashSet<string> availableCurrencies)
        {
            _currencyService = currencyService;
            _userDataService = userDataService;
            _availableCurrencies = availableCurrencies;
        }

        public async Task<CommandHandlerResult> HandleCommand(string command, long chatId, string messageText, UserData userData)
        {
            if (string.IsNullOrWhiteSpace(command))
                return await HandleUserInput(chatId, messageText, userData);

            return command switch
            {
                "/start" => HandleStartCommand(userData),
                "/go" => HandleGoCommand(userData),
                "/language" => HandleLanguageCommand(userData.LanguageCode),
                "/yesterday" => await HandleYesterdayCommand(chatId, userData),
                "/today" => await HandleTodayCommand(chatId, userData),
                "/help" => new CommandHandlerResult { ResponseMessage = GetLocalizedMessage(RKeys.HelpMessage, userData.LanguageCode) },
                var selectedLanguage when selectedLanguage.Length == 3 && selectedLanguage[0] == '/'
                             => HandlSelectedLanguageCommand(selectedLanguage[1..], chatId, userData),
                _ => new CommandHandlerResult { ResponseMessage = $"{GetLocalizedMessage(RKeys.UnknownСommandMessage, userData.LanguageCode)} {GetLocalizedMessage(RKeys.HelpMessage, userData.LanguageCode)}" }
            };
        }

        private CommandHandlerResult HandleStartCommand(UserData userData) => new()
        {
            ResponseMessage = GetLocalizedMessage(RKeys.WelcomeMessage, userData.LanguageCode),
            Keyboard = StartMenu(userData.LanguageCode)
        };

        private CommandHandlerResult HandleGoCommand(UserData userData) => new()
        {
            ResponseMessage = string.Format(GetLocalizedMessage(RKeys.CurrencyPromptMessage, userData.LanguageCode), string.Join(", ", _availableCurrencies)),
            Keyboard = CurrencyMenu()
        };

        private CommandHandlerResult HandleLanguageCommand(string selectedLanguage) => new()
        {
            ResponseMessage = GetLocalizedMessage(RKeys.SelectOptionMessage, selectedLanguage),
            Keyboard = ChoiceLanguageMenu()
        };

        private CommandHandlerResult HandlSelectedLanguageCommand(string selectedLanguage, long chatId, UserData userData)
        {
            userData.LanguageCode = selectedLanguage;
            _userDataService.SaveUserData(chatId, userData);

            return new CommandHandlerResult
            {
                ResponseMessage = GetLocalizedMessage(RKeys.SelectOptionMessage, selectedLanguage),
                Keyboard = StartMenu(selectedLanguage)
            };
        }

        private async Task<CommandHandlerResult> HandleYesterdayCommand(long chatId, UserData userData)
        {
            if (string.IsNullOrEmpty(userData.SelectedCurrency))
                return HandleGoCommand(userData);

            string formattedDate = DateTime.Today.AddDays(-1).ToString("dd.MM.yyyy");
            return await HandleInputDate(chatId, formattedDate, userData);
        }

        private async Task<CommandHandlerResult> HandleTodayCommand(long chatId, UserData userData)
        {
            if (string.IsNullOrEmpty(userData.SelectedCurrency))
                return HandleGoCommand(userData);

            string formattedDate = DateTime.Today.ToString("dd.MM.yyyy");
            return await HandleInputDate(chatId, formattedDate, userData);
        }

        public async Task<CommandHandlerResult> HandleUserInput(long chatId, string messageText, UserData userData)
        {
            return userData switch
            {
                var userDataState when userDataState.SelectedCurrency == string.Empty => HandleInputCurrency(chatId, messageText, userData),
                var userDataState when userDataState.SelectedCurrency != string.Empty => await HandleInputDate(chatId, messageText, userData),
                _ => new CommandHandlerResult { ResponseMessage = GetLocalizedMessage(RKeys.DefaultMessage, userData.LanguageCode) }
            };
        }

        private CommandHandlerResult HandleInputCurrency(long chatId, string messageText, UserData userData)
        {
            if (!_availableCurrencies.Contains(messageText.ToUpper()))
                return new CommandHandlerResult
                {
                    ResponseMessage = string.Format(
                        GetLocalizedMessage(RKeys.InvalidCurrencyMessage, userData.LanguageCode)
                        , messageText
                        , string.Join(", ", _availableCurrencies)),
                    Keyboard = CurrencyMenu()
                };

            userData.SelectedCurrency = messageText.ToUpper();
            _userDataService.SaveUserData(chatId, userData);

            return new CommandHandlerResult
            {
                ResponseMessage = GetLocalizedMessage(RKeys.DatePromptMessage, userData.LanguageCode),
                Keyboard = DateMenu(userData.LanguageCode)
            };
        }

        private async Task<CommandHandlerResult> HandleInputDate(long chatId, string messageText, UserData userData)
        {
            if (!DateTime.TryParseExact(messageText, DATE_FORMAT, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime selectedDate))
                return new CommandHandlerResult
                {
                    ResponseMessage = GetLocalizedMessage(RKeys.InvalidDateMessage, userData.LanguageCode),
                    Keyboard = DateMenu(userData.LanguageCode)
                };

            var selectedCurrency = userData.SelectedCurrency;
            try
            {
                var currencyRateInfo = await _currencyService.GetCurrencyInfoAsync(selectedCurrency, selectedDate, userData.LanguageCode);

                var exchangeCourseMessage = GetLocalizedMessage(RKeys.ExchangeCourseMessage, userData.LanguageCode);

                var formattedResponseMessage = string.Format(
                    exchangeCourseMessage
                    , selectedDate.ToString(DATE_FORMAT)
                    , selectedCurrency
                    , FormatRate(currencyRateInfo.PurchaseRate, DECIMAL_POINT, userData.LanguageCode)
                    , FormatRate(currencyRateInfo.SaleRate, DECIMAL_POINT, userData.LanguageCode)
                    , currencyRateInfo.BaseCurrency);

                ClearSelectedCurrensy(chatId, userData);

                return new CommandHandlerResult { ResponseMessage = formattedResponseMessage, Keyboard = RepeatMenu(userData.LanguageCode) };
            }
            catch (Exception ex)
            {
                return HandleException(ex, chatId, messageText, userData);
            }
        }

        private CommandHandlerResult HandleException(Exception ex, long chatId, string messageText, UserData userData)
        {
            var errorMessage = ex switch
            {
                NullReferenceException => string.Format(ex.Message, messageText, userData.SelectedCurrency),
                HttpRequestException => ex.Message,
                JsonException => ex.Message,
                _ => GetLocalizedMessage(RKeys.RequestProcessingError, userData.LanguageCode)
            };

            ClearSelectedCurrensy(chatId, userData);

            return new CommandHandlerResult { ResponseMessage = errorMessage };
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
