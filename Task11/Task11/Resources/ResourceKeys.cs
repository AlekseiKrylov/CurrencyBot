using System.Globalization;
using System.Resources;

namespace Task11.Resources
{
    internal static class ResourceKeys
    {
        #region User Messages

        public const string WelcomeMessage = "WelcomeMessage";
        public const string CurrencyPromptMessage = "CurrencyPromptMessage";
        public const string DatePromptMessage = "DatePromptMessage";
        public const string InvalidCurrencyMessage = "InvalidCurrencyMessage";
        public const string InvalidDateMessage = "InvalidDateMessage";
        public const string ExchangeCourseMessage = "ExchangeCourseMessage";
        public const string UnknownСommandMessage = "UnknownСommandMessage";
        public const string HelpMessage = "HelpMessage";
        public const string DefaultMessage = "DefaultMessage";

        public const string PlaceholderMessage = "PlaceholderMessage";

        #endregion

        #region Caution Messages

        public const string NoDataCaution = "NoDataCaution";

        #endregion

        #region Error Messages

        public const string RequestProcessingError = "RequestProcessingError";

        #endregion

        public enum RKeys
        {
            WelcomeMessage,
            CurrencyPromptMessage,
            DatePromptMessage,
            InvalidCurrencyMessage,
            InvalidDateMessage,
            ExchangeCourseMessage,
            UnknownСommandMessage,
            HelpMessage,
            DefaultMessage,
            PlaceholderMessage,
            NoDataCaution,
            RequestProcessingError,
            RateNotFoundCaution,
            RatesNotFoundCaution,
            FailedRetrieveDataError,
            ProcessingDataError
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

        public static string GetLocalizedMessage(string resourceKey, string languageCode)
        {
            if (!string.IsNullOrWhiteSpace(languageCode))
                return ResourceManager.GetString(resourceKey, new CultureInfo(languageCode))
                    ?? $"Something's wrong! {resourceKey}";

            return ResourceManager.GetString(resourceKey, CultureInfo.InvariantCulture)
                    ?? $"Something's wrong! {resourceKey}";
        }
    }
}
