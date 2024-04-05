using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mango.Services.ShoppingCartAPI.Models
{
    public class CartHeader
    {
        [Key]
        public int cartHeaderId { get; set; }
        public string? UserId {  get; set; }
        public string? CouponId {  get; set; }
        [NotMapped]
        public double Discount {  get; set; }
        [NotMapped]
        public double CartTotal {  get; set; }
    }
}
