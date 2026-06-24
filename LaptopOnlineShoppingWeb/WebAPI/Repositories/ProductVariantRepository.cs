using WebAPI.Data;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class ProductVariantRepository : GenericRepository<ProductVariant>, IProductVariantRepository
    {
        public ProductVariantRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
