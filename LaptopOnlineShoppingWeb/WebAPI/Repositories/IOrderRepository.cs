using System.Collections.Generic;
using System.Threading.Tasks;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public interface IOrderRepository : IGenericRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByCustomerIdAsync(int customerId);
        Task<Order?> GetOrderWithDetailsAsync(int orderId);
    }
}
