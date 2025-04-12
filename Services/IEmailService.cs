namespace dagstore.Services
{
    public interface IEmailService
    {
        Task SendVerificationEmail(string email, string verificationLink);
    }
} 