using BCrypt.Net;
using TrueVote.Interfaces;
using TrueVote.Models;

namespace TrueVote.Service
{
    public class EncryptionService : IEncryptionService
    {
        public async Task<EncryptModel> EncryptData(EncryptModel data)
        {
            return await Task.Run(() =>
            {
                string plainText = data.Data ?? string.Empty;

                if (string.IsNullOrEmpty(data.HashKey))
                {
                    data.HashKey = BCrypt.Net.BCrypt.GenerateSalt();
                }

                data.EncryptedText = BCrypt.Net.BCrypt.HashPassword(plainText, data.HashKey);

                return data;
            });
        }
    }
}
