namespace Server_API.Service.Interface
{
    public interface IBankStatementService
    {
        string? ProcessBankStatement(string statementFilePath, string expenseFilePath, string finalFilePath);

        string ConvertCsvToXls(string csvFilePath, string xlsFilePath);
    }
}