using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public interface IUserRepository
    {
        Task<IEnumerable<User>> GetAllUsersWithRolesAsync();
        Task<User?> GetByIdAsync(int id);
        Task<bool> UsernameExistsAsync(string username);
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task SaveAsync();
        Task ToggleStatusAsync(int id);
        Task<User?> GetUserDetailsAsync(int id);
    }
}
