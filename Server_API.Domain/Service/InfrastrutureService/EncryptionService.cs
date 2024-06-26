using Server_API.Domain.Infrastructure.Interface;
using Server_API.Domain.Service.InfrastrutureService.Interface;

namespace Server_API.Domain.Service.InfrastrutureService
{
    public class EncryptionService : IEncryptionService
    {
        private readonly ICrypto _crypto;

        public EncryptionService(ICrypto crypto)
        {
            _crypto = crypto;
        }

        public string Encrypt(string data)
        {
            return _crypto.Encrypt(data);
        }

        public string Decrypt(string data)
        {
            return _crypto.Decrypt(data);
        }
    }
}