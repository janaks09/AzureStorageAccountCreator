using Microsoft.Azure.Management.Storage;
using Microsoft.Azure.Management.Storage.Models;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorageAccountCreator
{
    class Program
    {
        static void Main(string[] args)
        {
            var subscriptionId = "[SubscriptionId]";
            var resourceGroup = "[Resoucre Grroup]";
            var StorageManagement = new StorageManagementClient(new CustomLoginCredentials());
            StorageManagement.SubscriptionId = subscriptionId;

            var re = StorageManagement.StorageAccounts.CreateAsync(resourceGroup, "[STORAGE_NAME]", new StorageAccountCreateParameters()
            {
                Location = "East US",
                AccessTier = AccessTier.Hot,
                Kind = Kind.StorageV2,
                Sku = new Sku { Name = SkuName.StandardZRS }

            }, new CancellationToken() { }).Result;

            Console.WriteLine("Successfully Created");
            Console.ReadKey();
        }
    }

    public class CustomLoginCredentials : ServiceClientCredentials
    {
        readonly string tenantId = "[App TenantId]";
        readonly string clientId = "[ClientId]";
        readonly string secretKey = "[Client Secret]";
        private string AuthenticationToken { get; set; }
        public override void InitializeServiceClient<T>(ServiceClient<T> client)
        {
            var authenticationContext = new AuthenticationContext($"https://login.windows.net/{tenantId}");
            var credential = new ClientCredential(clientId: clientId, clientSecret: secretKey);

            var result = authenticationContext.AcquireTokenAsync(resource: "https://management.core.windows.net/", clientCredential: credential).Result;

            if (result == null)
            {
                throw new InvalidOperationException("Failed to obtain the JWT token");
            }

            AuthenticationToken = result.AccessToken;
        }
        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (AuthenticationToken == null)
            {
                throw new InvalidOperationException("Token Provider Cannot Be Null");
            }

            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", AuthenticationToken);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            await base.ProcessHttpRequestAsync(request, cancellationToken);

        }
    }

}
