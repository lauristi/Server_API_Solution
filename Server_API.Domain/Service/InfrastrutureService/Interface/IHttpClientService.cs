namespace Server_API.Domain.Service.InfrastrutureService.Interface
{
    public interface IHttpClientService
    {
        Task<HttpResponseMessage> GetAsync(string url);
    }
}