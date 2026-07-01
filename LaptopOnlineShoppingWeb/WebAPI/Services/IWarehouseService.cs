using System.Threading.Tasks;
using WebAPI.DTOs;

namespace WebAPI.Services
{
    public interface IWarehouseService
    {
        Task<(bool Success, string Message)> ImportProductsAsync(ImportPhysicalProductDto dto, int branchId);
    }
}
