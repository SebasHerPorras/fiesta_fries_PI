using backend.Models;
using backend.Repositories;

namespace backend.Interfaces
{
    public interface IEmailVerificationMessageService
    {
        public void buildEmail(UserModel request);

        public void sendEmail(MailRepository mailRepository, MailModel mailTokenVerification, UserModel request);

    }
}
