namespace TestProject.DataModels
{
    internal class SourceCompany
    {

        private string inn;

        public string INN
        {
            get
            {
                return GetValidINN(inn);
            }
            set
            {
                inn = value ?? "";
            }
        }

        private string GetValidINN(string inn)
        {
            if (Name?.ToLower() == "физ лицо")
            {
                return "";
            }
            if (string.IsNullOrWhiteSpace(inn.Replace("0", "")))
            {
                return "0";
            }
            return inn;
        }

        private string name;

        public string Name
        {
            get
            {
                return name ?? "";
            }
            set
            {
                name = value;
            }
        }

        public bool IsRussianINN
        {
            get
            {
                return INN.Length == 12 || INN.Length == 10;
            }
        }
    }
}