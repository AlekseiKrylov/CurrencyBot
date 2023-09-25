using System.Globalization;
using System.Resources;

namespace Task11.Resources
{
    public static class ResourceKeys
    {
        public enum RKeys
        {
            #region User Messages

            WelcomeMessage,
            CurrencyPromptMessage,
            DatePromptMessage,
            InvalidCurrencyMessage,
            InvalidDateMessage,
            ExchangeCourseMessage,
            UnknownСommandMessage,
            HelpMessage,
            DefaultMessage,
            SelectOptionMessage,

            #endregion

            #region Caution Messages

            /// <summary>
            /// Text: No data
            /// </summary>
            NoDataCaution,
            /// <summary>
            /// Text: {1} exchange rate for {0} was not found. 0 - Date, 1 - CurrencyCode
            /// </summary>
            RateNotFoundCaution,
            /// <summary>
            /// Text: Currency rates for {0} were not found. 0 - Date.
            /// </summary>
            RatesNotFoundCaution,

            #endregion

            #region Error Messages

            /// <summary>
            /// Text: There was an error in processing your request.
            /// </summary>
            RequestProcessingError,
            /// <summary>
            /// Text: Failed to retrieve exchange rate data.
            /// </summary>
            FailedRetrieveDataError,
            /// <summary>
            /// Text: Error in processing data on exchange rates.
            /// </summary>
            ProcessingDataError,

            #endregion

            #region Menu Items

            Go,
            Language,
            Help,
            Yesterday,
            Today,
            Repeat,

            #endregion

            /// <summary>
            /// Text: This is a test message. If you see it, please notify the developer.
            /// </summary>
            PlaceholderMessage
        }

        public static ResourceManager ResourceManager { get; private set; }

        public static void InitializeResourceManager(ResourceManager resourceManager) => ResourceManager = resourceManager;

        public static string GetLocalizedMessage(RKeys resourceKey, string languageCode)
        {
            if (!string.IsNullOrWhiteSpace(languageCode))
                return ResourceManager.GetString(resourceKey.ToString(), new CultureInfo(languageCode))
                    ?? $"Something's wrong! {resourceKey}";

            return ResourceManager.GetString(resourceKey.ToString(), CultureInfo.InvariantCulture)
                    ?? $"Something's wrong! {resourceKey}";
        }
    }
}
