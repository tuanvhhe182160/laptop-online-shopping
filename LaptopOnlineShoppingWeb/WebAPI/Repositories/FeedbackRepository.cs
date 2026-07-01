using WebAPI.Data;
using WebAPI.Entities;

namespace WebAPI.Repositories
{
    public class FeedbackRepository : GenericRepository<Feedback>, IFeedbackRepository
    {
        public FeedbackRepository(ApplicationDbContext context) : base(context)
        {
        }
    }
}
