namespace Server_API.Infrastructure
{
    public class MultiPartResponse
    {
        public string JsonContent { get; set; }
        public byte[] FileContent { get; set; }
    }
}