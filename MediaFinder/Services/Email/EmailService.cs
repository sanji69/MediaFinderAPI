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
        public async Task SendPasswordResetAsync(string toEmail, string resetUrl, string? language = null)
        {
            var resolvedLanguage = _localizationService.ResolveLanguage(language);

            var isFrench = resolvedLanguage.StartsWith(
                "fr",
                StringComparison.OrdinalIgnoreCase);

            var subject = isFrench
                ? "Réinitialisation de votre mot de passe"
                : "Reset your password";

            var body = isFrench
               ? $"""
                  Bonjour,

                  Vous avez demandé la réinitialisation de votre mot de passe.

                  Cliquez sur le lien suivant pour choisir un nouveau mot de passe :
                  {resetUrl}

                  Ce lien expirera dans 1 heure.

                  Si vous n'êtes pas à l'origine de cette demande, ignorez cet email.
                  """
               : $"""
                  Hello,

                  You requested a password reset.

                  Click the following link to choose a new password:
                  {resetUrl}

                  This link will expire in 1 hour.

                  If you did not request this, you can ignore this email.
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
