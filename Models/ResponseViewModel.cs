namespace GSRecordMining.Models
{
    public class ResponseViewModel
    {
        public int statusCode { get; set; }
        public string message { get; set; } = "";
        public object data { get; set; }
    }
}
