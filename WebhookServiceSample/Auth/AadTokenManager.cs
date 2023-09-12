namespace WebhookServiceSample.Auth
{
    using Microsoft.Identity.Client;
    using System.Threading.Tasks;

    public class AadTokenManager
    {
        private string ClientId = "";
        private string ClientSecret = "";
        private string TenantId = "";

        private IConfidentialClientApplication AppInstance;

        private static AadTokenManager TokenManagerInstance;

        public static AadTokenManager GetInstance()
        {
            if (TokenManagerInstance == null)
            {
                TokenManagerInstance = new AadTokenManager();
            }

            return TokenManagerInstance;
        }

        private AadTokenManager()
        {
            AppInstance = ConfidentialClientApplicationBuilder.Create(ClientId).WithClientSecret(ClientSecret).Build();
        }

        public async Task<string> GetAadTokenAsync()
        {
            var authResult = await AppInstance.AcquireTokenForClient(scopes: new[] { "https://canary.graph.microsoft.com/.default" })
                               .WithAuthority(AzureCloudInstance.AzurePublic, TenantId)
                               .ExecuteAsync();
            return authResult.AccessToken;
        }
    }
}