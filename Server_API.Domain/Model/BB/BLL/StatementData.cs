namespace Server_API.Domain.Model.BB.BLL
{
    public class StatementData
    {
        public string? Date { get; set; }
        public string? Subject { get; set; }

        public string? Type { get; set; }
        public string? Score { get; set; }
        public bool IsCredit { get; set; }

        //Evita aviso chato de null
        private string? _value;

        public string Value
        {
            get => _value ?? "0";
            set => _value = value;
        }

        public string LineCSV()
        {
            return $"{Date};{Subject};{Value};{Type};{Score}";
        }
    }
}