using static Server_API.Domain.Model.BB.Enumeradores;

namespace Server_API.Domain.Model.BB.Spending
{
    public class SpendingData
    {
        public string? Date { get; set; }
        public string? Subject { get; set; }

        public string? Type { get; set; }
        public string? Score { get; set; }

        //------------------------------------------
        public string StringValue { get; set; }

        public string GrossValue { get; set; }
        public Decimal DecimalValue { get; set; }

        //------------------------------------------
        public bool IsCredit { get; set; }

        public FINANCIAL_TYPE FinancialType { get; set; }

        public string LineCSV()
        {
            return $"{Date};{Subject};{StringValue};{Type};{Score}";
        }
    }
}