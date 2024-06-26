using Server_API.Domain.Model.BB.Spending;

namespace Server_API.Domain.Service.InfrastrutureService.Interface
{
    public interface IXlsService
    {
        string ConvertCsvToXls(string csvFilePath, string xlsFilePath);

        bool CreateNewFileCSV(string finalFilePath, List<SpendingData> spendingData);

        bool CreateNewFileXLS(string xlsFilePath, List<SpendingData> spendingData);

        string CreateXlsArchiveName(string dateString, string extension);
    }
}