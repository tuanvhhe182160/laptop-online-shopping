using WebAPI.Data;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class PhysicalProductRepository : GenericRepository<PhysicalProduct>, IPhysicalProductRepository
    {
        public PhysicalProductRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
