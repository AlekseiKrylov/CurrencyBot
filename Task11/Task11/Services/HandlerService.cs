using System.Text.RegularExpressions;
using Task11.Models;
using Task11.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using static Task11.Resources.ResourceKeys;

namespace Task11.Services
{
    public class HandlerService
    {
        private const string PATTERN_COMMAND = @"^\/\w+";
        private readonly ICommandHandlerService _commandHandlerService;
        private readonly IUserDataService _userDataService;

        public HandlerService(ICommandHandlerService commandHandlerService, IUserDataService userDataService)
        {
            _commandHandlerService = commandHandlerService;
            _userDataService = userDataService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type != UpdateType.CallbackQuery && update.CallbackQuery is null && update.Type != UpdateType.Message && update.Message?.From is null)
                    return;

                long chatId = (update.Type == UpdateType.CallbackQuery) ? update.CallbackQuery!.From.Id
                            : (update.Type == UpdateType.Message) ? update.Message!.Chat.Id
                            : default;

                var messageText = (update.Type == UpdateType.CallbackQuery) ? update.CallbackQuery!.Data ?? string.Empty
                                   : (update.Type == UpdateType.Message) ? update.Message!.Text ?? string.Empty
                                   : string.Empty;

                var languageCode = (update.Type == UpdateType.CallbackQuery) ? update.CallbackQuery!.From.LanguageCode ?? string.Empty
                                    : (update.Type == UpdateType.Message) ? update.Message!.From!.LanguageCode ?? string.Empty
                                    : string.Empty;

                var command = Regex.IsMatch(messageText, PATTERN_COMMAND) ? messageText : string.Empty;

                var userData = _userDataService.GetUserData(chatId) ?? new UserData();

                if (string.IsNullOrEmpty(userData.LanguageCode) && !string.IsNullOrEmpty(languageCode))
                {
                    userData.LanguageCode = languageCode;
                    _userDataService.SaveUserData(chatId, userData);
                }

                var hendlerResult = await _commandHandlerService.HandleCommand(command, chatId, messageText, userData);

                await bot.SendTextMessageAsync(chatId, hendlerResult.ResponseMessage, replyMarkup: hendlerResult.Keyboard, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                var responseMessage = update.Type switch
                {
                    UpdateType.Message => GetLocalizedMessage(RKeys.RequestProcessingError, update.Message?.From?.LanguageCode ?? string.Empty),
                    UpdateType.CallbackQuery => GetLocalizedMessage(RKeys.RequestProcessingError, update.CallbackQuery?.From.LanguageCode ?? string.Empty),
                    _ => GetLocalizedMessage(RKeys.RequestProcessingError, string.Empty)
                };

                var chatId = (update.Message is not null) ? update.Message!.Chat.Id : (update.CallbackQuery is not null) ? update.CallbackQuery!.From.Id : default;

                if (update.Message is not null || update.CallbackQuery is not null)
                    await bot.SendTextMessageAsync(chatId, responseMessage, cancellationToken: cancellationToken);

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
