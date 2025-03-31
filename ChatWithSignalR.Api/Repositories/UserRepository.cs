using ChatWithSignalR.UsersDb;
using Microsoft.EntityFrameworkCore;

namespace ChatWithSignalR.Api.Repositories
{
    public class UserRepository(UsersDbContext usersDbContext) : IUserRepository
    {
        public async Task<bool> Exists(string email)
        {
            return await usersDbContext.Users.AnyAsync(u => u.Email == email);
        }

        public async Task Insert(User user)
        {
            await usersDbContext.Users.AddAsync(user);
            await usersDbContext.SaveChangesAsync();
        }

        public async Task<User?> GetByEmail(string email)
        {
            return await usersDbContext.Users.SingleOrDefaultAsync(u => u.Email == email);
        }
    }
}
