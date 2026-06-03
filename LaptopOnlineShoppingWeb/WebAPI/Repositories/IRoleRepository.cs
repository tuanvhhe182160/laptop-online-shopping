using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllRolesAsync();
    }
}
