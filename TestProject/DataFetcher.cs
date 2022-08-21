using System.Net.Http;
using TestProject.DataModel;
using System.Text.Json;
using System.Text;
using System.Threading;


namespace TestProject
{
    internal class DataFetcher
    {
        private const string REQUEST_URL = "https://www.lesegais.ru/open-area/graphql";
        private const string USER_AGENT = "Mozilla/4.0 (Compatible; Windows NT 5.1; MSIE 6.0) (compatible; MSIE 6.0; Windows NT 5.1; .NET CLR 1.1.4322; .NET CLR 2.0.50727)";
        

        private HttpClient client;
        private RequestBody requestBody;

        public DataFetcher()
        {
            client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", USER_AGENT);
            requestBody = new RequestBody();
        }

        public Page GetDataPage(int pageNumber)
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    string requestBodyStr = JsonSerializer.Serialize(FormRequestBody(pageNumber));
                    StringContent httpContent =
                        new StringContent(requestBodyStr, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(REQUEST_URL, httpContent).Result;
                    string responseString = response.Content.ReadAsStringAsync().Result;
                    Page dataPage = JsonSerializer.Deserialize<Page>(responseString);
                    Thread.Sleep(Constants.REQUEST_TIMEOUT);
                    return dataPage;
                }
                catch (System.Exception ex)
                {
                    UI.LogError("Произошла ошибка при получении данных из источника:");
                    UI.LogError(ex);
                    UI.LogError("Попытка №1");
                    Thread.Sleep(Constants.REQUEST_FAIL_TIMEOUT);
                    if (i == 4)
                    {
                        throw ex;
                    }
                }
            }
            return null;
        }

        private RequestBody FormRequestBody(int pageNumber)
        {
            requestBody.Variables.Number = pageNumber;
            return requestBody;
        }
    }
}