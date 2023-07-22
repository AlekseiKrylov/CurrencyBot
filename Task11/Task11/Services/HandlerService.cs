using Task11.Models;
using Task11.Services.Interfaces;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace Task11.Services
{
    internal class HandlerService
    {
        private readonly ICommandHandlerService _commandHandlerService;
        private readonly ITelegramBotClient _bot;
        private readonly IUserDataService _userDataService;

        public HandlerService(ICommandHandlerService commandHandlerService, ITelegramBotClient bot, IUserDataService userDataService)
        {
            _commandHandlerService = commandHandlerService;
            _bot = bot;
            _userDataService = userDataService;
        }

        public async Task HandleUpdateAsync(ITelegramBotClient bot, Update update, CancellationToken cancellationToken)
        {
            try
            {
                if (update.Type == UpdateType.Message && update.Message.Text != null)
                {
                    long chatId = update.Message.Chat.Id;
                    string messageText = update.Message.Text;
                    string languageCode = update.Message.From.LanguageCode;

                    var userData = _userDataService.GetUserData(chatId) ?? new UserData();

                    if (string.IsNullOrEmpty(userData.LanguageCode) && !string.IsNullOrEmpty(languageCode))
                    {
                        userData.LanguageCode = languageCode;
                        _userDataService.SaveUserData(chatId, userData);
                    }

                    string responseMessage = await _commandHandlerService.HandleCommand(messageText, chatId, messageText);

                    await _bot.SendTextMessageAsync(chatId, responseMessage, cancellationToken: cancellationToken);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Произошла ошибка при обработке обновления: {ex.Message}");
            }
        }

        public Task HandlePollingErrorAsync(ITelegramBotClient bot, Exception exception, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Произошла ошибка: {exception.Message}");
            return Task.CompletedTask;
        }
    }
}
