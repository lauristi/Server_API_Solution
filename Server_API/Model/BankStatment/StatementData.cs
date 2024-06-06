namespace Server_API.Model
{
    public class StatementData
    {
        public String? Date { get; set; }
        public String? Subject { get; set; }
        public String? Value { get; set; }
        public String? Type { get; set; }
        public String? Score { get; set; }

        public string LineCSV()
        {
            return $"{Date};{Subject};{Value};{Type};{Score}";
        }
    }
}