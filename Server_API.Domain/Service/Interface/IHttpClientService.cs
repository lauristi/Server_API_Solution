namespace Server_API.Domain.Service.Interface
{
    public interface IHttpClientService
    {
        Task<HttpResponseMessage> GetAsync(string url);
    }
}