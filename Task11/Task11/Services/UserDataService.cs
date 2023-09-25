using System.Collections.Concurrent;
using Task11.Models;
using Task11.Services.Interfaces;

namespace Task11.Services
{
    public class UserDataService : IUserDataService
    {
        private readonly ConcurrentDictionary<long, UserData> _userDataCache = new();

        public UserData? GetUserData(long chatId)
        {
            _userDataCache.TryGetValue(chatId, out var data);
            return data?.Copy();
        }

        public void SaveUserData(long chatId, UserData data)
        {
            _userDataCache.AddOrUpdate(chatId, data, (key, oldValue) => data);
        }
    }
}
