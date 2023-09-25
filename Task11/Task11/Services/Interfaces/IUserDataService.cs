using Task11.Models;

namespace Task11.Services.Interfaces
{
    public interface IUserDataService
    {
       public UserData? GetUserData(long chatId);
       public void SaveUserData(long chatId, UserData data);
    }
}
