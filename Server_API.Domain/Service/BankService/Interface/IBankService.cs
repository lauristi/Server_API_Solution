using Server_API.Domain.Model.BB;

namespace Server_API.Domain.Service.BankService.Interface
{
    public interface IBankService
    {
        ProcessedData ProcessRawBankDetails(string statementFilePath, string expenseFilePath, string finalFilePath);
    }
}