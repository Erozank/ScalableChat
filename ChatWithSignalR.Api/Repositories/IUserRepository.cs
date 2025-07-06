
using ChatWithSignalR.Api.Entities;
using ChatWithSignalR.Api.Models;

namespace ChatWithSignalR.Api.Repositories
{
    public interface IUserRepository
    {
        Task<bool> Exists(string email);
        Task<UserEntity?> GetByEmail(string email);
        Task<UserPreview?> GetUserPreviewByUserId(Guid userId);
        Task<Guid?> GetUserIdByNickname(string nickname);
        Task Insert(UserEntity user);
    }
}