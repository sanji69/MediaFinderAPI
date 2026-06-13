namespace MediaFinder.Interface
{
    public interface IEmailService
    {
        Task SendEmailConfirmationAsync(string toEmail, string confirmationUrl, string? language = null);
    }
}
