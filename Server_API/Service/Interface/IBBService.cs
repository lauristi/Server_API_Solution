namespace Server_API.Service.Interface
{
    public interface IBBService
    {
        string? ProcessStatment(string statementFilePath, string expenseFilePath, string finalFilePath);

        string ConvertCsvToXls(string csvFilePath, string xlsFilePath);

        decimal ProcessMonthSpending(string statementFilePath);
    }       
        
}