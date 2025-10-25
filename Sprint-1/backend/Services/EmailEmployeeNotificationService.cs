using backend.Interfaces;
using backend.Models;
using backend.Repositories;
namespace backend.Services
{
    public class EmailEmployeeNotificationService : IEmailVerificationMessageService
    {
        private readonly EmailEmployeeNotificationRepository _repository;

        public EmailEmployeeNotificationService()
        {
            this._repository = new EmailEmployeeNotificationRepository();
        }
        public void sendEmail(UserModel request)
        {
            this._repository.buildEmail(request);
        }
    }
}
