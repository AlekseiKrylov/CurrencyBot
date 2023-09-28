using CurrencyBot.Models;

namespace CurrencyBot.Services.Interfaces
{
    public interface ICommandHandlerService
    {
        Task<CommandHandlerResult> HandleCommand(string command, long chatId, string messageText, UserData userData);
    }
}
