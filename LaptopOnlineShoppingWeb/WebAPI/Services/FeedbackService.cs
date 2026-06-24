using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WebAPI.Data;
using WebAPI.DTOs;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Services
{
    public class FeedbackService : IFeedbackService
    {
        private readonly IFeedbackRepository _feedbackRepo;
        private readonly ApplicationDbContext _context;

        public FeedbackService(IFeedbackRepository feedbackRepo, ApplicationDbContext context)
        {
            _feedbackRepo = feedbackRepo;
            _context = context;
        }

        public async Task<(bool Success, string Message)> CreateFeedbackAsync(CreateFeedbackDto dto, int customerId)
        {
            // Kiểm tra xem khách hàng đã mua sản phẩm này và đơn hàng đã hoàn thành chưa
            var hasPurchased = await _context.Orders
                .AnyAsync(o => o.CustomerId == customerId 
                               && (o.OrderStatus == "Completed" || o.OrderStatus == "Delivered")
                               && o.OrderDetails.Any(od => od.VariantId == dto.VariantId));

            if (!hasPurchased)
            {
                return (false, "Bạn chỉ có thể đánh giá sản phẩm sau khi đã mua và nhận hàng thành công.");
            }

            // Kiểm tra xem đã đánh giá chưa (tùy chọn, giả sử cho phép đánh giá nhiều lần hoặc 1 lần)
            // Trong đề bài không cấm đánh giá nhiều lần, nhưng thông thường mỗi người/variant 1 đánh giá
            // Tuy nhiên cứ làm theo đúng yêu cầu đề bài.
            
            var feedback = new Feedback
            {
                VariantId = dto.VariantId,
                CustomerId = customerId,
                Rating = dto.Rating,
                Comment = dto.Comment,
                CreatedAt = DateTime.Now,
                IsHidden = false
            };

            await _feedbackRepo.AddAsync(feedback);
            await _feedbackRepo.SaveAsync();

            return (true, "Đánh giá của bạn đã được ghi nhận thành công.");
        }

        public async Task<(bool Success, string Message)> HideFeedbackAsync(int feedbackId)
        {
            var feedback = await _feedbackRepo.GetByIdAsync(feedbackId);
            if (feedback == null)
            {
                return (false, "Không tìm thấy đánh giá.");
            }

            // Cập nhật cờ IsHidden thay vì xóa cứng
            feedback.IsHidden = true;
            _feedbackRepo.Update(feedback);
            await _feedbackRepo.SaveAsync();

            return (true, "Đã ẩn đánh giá thành công (Xóa mềm).");
        }
    }
}
