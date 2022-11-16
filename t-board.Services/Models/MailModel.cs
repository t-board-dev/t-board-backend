namespace t_board.Services.Models
{
    public class MailModel
    {
        public string Subject { get; set; }
        public string Body { get; set; }
        public string To { get; set; }
        public string Cc { get; set; } = string.Empty;
    }
}
