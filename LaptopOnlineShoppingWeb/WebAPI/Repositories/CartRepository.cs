using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
   public class CartRepository : GenericRepository<Cart>, ICartRepository
   {
       private readonly ApplicationDbContext _context;

       public CartRepository(ApplicationDbContext context) : base(context)
       {
           _context = context;
       }

        public async Task<Cart?> GetCartByCustomerIdAsync(int customerId)
        {
            return await _context.Carts
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.ProductVariant)
                .FirstOrDefaultAsync(c => c.CustomerId == customerId);
        }
    }
}
