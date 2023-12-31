﻿using CurrencyBot.Models;

namespace CurrencyBot.Services.Interfaces
{
    public interface IUserDataService
    {
        public UserData? GetUserData(long chatId);
        public void SaveUserData(long chatId, UserData data);
    }
}
