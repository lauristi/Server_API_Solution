namespace Server_API.Domain.Infrastructure.Interface
{
    public interface ICrypto
    {
        string Encrypt(string input);

        string Decrypt(string input);
    }
}