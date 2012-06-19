using System;
using System.Net.Mail;

namespace MyApplicationComponent
{
    public class SendEmail
    {
        public SendEmailStatus Send(EmailInformation emailInfo)
        {
            var status = new SendEmailStatus();
            try
            {
                using (var smtpClient = new SmtpClient())
                {
                    using (var mailMsg = new MailMessage())
                    {
                        mailMsg.From = new MailAddress(emailInfo.FromAddress, emailInfo.FromName);
                        mailMsg.To.Add(new MailAddress(emailInfo.ToAddress, emailInfo.ToName));
                        mailMsg.Subject = emailInfo.Subject;
                        mailMsg.Body = emailInfo.MessageText;
                        mailMsg.IsBodyHtml = emailInfo.IsHtmlMessage;

                        smtpClient.Send(mailMsg);

                        status.WasSent = true;
                    }
                }
            }
            catch (Exception ex)
            {
                status.ErrorMessage = ex.Message;
            }
            return status;
        }
    }
}
