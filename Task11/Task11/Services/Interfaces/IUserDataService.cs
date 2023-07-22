using Task11.Models;

namespace Task11.Services.Interfaces
{
    internal interface IUserDataService
    {
        UserData GetUserData(long chatId);
        void SaveUserData(long chatId, UserData data);
    }
}
