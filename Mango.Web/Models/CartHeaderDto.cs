

using System.ComponentModel.DataAnnotations;

namespace Mango.Web.Models
{
    public class CartHeaderDto
    {
        
        public string Title { get; set; }
     //   [Key]
        public int CartHeaderId { get; set; }
        public string? UserId { get; set; }
        public string? CouponId { get; set; }
        public double Discount { get; set; }
        public double CartTotal { get; set; }
    }
}
