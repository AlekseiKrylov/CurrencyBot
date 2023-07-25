using Task11.Models;

namespace Task11.Services.Interfaces
{
    public interface ICommandHandlerService
    {
        Task<CommandHandlerResult> HandleCommand(string command, long chatId, string messageText, UserData userData);
    }
}
