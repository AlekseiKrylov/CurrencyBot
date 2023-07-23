using Task11.Models;

namespace Task11.Services.Interfaces
{
    internal interface ICommandHandlerService
    {
        Task<string> HandleCommand(string command, long chatId, string messageText, UserData userData);
    }
}
