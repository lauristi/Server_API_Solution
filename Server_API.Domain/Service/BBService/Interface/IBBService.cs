using Server_API.Domain.Model.BB;

namespace Server_API.Domain.Service.BBService.Interface
{
    public interface IBBService
    {
        ProcessedData ProcessBBStatment(string statementFilePath, string expenseFilePath, string finalFilePath);
    }
}