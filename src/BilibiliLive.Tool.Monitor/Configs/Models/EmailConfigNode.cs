namespace BilibiliLive.Tool.Monitor.Configs.Models
{
    public class EmailConfigNode
    {
        public string SmtpServer { get; set; }

        public bool SmtpSsl { get; set; }

        public uint SmtpPort { get; set; }

        public string SendMailAddress { get; set; }

        public string SendMailName { get; set; }

        public string Password { get; set; }

        public string ReceiveMailAddress { get; set; }

        public static string Position { get { return "AppSettings:EmailConfig"; } }

    }
}
