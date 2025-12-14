using Server_API.Domain.Model.BB;
using Server_API.Domain.Model.BB.BLL;
using Server_API.Domain.Model.BB.Spending;

namespace Server_API.Domain.Service.ProcessStatementService.Interface
{
    public interface IProcessStatementService
    {
        SpendingData ProcessSubject(SpendingData spendingData, List<Expense> expenses);

        ProcessedData ProcessTotalKnowSpending(List<SpendingData> spendingDataList);
    }
}