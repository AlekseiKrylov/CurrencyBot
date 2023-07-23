using System.Resources;
using System.Text.RegularExpressions;
using Task11.Models;
using Task11.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Task11.Resources.ResourceKeys;

namespace Task11.Services
{
    internal class HandlerService
    {
        private const string PATTERN_COMMAND = @"^\/\w+";
        private readonly ICommandHandlerService _commandHandlerService;
        private readonly IUserDataService _userDataService;
        private readonly ResourceManager _resourceManager;

        public HandlerService(ICommandHandlerService commandHandlerService, IUserDataService userDataService, ResourceManager resourceManager)
        {
            _commandHandlerService = commandHandlerService;
            _userDataService = userDataService;
            _resourceManager = resourceManager;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type != UpdateType.Message && update.Message?.Text is null)
                    return;

                var chatId = update.Message.Chat.Id;
                var messageText = update.Message.Text;
                var languageCode = update.Message.From.LanguageCode;
                var command = string.Empty;

                var userData = _userDataService.GetUserData(chatId) ?? new UserData();

                if (string.IsNullOrEmpty(userData.LanguageCode) && !string.IsNullOrEmpty(languageCode))
                {
                    userData.LanguageCode = languageCode;
                    _userDataService.SaveUserData(chatId, userData);
                }

                if (Regex.IsMatch(messageText, PATTERN_COMMAND))
                    command = messageText;

                string responseMessage = await _commandHandlerService.HandleCommand(command, chatId, messageText, userData);

                await bot.SendTextMessageAsync(chatId, responseMessage, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                var responseMessage = GetLocalizedMessage(RKeys.RequestProcessingError, update.Message.From.LanguageCode); //_resourceManager.GetString(ResourceKeys.RequestProcessingError, new CultureInfo(update.Message.From.LanguageCode));
                await bot.SendTextMessageAsync(update.Message.Chat.Id, responseMessage, cancellationToken: cancellationToken);
                Console.WriteLine($"Error handling 'update': {ex.Message}");
            }
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Error: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
