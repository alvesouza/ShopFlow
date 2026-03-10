namespace ShopFlow.Application.Interfaces;
public interface IEmailSender
{
        public void SendEmail(string emailTo, string subject, string body){}
}