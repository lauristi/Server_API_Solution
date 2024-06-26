using Server_API.Domain.Model.BB.BLL;
using Server_API.Domain.Service.BBService.Interface;

namespace Server_API.Domain.Service.BBService
{
    public class ExpenseService : IExpenseService
    {
        public List<Expense> LoadExpensesList(string expenseFile)
        {
            List<Expense> expenses = new List<Expense>();

            try
            {
                //CARREGO AS CONVERSOES
                string[] lines = File.ReadAllLines(expenseFile);

                foreach (string line in lines)
                {
                    Expense expense = new Expense();

                    string cleanLine = line.Replace("\"", "");
                    string[] aItem = cleanLine.Split(';');

                    expense.Origin = aItem[0];
                    expense.Owner = aItem[1];
                    //------------------------------------
                    expenses.Add(expense);
                }

                return expenses;
            }
            catch (Exception)
            {
                return expenses;
            }
        }
    }
}