using System.ComponentModel.DataAnnotations;

namespace CornerStore.Models;

public class OrderProduct
{
    [Required]
    public int Id {get; set;}
    public int ProductId {get; set;}
    [Required]
    public int OrderId {get; set;}
    public int Quantity {get; set;}
    public Product Product { get; set; }
    [Required]
    public Order Order { get; set; }
}