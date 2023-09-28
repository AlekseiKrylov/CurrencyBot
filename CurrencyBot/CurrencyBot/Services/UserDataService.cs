using CurrencyBot.Models;
using CurrencyBot.Services.Interfaces;
using System.Collections.Concurrent;

namespace CurrencyBot.Services
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
