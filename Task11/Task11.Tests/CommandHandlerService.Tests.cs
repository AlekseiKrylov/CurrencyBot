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
        [DataRow("ru", "������! ��� ��������� �������� ����� ����� � ������ �� �����������.", "������,����,������")]
        [DataRow("uk", "�����! ��� �������� ���������� ����� ����� �� ����� �� �����������.", "������,����,��������")]
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
        [DataRow("ru", "�������� ������ �� �������� ���������� ��� ��������� � ��� ��� ������.\r\n������ ��������� �����: {0}", "USD,EUR")]
        [DataRow("uk", "������� ������ �� ������� �������� ��� ������� � ��� ��� ������.\r\n������ ��������� �����: {0}", "USD,EUR")]
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
        [DataRow("ru", "�������� ���� �� �������� ���������� ��� ��������� � ��� ���� � ������� dd.MM.yyyy. ������ 15.05.2021", "�����,�������")]
        [DataRow("uk", "������� ���� �� ������� �������� ��� ������� � ��� ���� � ������ dd.MM.yyyy. ������� 15.05.2021", "�����,�������")]
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
        [DataRow("ru", "������ {0} ����������. ���������� ��� ���.\r\n������ ��������� �����: {1}", "USD,EUR")]
        [DataRow("uk", "������ {0} ����������. ��������� �� ���.\r\n������ ��������� �����: {1}", "USD,EUR")]
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
        [DataRow("ru", "����: {0}\r\n������: {1}\r\n���� �������: {2} {4}\r\n���� �������: {3} {4}", "���������")]
        [DataRow("uk", "����: {0}\r\n������: {1}\r\n���� �����: {2} {4}\r\n���� �������: {3} {4}", "���������")]
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
        [DataRow("ru", "������������ ������ ����. ����������, ����������� ������ dd.MM.yyyy", "�����,�������")]
        [DataRow("uk", "����������� ������ ����. ���� �����, �������������� ������ dd.MM.yyyy", "�����,�������")]
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
        [DataRow("ru", "���� ��� {1} �� {0} �� ������.")]
        [DataRow("uk", "���� ��� {1} �� {0} �� ��������.")]
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
        [DataRow("ru", "����� ����� �� {0} �� �������.")]
        [DataRow("uk", "����� ����� �� {0} �� ��������.")]
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
        [DataRow("ru", "�� ������� �������� ������ � ������ �����.")]
        [DataRow("uk", "�� ������� �������� ��� ��� ����� �����.")]
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
        [DataRow("ru", "������ ��� ��������� ������ � ������ �����.")]
        [DataRow("uk", "������� ��� ������� ����� ��� ����� �����.")]
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