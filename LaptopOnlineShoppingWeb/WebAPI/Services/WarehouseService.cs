using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IPhysicalProductRepository _physicalProductRepo;
        private readonly IProductVariantRepository _variantRepo;
        private readonly ApplicationDbContext _context;

        public WarehouseService(
            IPhysicalProductRepository physicalProductRepo, 
            IProductVariantRepository variantRepo,
            ApplicationDbContext context)
        {
            _physicalProductRepo = physicalProductRepo;
            _variantRepo = variantRepo;
            _context = context;
        }

        public async Task<(bool Success, string Message)> ImportProductsAsync(ImportPhysicalProductDto dto, int branchId)
        {
            // 1. Kiểm tra cấu hình có tồn tại không
            var variant = await _variantRepo.GetByIdAsync(dto.VariantId);
            if (variant == null)
            {
                return (false, "Biến thể sản phẩm (Variant) không tồn tại.");
            }

            // 2. Làm sạch danh sách Serial
            var serials = dto.SerialNumbers
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Select(s => s.Trim())
                .Distinct()
                .ToList();

            if (!serials.Any())
            {
                return (false, "Danh sách Serial Numbers hợp lệ trống.");
            }

            // Bọc trong transaction để an toàn (dù SaveChanges() mặc định đã là transaction)
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 3. Kiểm tra trùng mã Serial trong hệ thống
                var existingSerials = await _physicalProductRepo.GetQueryable()
                    .Where(p => serials.Contains(p.SerialNumber))
                    .Select(p => p.SerialNumber)
                    .ToListAsync();

                if (existingSerials.Any())
                {
                    var joinedSerials = string.Join(", ", existingSerials);
                    return (false, $"Các Serial Numbers sau đã tồn tại trong hệ thống: {joinedSerials}");
                }

                // 4. Tạo các bản ghi mới
                var newProducts = new List<PhysicalProduct>();
                foreach (var serial in serials)
                {
                    newProducts.Add(new PhysicalProduct
                    {
                        VariantId = dto.VariantId,
                        BranchId = branchId,
                        SerialNumber = serial,
                        Status = "InStock"
                    });
                }

                await _context.PhysicalProducts.AddRangeAsync(newProducts);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
                return (true, $"Nhập thành công {newProducts.Count} thiết bị vào kho.");
            }
            catch (System.Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Có lỗi xảy ra trong quá trình nhập kho: " + ex.Message);
            }
        }
    }
}
