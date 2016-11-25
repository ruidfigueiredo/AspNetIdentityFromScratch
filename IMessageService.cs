using System.Threading.Tasks;

namespace AspNetIdentityFromScratch
{
    public interface IMessageService
    {
        Task Send(string email, string subject, string message);
    }
}
