using backend.Interfaces;
using backend.Models;
using backend.Repositories;
using System.Net;
using System.Net.Mail;
namespace backend.Services
{
    public class EmailEmployeeNotificationService : IEmailVerificationMessageService
    {

        public EmailEmployeeNotificationService()
        {

        }

        public void buildEmail(UserModel request)
        {
            var token = Guid.NewGuid().ToString();

            DateTime expiration = DateTime.Now.AddDays(3);

            var mailTokenVerification = new MailModel
            {
                userID = request.Id,
                token = token,
                experationDate = expiration
            };

            var mailRepository = new MailRepository();

            this.sendEmail(mailRepository, mailTokenVerification, request);
        }
        public void sendEmail(MailRepository mailRepository, MailModel mailTokenVerification, UserModel request)
        {
            mailRepository.insertMailNoty(mailTokenVerification);

            Console.WriteLine("Query realizado con éxito\n");

            var verificationLink = $"http://localhost:5081/api/user/verify?token={mailTokenVerification.token}";

            var mailAddr = new MailAddress("pruebadisenosoft@gmail.com", "Fiesta Fries");

            var sendTo = new MailAddress(request.Email.Trim());

            const string password = "rxhd qmzc uvxi sxmg";

            const string subject = "Verificación de invitacón de empleado Fiesta Fries";

            string body = $"¡Saludos cordiales!, somos la gente de Fiesta Fries enviandote el enlace de verficación de Empleado para finalizar el proceso de creación de tu cuenta: {verificationLink} tus credenciales de inicio de sesión son las siguientes: email: {request.Email.Trim()} constraseña:{request.PasswordHash.Trim()}";

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(mailAddr.Address, password)
            };
            using (var message = new MailMessage(mailAddr, sendTo)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
