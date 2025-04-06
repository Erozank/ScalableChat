
using ChatWithSignalR.Api.Models;

namespace ChatWithSignalR.Api.Repositories
{
    public interface IUserRepository
    {
        Task<bool> Exists(string email);
        Task<User?> GetByEmail(string email);
        Task Insert(User user);
    }
}