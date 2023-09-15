namespace WebhookServiceSample.Repositories
{
    using Newtonsoft.Json;
    using System.Net.Http.Headers;
    using System.Threading.Tasks;
    using WebhookServiceSample.Auth;
    using WebhookServiceSample.Dtos;

    public class ApiDataRepository
    {
        private readonly IConfigurationRoot _config;

        public ApiDataRepository(IConfigurationRoot config)
        {
            _config = config;
        }

        public async Task<SearchResultDTO> GetReportingDataAsync()
        {
            AadTokenManager tokenManager = AadTokenManager.GetInstance(_config);
            var token = await tokenManager.GetAadTokenAsync();

            HttpClient httpClient = new();
            httpClient.DefaultRequestHeaders.Accept.Clear();

            httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            HttpResponseMessage response = await httpClient.GetAsync("https://canary.graph.microsoft.com/testprodbeta_Intune_SH/deviceManagement/managedDevices?$filter=complianceState%20eq%20'noncompliant'&$select=id,complianceGracePeriodExpirationDateTime,deviceName");

            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();
            SearchResultDTO searchResult = JsonConvert.DeserializeObject<SearchResultDTO>(responseBody);

            return searchResult;
        }
    }
}