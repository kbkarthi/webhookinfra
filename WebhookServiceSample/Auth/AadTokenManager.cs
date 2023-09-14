namespace WebhookServiceSample.Auth
{
    using Microsoft.Identity.Client;
    using System.Threading.Tasks;
    using Azure.Identity;
    using Azure.Security.KeyVault.Secrets;

    public class AadTokenManager
    {
        private string ClientId = "";
        private string ClientSecret = "";
        private string TenantId = "";

        private IConfidentialClientApplication AppInstance;

        private static AadTokenManager TokenManagerInstance;

        public static AadTokenManager GetInstance(bool usingKv = false)
        {
            if (TokenManagerInstance == null)
            {
                TokenManagerInstance = new AadTokenManager(usingKv);
            }

            return TokenManagerInstance;
        }

        private AadTokenManager(bool usingKv = false)
        {
            if (usingKv)
            {
                var kvUri = "https://kabaluwebhookkv.vault.azure.net/";
                var client = new SecretClient(new Uri(kvUri), new DefaultAzureCredential());
                this.ClientId = Task.Run(() => client.GetSecretAsync(nameof(ClientId))).Result?.Value?.Value ?? "";
                this.ClientSecret = Task.Run(() => client.GetSecretAsync(nameof(ClientSecret))).Result?.Value?.Value ?? "";
                this.TenantId = Task.Run(() => client.GetSecretAsync(nameof(TenantId))).Result?.Value?.Value ?? "";
            }

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