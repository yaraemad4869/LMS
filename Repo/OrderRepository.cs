using LearningManagementSystem.Data;
using LearningManagementSystem.Data.Enum;
using LearningManagementSystem.IRepo;
using LearningManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningManagementSystem.Repo
{
    public class OrderRepository : Repository<Order>, IOrderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderRepository(ApplicationDbContext context) : base(context) 
        {
        }
        public List<Order> GetOrders(User user)
        {
            return user.Orders.ToList();
        }
        public Order GetCart(User user)
        {
            return user.Orders.FirstOrDefault(o => o.OrderStatus == OrderStatus.Cart);
        }

        public async Task<Order?> AddToCart(Course course, User user)
        {
            Order? cart = user.Orders.FirstOrDefault(o => o.OrderStatus == OrderStatus.Cart);
            if (cart == null)
            {
                cart = new Order();
                cart.UserId = user.Id;
            }
            if (course != null)
            {
                cart.TotalPrice += course.Price;
                cart.Courses.Add(course);
                await _context.SaveChangesAsync();
            }
            return cart;
        }
    }
}
