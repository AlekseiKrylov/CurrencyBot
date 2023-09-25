using Microsoft.Extensions.Configuration;

namespace Task11
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
            .AddJsonFile($"appsettings.json", false, true)
            .Build();

            var botInitializer = new BotInitializer(configuration);
            botInitializer.Initialize();

            Console.ReadLine();
        }
    }
}
