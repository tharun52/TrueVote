using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using TrueVote.Interfaces;

namespace TrueVote.Service
{
    public class KeyVaultService : IKeyVaultService
    {
         private readonly IConfiguration _configuration;
        private readonly SecretClient _secretClient;

        public KeyVaultService(IConfiguration configuration)
        {
            _configuration = configuration;
            var vaultUrl = _configuration["AzureKeyVault:VaultUrl"];
            if (vaultUrl == null)
            {
                throw new Exception("VaultUrl not found");
            }
            _secretClient = new SecretClient(new Uri(vaultUrl), new DefaultAzureCredential());
        }

        public async Task<string> GetSasUrlAsync(string secretName)
        {
            KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);
            return secret.Value;
        }
    }
}