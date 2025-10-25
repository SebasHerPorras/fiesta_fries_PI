using backend.Models;

namespace backend.Interfaces
{
    public interface IEmailVerificationMessageService
    {
       public void sendEmail(UserModel request);
    }
}
