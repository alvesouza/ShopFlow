using System.Net.Mail;

namespace ShopFlow.Application.Interfaces;
public class EmailSender: IEmailSender
{    
    private string _smtpHost;
    private int _smtpPort;
    private string _emailFrom;

    public EmailSender( string smtpHost, int smtpPort, string emailFrom )
    {
        _smtpHost = smtpHost;
        _smtpPort = smtpPort;
        _emailFrom= emailFrom;
    }
    public void SendEmail( string emailTo, string subject, string body )
    {
        Console.BackgroundColor = ConsoleColor.Blue;
        Console.WriteLine(
            $"smtpHost = {_smtpHost}\n_smtpPort = {_smtpPort}\n" +
            $"emailFrom = {_emailFrom}\n"
        );
        Console.BackgroundColor = ConsoleColor.Green;
        Console.WriteLine(
            $"Emailed to {emailTo} the email with subject: {subject}\n" +
            $"and body:\n{body}"
        );
        Console.ResetColor();
    }
}