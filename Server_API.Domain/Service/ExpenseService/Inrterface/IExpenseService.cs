using Server_API.Domain.Model.BB.BLL;

namespace Server_API.Domain.Service.ExpenseService.Inrterface
{
    public interface IExpenseService
    {
        List<Expense> LoadExpensesList(string expenseFile);
    }
}