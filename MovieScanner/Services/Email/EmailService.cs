using System.Net.Mail;
using MediaFinder.Interface;
using MediaFinder.Options;
using Microsoft.Extensions.Options;

namespace MediaFinder.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly SmtpOptions _options;
        private readonly ILocalizationService _localizationService;

        public EmailService(IOptions<SmtpOptions> options, ILocalizationService localizationService)
        {
            _options = options.Value;
            _localizationService = localizationService;
        }

        public async Task SendEmailConfirmationAsync(string toEmail, string confirmationUrl, string? language = null)
        {
            var resolvedLanguage = _localizationService.ResolveLanguage(language);

            var isFrench = resolvedLanguage.StartsWith(
                "fr",
                StringComparison.OrdinalIgnoreCase);

            var subject = isFrench
                ? "Confirmez votre compte MediaFinder"
                : "Confirm your MediaFinder account";

            var body = isFrench
                ? $"""
                    Bonjour,

                    Merci de vous être inscrit sur MediaFinder.

                    Cliquez sur le lien suivant pour confirmer votre compte :

                    {confirmationUrl}

                    Si vous n'êtes pas à l'origine de cette inscription, ignorez cet email.
                    """
                : $"""
                    Hello,

                    Thank you for signing up to MediaFinder.

                    Click the following link to confirm your account:

                    {confirmationUrl}

                    If you did not create this account, you can ignore this email.
                    """;

            using var client = new SmtpClient(_options.Host, _options.Port);

            using var message = new MailMessage
            {
                From = new MailAddress(_options.FromAddress, _options.FromName),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };

            message.To.Add(toEmail);

            await client.SendMailAsync(message);
        }
    }
}
