using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;

namespace BookShoppingCartMvcUI.Shared
{
    public interface IEmailHelper
    {
        Task SendEmailAsync(EmailRequest emailRequest);
    }
    public class EmailHelper : IEmailHelper
    {
        EmailConfig _emailConfig;

        public EmailHelper(IOptions<EmailConfig> emailConfig)
        {
            _emailConfig = emailConfig.Value;
        }

        public async Task SendEmailAsync(EmailRequest emailRequest)
        {
            try
            {
                SmtpClient smtpClient = new SmtpClient(_emailConfig.Provider, _emailConfig.Port);
                smtpClient.Credentials = new NetworkCredential(_emailConfig.DefaultSender, _emailConfig.Password);
                smtpClient.UseDefaultCredentials = false;
                smtpClient.EnableSsl = true;

                MailMessage mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(_emailConfig.DefaultSender, "BookStore by ThiLK");
                mailMessage.To.Add(emailRequest.To);
                mailMessage.IsBodyHtml = true;
                mailMessage.Subject = emailRequest.Subject;
                mailMessage.Body = emailRequest.Content;

                if (emailRequest.AttachmentFilePaths.Length > 0)
                {
                    foreach (var path in emailRequest.AttachmentFilePaths)
                    {
                        Attachment attachment = new Attachment(path);

                        mailMessage.Attachments.Add(attachment);
                    }
                }

                await smtpClient.SendMailAsync(mailMessage);
                mailMessage.Dispose();
            }
            catch (Exception ex)
            {
                //log ex
                throw;
            }

        }

    }
}
