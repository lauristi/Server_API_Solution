namespace Server_API.Model
{
    public class StatementData
    {
        public String? Date { get; set; }
        public String? Subject { get; set; }

        public String? Type { get; set; }
        public String? Score { get; set; }
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