namespace Server_API.Domain.Service.InfrastrutureService.Interface
{
    public interface INormalizeService
    {
        string NormalizeString(string text);

        string NormalizeStringSize(string text, int size);

        string NormalizeValue(string value);

        decimal NormalizeToDecimal(string value);
    }
}