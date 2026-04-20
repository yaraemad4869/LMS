using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LearningManagementSystem.Data.Enum;
using LearningManagementSystem.Data.Enum;
using PayPalCheckoutSdk.Orders;

namespace LearningManagementSystem.Models
{
    public class Order : PayPalCheckoutSdk.Orders.OrderRequest
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int UserId { get; set; }
        public virtual List<Course> Courses { get; set; }
        public decimal TotalPrice { get; set; }
        public string? Description { get; set; }
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Pending;
        public OrderStatus OrderStatus { get; set; } = OrderStatus.Cart;
        public void SetTotalPrice()
        {
            TotalPrice= Courses.Sum(c => c.Price);
        }
    }
}
