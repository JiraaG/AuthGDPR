namespace AuthGDPR.Application.Interfaces
{
    public interface IEmailCustomSender
    {
        Task SendEmailAsync(string email, string subject, string htmlMessage);
    }
}
