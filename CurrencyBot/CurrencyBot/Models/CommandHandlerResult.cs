using Telegram.Bot.Types.ReplyMarkups;

namespace CurrencyBot.Models
{
    public class CommandHandlerResult
    {
        public string ResponseMessage { get; set; }
        public InlineKeyboardMarkup? Keyboard { get; set; }
    }
}
