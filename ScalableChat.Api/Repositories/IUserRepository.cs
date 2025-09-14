
using ScalableChat.Api.Entities;
using ScalableChat.Api.Models;

namespace ScalableChat.Api.Repositories
{
    public interface IUserRepository
    {
        Task<bool> Exists(string nickname);
        Task<UserEntity?> GetByNickname(string nickname);
        Task<UserPreview?> GetUserPreviewByUserId(Guid userId);
        Task<Guid?> GetUserIdByNickname(string nickname);
        Task Insert(UserEntity user);
    }
}