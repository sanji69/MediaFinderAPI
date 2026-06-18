namespace MediaFinder.Interface
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string toEmail, string confirmationUrl, string? language = null);
        Task SendPasswordResetAsync(string to, string resetUrl, string? language = null);
    }
}
