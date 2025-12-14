using Server_API.Domain.Model.BB.Spending;
using Server_API.Domain.Service.Enums;

namespace Server_API.Domain.Service.InfrastrutureService.Interface
{
    public interface IXlsService
    {
        string ConvertCsvToXls(string csvFilePath, string xlsFilePath);

        bool CreateNewFileCSV(string finalFilePath, List<SpendingData> spendingData);

        bool CreateNewFileXLS(string xlsFilePath, List<SpendingData> spendingData);

        string CreateXlsArchiveName(BankType bankType, string dateString, string extension);
    }
}