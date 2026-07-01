using System.Threading.Tasks;
using WebAPI.DTOs;

namespace WebAPI.Services
{
    public interface IFeedbackService
    {
        Task<(bool Success, string Message)> CreateFeedbackAsync(CreateFeedbackDto dto, int customerId);
        Task<(bool Success, string Message)> HideFeedbackAsync(int feedbackId);
        Task<(bool Success, string Message)> UpdateFeedbackAsync(int feedbackId, int customerId, int rating, string comment);
        Task<(bool Success, string Message)> DeleteFeedbackAsync(int feedbackId, int customerId);
    }
}
