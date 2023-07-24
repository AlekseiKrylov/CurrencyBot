using Telegram.Bot.Types.ReplyMarkups;

namespace Task11.Models
{
    internal class CommandHandlerResult
    {
        public string ResponseMessage { get; set; }
        public InlineKeyboardMarkup? Keyboard { get; set; }
    }
}
