namespace TrueVote.Interfaces
{
    public interface IKeyVaultService
    {
        Task<string> GetSasUrlAsync(string secretName);
    }
}