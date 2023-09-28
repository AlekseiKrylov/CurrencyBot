using CurrencyBot.Resources;
using CurrencyBot.Services;
using CurrencyBot.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Resources;
using Telegram.Bot;
using Telegram.Bot.Polling;

namespace CurrencyBot
{
    internal class BotInitializer
    {
        private readonly string _botToken;
        private readonly string _apiUrl;
        private readonly HashSet<string> _availableCurrencies;

        public BotInitializer(IConfiguration configuration)
        {
            _botToken = configuration["BotSettings:Token"];
            _apiUrl = configuration["ApiSettings:PrivatBankApiUrl"];
            _availableCurrencies = new HashSet<string>(
                configuration.GetSection("CurrencySettings:AvailableCurrencies").Get<List<string>>(),
                StringComparer.OrdinalIgnoreCase);
        }

        public void Initialize()
        {
            var serviceProvider = ConfigureServices();
            var resourceManager = serviceProvider.GetRequiredService<ResourceManager>();
            ResourceKeys.InitializeResourceManager(resourceManager);

            var bot = serviceProvider.GetRequiredService<ITelegramBotClient>();
            Console.WriteLine($"Bot {bot.GetMeAsync().Result.FirstName} is running");

            var handlerService = serviceProvider.GetRequiredService<HandlerService>();

            var cts = new CancellationTokenSource();
            var cancellationToken = cts.Token;
            var receiverOptions = new ReceiverOptions
            {
                AllowedUpdates = { }, // receive all update types
            };

            bot.StartReceiving(
                updateHandler: async (bot, update, token) => await handlerService.HandleUpdateAsync(bot, update, token),
                pollingErrorHandler: async (bot, exception, token) => await handlerService.HandlePollingErrorAsync(bot, exception, token),
                receiverOptions: receiverOptions,
                cancellationToken: cancellationToken
            );
        }

        private IServiceProvider ConfigureServices() => new ServiceCollection()
            .AddSingleton<ITelegramBotClient>(new TelegramBotClient(_botToken))
            .AddSingleton<IUserDataService, UserDataService>()
            .AddSingleton<ICommandHandlerService, CommandHandlerService>(provider =>
            new CommandHandlerService(
                provider.GetRequiredService<ICurrencyService>(),
                provider.GetRequiredService<IUserDataService>(),
                _availableCurrencies))
            .AddSingleton<HandlerService>()
            .AddHttpClient()
            .AddSingleton<ICurrencyService, CurrencyService>(provider =>
            {
                var httpClientFactory = provider.GetRequiredService<IHttpClientFactory>();
                var httpClient = httpClientFactory.CreateClient();
                httpClient.BaseAddress = new Uri(_apiUrl);

                return new CurrencyService(httpClient, _availableCurrencies);
            })
            .AddSingleton(provider =>
            {
                return new ResourceManager("Task11.Resources.LanguagePackage", typeof(Program).Assembly);
            })
            .BuildServiceProvider();
    }
}
