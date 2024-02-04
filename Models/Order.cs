
using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models;

public class Order
{
    public int Id { get; set; }
    [Required]
    public int CashierId { get; set; }
    public DateTime? PaidOnDate { get; set; }
    public List<OrderProduct> OrderProducts { get; set; }
    public Cashier Cashier { get; set; }
    // Computed property for Total
    public decimal Total
    {
        get
        {
            if (OrderProducts != null && OrderProducts.Any())
            {
                // Calculate the total by summing product prices times quantity
                return OrderProducts.Sum(op =>
                {
                    if(op != null && op.Product != null){
                        return op.Product.Price * op.Quantity;
                    }
                    return 0M;
                });
            
            }

            return 0M; // If no products in the order, total is zero
        }
    }

    internal object Include(Func<object, object> value)
    {
        throw new NotImplementedException();
    }
}
