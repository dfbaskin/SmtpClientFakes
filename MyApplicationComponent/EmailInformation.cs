
namespace MyApplicationComponent
{
    public class EmailInformation
    {
        public string FromName { get; set; }
        public string FromAddress { get; set; }
        public string ToName { get; set; }
        public string ToAddress { get; set; }
        public string Subject { get; set; }
        public string MessageText { get; set; }
        public bool IsHtmlMessage { get; set; }
    }
}
