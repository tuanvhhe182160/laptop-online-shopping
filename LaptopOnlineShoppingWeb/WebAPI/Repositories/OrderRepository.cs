//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Microsoft.EntityFrameworkCore;
//using WebAPI.Data;
//using WebAPI.Entities;

//namespace WebAPI.Repositories
//{
//    public class OrderRepository : GenericRepository<Order>, IOrderRepository
//    {
//        private readonly ApplicationDbContext _context;

//        public OrderRepository(ApplicationDbContext context) : base(context)
//        {
//            _context = context;
//        }

//        public async Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId)
//        {
//            return await _context.Orders
//                .Where(o => o.CustomerId == customerId)
//                .OrderByDescending(o => o.OrderDate)
//                .ToListAsync();
//        }

//        public async Task<Order?> GetOrderWithDetailsAsync(int orderId)
//        {
//            return await _context.Orders
//                .Include(o => o.Customer)
//                .Include(o => o.OrderDetails)
//                .ThenInclude(od => od.Laptop)
//                .FirstOrDefaultAsync(o => o.OrderId == orderId);
//        }
//    }
//}
