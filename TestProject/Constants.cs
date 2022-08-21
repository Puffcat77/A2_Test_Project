namespace TestProject
{
    internal class Constants
    {
        public const string DB_CONNECTION_STRING = "dbConnectionString";
        /// <summary>
        /// 0 - обработке ...
        /// 1 - ex.Message
        /// 2 - ex.StackTrace
        /// </summary>
        public const string ERROR_TEXT = "Произошла ошибка при обработке {0}: {1} - {2}";
        public const string VOLUME_DB_FORMAT = "0.0000";
        public const int DEAL_NUMBER_LENGTH = 40;
        public const int WOOD_VOLUME_LENGTH = 15;
        public const int WOOD_VOLUME_FLOAT_LENGTH = 4;
        public const int DEAL_DATE_LENGTH = 7;
        public const int PAGE_SIZE = 5000;
        public const int REQUEST_TIMEOUT = 1000;
        public const int REQUEST_FAIL_TIMEOUT = 60000;
        /// <summary>
        /// Переры между обходами
        /// </summary>
        public const int FETCH_TIMEOUT = 600000;
        public const int PACKAGE_SIZE = 1000;
    }
}