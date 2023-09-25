using Telegram.Bot.Types.ReplyMarkups;
using static Task11.Resources.ResourceKeys;

namespace Task11.Utilities
{
    public static class MenuBuilder
    {
        public static InlineKeyboardMarkup StartMenu(string languageCode) => new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(GetLocalizedMessage(RKeys.Go, languageCode), "/go"),
            },
            new[]
            {
                InlineKeyboardButton.WithCallbackData(GetLocalizedMessage(RKeys.Language, languageCode), "/language"),
                InlineKeyboardButton.WithCallbackData(GetLocalizedMessage(RKeys.Help, languageCode), "/help"),
            }
        });

        public static InlineKeyboardMarkup ChoiceLanguageMenu() => new(new[]
        {
            new []
            {
                InlineKeyboardButton.WithCallbackData("Русский", "/ru"),
                InlineKeyboardButton.WithCallbackData("English", "/en"),
                InlineKeyboardButton.WithCallbackData("Украінська", "/uk")
            }
        });

        public static InlineKeyboardMarkup CurrencyMenu() => new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData("USD", "usd"),
                InlineKeyboardButton.WithCallbackData("EUR", "eur"),
            }
        });

        public static InlineKeyboardMarkup DateMenu(string languageCode) => new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(GetLocalizedMessage(RKeys.Yesterday, languageCode), "/yesterday"),
                InlineKeyboardButton.WithCallbackData(GetLocalizedMessage(RKeys.Today, languageCode), "/today"),
            }
        });

        public static InlineKeyboardMarkup RepeatMenu(string languageCode) => new(new[]
        {
            new[]
            {
                InlineKeyboardButton.WithCallbackData(GetLocalizedMessage(RKeys.Repeat, languageCode), "/go"),
            }
        });
    }
}
