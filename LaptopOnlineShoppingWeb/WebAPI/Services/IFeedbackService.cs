using System.Threading.Tasks;
using WebAPI.DTOs;

namespace WebAPI.Services
{
    public interface IFeedbackService
    {
        Task<(bool Success, string Message)> CreateFeedbackAsync(CreateFeedbackDto dto, int customerId);
        Task<(bool Success, string Message)> HideFeedbackAsync(int feedbackId);
    }
}
