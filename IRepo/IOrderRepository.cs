using LearningManagementSystem.Data;
using LearningManagementSystem.Models;

namespace LearningManagementSystem.IRepo
{
    public interface IOrderRepository
    {
        List<Order> GetOrders(User user);
        Task<Order?> AddToCart(Course course, User user);
        Order GetCart(User user);
    }
}
