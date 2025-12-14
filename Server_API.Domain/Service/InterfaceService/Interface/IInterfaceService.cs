using Server_API.Domain.Model.BB;

namespace Server_API.Domain.Service.InterfaceService.Interface
{
    internal interface IInterfaceService
    {
        public ProcessedData ProcessAllStatments(string statementFilePath, string expenseFilePath, string finalFilePath);
    }
}