namespace WebhookServiceSample.Auth
{
    using Microsoft.Identity.Client;
    using System.Threading.Tasks;

    public class AadTokenManager
    {
        private IConfidentialClientApplication AppInstance;
        private readonly IConfigurationRoot _config;
        private static AadTokenManager TokenManagerInstance;

        public static AadTokenManager GetInstance(IConfigurationRoot config)
        {
            if (TokenManagerInstance == null)
            {
                TokenManagerInstance = new AadTokenManager(config);
            }

            return TokenManagerInstance;
        }

        private AadTokenManager(IConfigurationRoot config)
        {
            _config = config;
            AppInstance = ConfidentialClientApplicationBuilder.Create(_config["ClientId"]).WithClientSecret(_config["ClientSecret"]).Build();
        }

        public async Task<string> GetAadTokenAsync()
        {
            var authResult = await AppInstance.AcquireTokenForClient(scopes: new[] { "https://canary.graph.microsoft.com/.default" })
                               .WithAuthority(AzureCloudInstance.AzurePublic, _config["TenantId"])
                               .ExecuteAsync();
            return authResult.AccessToken;
        }
    }
}