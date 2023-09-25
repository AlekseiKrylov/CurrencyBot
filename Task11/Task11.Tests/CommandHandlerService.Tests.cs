using Moq;
using Newtonsoft.Json;
using System.Resources;
using Task11.Models;
using Task11.Resources;
using Task11.Services;
using Task11.Services.Interfaces;

namespace Task11.Tests
{
    [TestClass]
    public class CommandHandlerServiceTests
    {
        private ICommandHandlerService _commandHandlerService;
        private Mock<ICurrencyService> _currencyServiceMock;
        private Mock<IUserDataService> _userDataServiceMock;
        private const long CHAT_ID = 12345;
        private const string DATE_FORMAT = "dd.MM.yyyy";
        private readonly HashSet<string> _availableTestCurrencies = new() { "USD", "EUR" };

        [ClassInitialize]
        public static void InitializeClass(TestContext testContext)
        {
            var resourceManager = new ResourceManager("Task11.Resources.LanguagePackage", typeof(CommandHandlerService).Assembly);
            ResourceKeys.InitializeResourceManager(resourceManager);
        }

        [TestInitialize]
        public void Initialize()
        {
            _currencyServiceMock = new Mock<ICurrencyService>();
            _userDataServiceMock = new Mock<IUserDataService>();
            _commandHandlerService = new CommandHandlerService(_currencyServiceMock.Object, _userDataServiceMock.Object, _availableTestCurrencies);
        }

        [TestMethod]
        [DataRow("en", "Hello, the bot allows you to get currency rates to UAH from PrivatBank.", "Go,Language,Help")]
        [DataRow("ru", "Привет! Бот позволяет получить курсы валют к гривне от ПриватБанка.", "Начать,Язык,Помощь")]
        [DataRow("uk", "Привіт! Бот дозволяє отримувати курси валют до гривні від ПриватБанку.", "Почати,Мова,Допомога")]
        public async Task HandleStartCommand(string languageCode, string expectedResponse, string expectedKeyboardButtons)
        {
            // Arrange
            var userData = new UserData { LanguageCode = languageCode };
            var expectedButtons = expectedKeyboardButtons.Split(',');

            // Act
            var result = await _commandHandlerService.HandleCommand("/start", CHAT_ID, "", userData);

            // Assert
            Assert.AreEqual(expectedResponse, result.ResponseMessage);

            var keyboardButtons = result.Keyboard?.InlineKeyboard
            .SelectMany(row => row.Select(button => button.Text))
            .ToArray();

            Assert.IsNotNull(keyboardButtons);
            CollectionAssert.AreEquivalent(expectedButtons, keyboardButtons);
        }

        [TestMethod]
        [DataRow("en", "Select the currency on the on-screen keyboard or send the currency code to the chat.\r\nList of available currencies: {0}", "USD,EUR")]
        [DataRow("ru", "Выберите валюту на экранной клавиатуре или отправьте в чат код валюты.\r\nСписок доступных валют: {0}", "USD,EUR")]
        [DataRow("uk", "Виберіть валюту на екранній клавіатурі або надішліть у чат код валюти.\r\nСписок доступних валют: {0}", "USD,EUR")]
        public async Task HandleGoCommand(string languageCode, string expectedResponse, string expectedKeyboardButtons)
        {
            // Arrange
            var userData = new UserData { LanguageCode = languageCode };
            var expectedButtons = expectedKeyboardButtons.Split(',');
            var expectedFormattedResponse = string.Format(expectedResponse, string.Join(", ", _availableTestCurrencies));

            // Act
            var result = await _commandHandlerService.HandleCommand("/go", CHAT_ID, "", userData);

            // Assert
            Assert.AreEqual(expectedFormattedResponse, result.ResponseMessage);

            var keyboardButtons = result.Keyboard?.InlineKeyboard
            .SelectMany(row => row.Select(button => button.Text))
            .ToArray();

            Assert.IsNotNull(keyboardButtons);
            CollectionAssert.AreEquivalent(expectedButtons, keyboardButtons);
        }

        [TestMethod]
        [DataRow("en", "Select a date on the on-screen keyboard or send a date in the format dd.MM.yyyy to the chat. Example 15.05.2021", "Yesterday,Today")]
        [DataRow("ru", "Выберите дату на экранной клавиатуре или отправьте в чат дату в формате dd.MM.yyyy. Пример 15.05.2021", "Вчера,Сегодня")]
        [DataRow("uk", "Виберіть дату на екранній клавіатурі або надішліть у чат дату у форматі dd.MM.yyyy. Приклад 15.05.2021", "Вчора,Сьогодні")]
        public async Task HandleInputCurrency_ValidCurrency(string languageCode, string expectedResponse, string expectedKeyboardButtons)
        {
            // Arrange
            var userData = new UserData { LanguageCode = languageCode };
            var expectedButtons = expectedKeyboardButtons.Split(',');

            // Act
            var result = await _commandHandlerService.HandleCommand("", CHAT_ID, "USD", userData);

            // Assert
            Assert.AreEqual(expectedResponse, result.ResponseMessage);

            var keyboardButtons = result.Keyboard?.InlineKeyboard
            .SelectMany(row => row.Select(button => button.Text))
            .ToArray();

            Assert.IsNotNull(keyboardButtons);
            CollectionAssert.AreEquivalent(expectedButtons, keyboardButtons);
        }

        [TestMethod]
        [DataRow("en", "Currency {0} is not available. Please try again.\r\nList of available currencies: {1}", "USD,EUR")]
        [DataRow("ru", "Валюта {0} недоступна. Попробуйте ещё раз.\r\nСписок доступных валют: {1}", "USD,EUR")]
        [DataRow("uk", "Валюта {0} недоступна. Спробуйте ще раз.\r\nСписок доступних валют: {1}", "USD,EUR")]
        public async Task HandleInputCurrency_InvalidCurrency(string languageCode, string expectedResponse, string expectedKeyboardButtons)
        {
            // Arrange
            var userData = new UserData { LanguageCode = languageCode };
            var expectedButtons = expectedKeyboardButtons.Split(',');
            var inputCurrency = "USDT";
            var expectedFormattedResponse = string.Format(
                expectedResponse
                , inputCurrency
                , string.Join(", ", _availableTestCurrencies));

            // Act
            var result = await _commandHandlerService.HandleCommand("", CHAT_ID, inputCurrency, userData);

            // Assert
            Assert.AreEqual(expectedFormattedResponse, result.ResponseMessage);

            var keyboardButtons = result.Keyboard?.InlineKeyboard
            .SelectMany(row => row.Select(button => button.Text))
            .ToArray();

            Assert.IsNotNull(keyboardButtons);
            CollectionAssert.AreEquivalent(expectedButtons, keyboardButtons);
        }

        [TestMethod]
        [DataRow("en", "Date: {0}\r\nCurrency: {1}\r\nBuy rate: {2} {4}\r\nSell rate: {3} {4}", "Repeat")]
        [DataRow("ru", "Дата: {0}\r\nВалюта: {1}\r\nКурс покупки: {2} {4}\r\nКурс продажи: {3} {4}", "Повторить")]
        [DataRow("uk", "Дата: {0}\r\nВалюта: {1}\r\nКурс купівлі: {2} {4}\r\nКурс продажу: {3} {4}", "Повторити")]
        public async Task HandleInputDate_ValidDate(string languageCode, string expectedResponse, string expectedKeyboardButtons)
        {
            // Arrange
            var userData = new UserData { LanguageCode = languageCode, SelectedCurrency = "USD" };
            var expectedButtons = expectedKeyboardButtons.Split(',');
            var messageText = new DateTime(2021, 05, 15);
            var currencyRateInfo = new CurrencyInfo
            {
                PurchaseRate = 26.5m,
                SaleRate = 27.0m,
                BaseCurrency = "UAH"
            };
            _currencyServiceMock.Setup(x => x.GetCurrencyInfoAsync("USD", messageText, languageCode)).ReturnsAsync(currencyRateInfo);
            var expectedFormattedResponseMessage = string.Format
                (expectedResponse
                , messageText.ToString(DATE_FORMAT)
                , userData.SelectedCurrency
                , currencyRateInfo.PurchaseRate
                , currencyRateInfo.SaleRate
                , currencyRateInfo.BaseCurrency);

            // Act
            var result = await _commandHandlerService.HandleCommand("", CHAT_ID, messageText.ToString(DATE_FORMAT), userData);

            // Assert
            Assert.AreEqual(expectedFormattedResponseMessage, result.ResponseMessage);

            var keyboardButtons = result.Keyboard?.InlineKeyboard
            .SelectMany(row => row.Select(button => button.Text))
            .ToArray();

            Assert.IsNotNull(keyboardButtons);
            CollectionAssert.AreEquivalent(expectedButtons, keyboardButtons);
        }

        [TestMethod]
        [DataRow("en", "Incorrect date format. Please use the format dd.MM.yyyy", "Yesterday,Today")]
        [DataRow("ru", "Некорректный формат даты. Пожалуйста, используйте формат dd.MM.yyyy", "Вчера,Сегодня")]
        [DataRow("uk", "Некоректний формат дати. Будь ласка, використовуйте формат dd.MM.yyyy", "Вчора,Сьогодні")]
        public async Task HandleInputDate_InvalidDate(string languageCode, string expectedResponse, string expectedKeyboardButtons)
        {
            // Arrange
            var userData = new UserData { LanguageCode = languageCode, SelectedCurrency = "USD" };
            var expectedButtons = expectedKeyboardButtons.Split(',');
            var messageText = "15.05.20211";

            // Act
            var result = await _commandHandlerService.HandleCommand("", CHAT_ID, messageText, userData);

            // Assert
            Assert.AreEqual(expectedResponse, result.ResponseMessage);

            var keyboardButtons = result.Keyboard?.InlineKeyboard
            .SelectMany(row => row.Select(button => button.Text))
            .ToArray();

            Assert.IsNotNull(keyboardButtons);
            CollectionAssert.AreEquivalent(expectedButtons, keyboardButtons);
        }

        [TestMethod]
        [DataRow("en", "{1} exchange rate for {0} was not found.")]
        [DataRow("ru", "Курс для {1} на {0} не найден.")]
        [DataRow("uk", "Курс для {1} на {0} не знайдено.")]
        public async Task HandleInputDate_NullReferenceException_RateNotFound(string languageCode, string expectedResponse)
        {
            // Arrange
            var userData = new UserData { LanguageCode = languageCode, SelectedCurrency = "USD" };
            var messageText = new DateTime(2021, 05, 15);

            _currencyServiceMock.Setup(x => x.GetCurrencyInfoAsync(userData.SelectedCurrency, messageText, userData.LanguageCode))
                       .ThrowsAsync(new NullReferenceException(expectedResponse));

            var expectedFormattedResponseMessage = string.Format
                (expectedResponse
                , messageText.ToString(DATE_FORMAT)
                , userData.SelectedCurrency);

            // Act
            var result = await _commandHandlerService.HandleCommand("", CHAT_ID, messageText.ToString(DATE_FORMAT), userData);

            // Assert
            Assert.AreEqual(expectedFormattedResponseMessage, result.ResponseMessage);
        }

        [TestMethod]
        [DataRow("en", "Currency rates for {0} were not found.")]
        [DataRow("ru", "Курсы валют на {0} не найдены.")]
        [DataRow("uk", "Курси валют на {0} не знайдено.")]
        public async Task HandleInputDate_NullReferenceException_RatesNotFound(string languageCode, string expectedResponse)
        {
            // Arrange
            var userData = new UserData { LanguageCode = languageCode, SelectedCurrency = "USD" };
            var messageText = new DateTime(2021, 05, 15);

            _currencyServiceMock.Setup(x => x.GetCurrencyInfoAsync(userData.SelectedCurrency, messageText, userData.LanguageCode))
                       .ThrowsAsync(new NullReferenceException(expectedResponse));

            var expectedFormattedResponseMessage = string.Format
                (expectedResponse
                , messageText.ToString(DATE_FORMAT)
                , userData.SelectedCurrency);

            // Act
            var result = await _commandHandlerService.HandleCommand("", CHAT_ID, messageText.ToString(DATE_FORMAT), userData);

            // Assert
            Assert.AreEqual(expectedFormattedResponseMessage, result.ResponseMessage);
        }

        [TestMethod]
        [DataRow("en", "Failed to retrieve exchange rate data.")]
        [DataRow("ru", "Не удалось получить данные о курсах валют.")]
        [DataRow("uk", "Не вдалося отримати дані про курси валют.")]
        public async Task HandleInputDate_HttpRequestException(string languageCode, string expectedResponse)
        {
            // Arrange
            var userData = new UserData { LanguageCode = languageCode, SelectedCurrency = "USD" };
            var messageText = new DateTime(2021, 05, 15);

            _currencyServiceMock.Setup(x => x.GetCurrencyInfoAsync(userData.SelectedCurrency, messageText, userData.LanguageCode))
                       .ThrowsAsync(new HttpRequestException(expectedResponse));

            // Act
            var result = await _commandHandlerService.HandleCommand("", CHAT_ID, messageText.ToString(DATE_FORMAT), userData);

            // Assert
            Assert.AreEqual(expectedResponse, result.ResponseMessage);
        }

        [TestMethod]
        [DataRow("en", "Error in processing data on exchange rates.")]
        [DataRow("ru", "Ошибка при обработке данных о курсах валют.")]
        [DataRow("uk", "Помилка при обробці даних про курси валют.")]
        public async Task HandleInputDate_JsonException(string languageCode, string expectedResponse)
        {
            // Arrange
            var userData = new UserData { LanguageCode = languageCode, SelectedCurrency = "USD" };
            var messageText = new DateTime(2021, 05, 15);

            _currencyServiceMock.Setup(x => x.GetCurrencyInfoAsync(userData.SelectedCurrency, messageText, userData.LanguageCode))
                       .ThrowsAsync(new JsonException(expectedResponse));

            // Act
            var result = await _commandHandlerService.HandleCommand("", CHAT_ID, messageText.ToString(DATE_FORMAT), userData);

            // Assert
            Assert.AreEqual(expectedResponse, result.ResponseMessage);
        }
    }
}