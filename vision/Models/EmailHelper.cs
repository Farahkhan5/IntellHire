
    using System.Net;
    using System.Net.Mail;

    namespace vision.Helpers
    {
        public class EmailHelper
        {
            public static void SendEmail(string toEmail, string subject, string body)
            {
                try
                {
                    var smtp = new SmtpClient("smtp.gmail.com") // Gmail use kar rahe hain
                    {
                        Port = 587,
                        Credentials = new NetworkCredential("your-email@gmail.com", "your-app-password"),
                        EnableSsl = true
                    };

                    var message = new MailMessage("your-email@gmail.com", toEmail, subject, body);
                    message.IsBodyHtml = true; // HTML email support

                    smtp.Send(message);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Email sending failed: " + ex.Message);
                }
            }
        }
    }


