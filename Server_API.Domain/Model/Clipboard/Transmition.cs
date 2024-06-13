namespace Server_API.Model.clipboard
{
    public class Transmition
    {
        private string? _content;

        public string? Content
        {
            get => _content ?? "";
            set => _content = value;
        }
    }
}