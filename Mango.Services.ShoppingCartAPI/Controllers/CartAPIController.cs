using AutoMapper;
using Mango.Services.ShoppingCartAPI.Data;
using Mango.Services.ShoppingCartAPI.Models;
using Mango.Services.ShoppingCartAPI.Models.Dto;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using System.Reflection.PortableExecutable;

namespace Mango.Services.ShoppingCartAPI.Controllers
{
    [Route("api/cart")]
    [ApiController]
    public class CartAPIController : Controller
    {
       private ResponseDto _responseDto;
        private IMapper _mapper;
        private readonly AppDbContext _db;
        private IProductService _productService;
        private ICouponService _couponService;

        public CartAPIController(AppDbContext db,
            IMapper mapper,IProductService productService , ICouponService couponService)
        {
                _db = db;
            _productService = productService;
            _couponService = couponService;
            this._responseDto = new ResponseDto();
            _mapper = mapper;
        }

        [HttpGet("GetCart/{userId}")]
        public async Task<ResponseDto>GetCart(string userId)
        {
            try
            {
                CartDto cart = new()
                {
                    CartHeader = _mapper.Map<CartHeaderDto>(_db.CartHeaders.First(u => u.UserId == userId))
                };
                cart.CartDetails=_mapper.Map<IEnumerable<CartDetailsDto>>(_db.CartDetails
                    .Where(u=>u.CartHeaderId==cart.CartHeader.CartHeaderId));

                IEnumerable<ProductDto> productDtos = await _productService.GetProducts();

                foreach (var item in cart.CartDetails)
                {
                    item.Product = productDtos.FirstOrDefault(u => u.productId == item.ProductId);
                    cart.CartHeader.CartTotal += (item.Count * item.Product.Price);
                }

                //apply coupon if any
                if (!string.IsNullOrEmpty(cart.CartHeader.CouponId))
                {
                    CouponDto coupon = await _couponService.GetCoupon(cart.CartHeader.CouponId);
                    if (coupon!=null &&cart.CartHeader.CartTotal>coupon.MinAmount)
                    {
                        cart.CartHeader.CartTotal -= coupon.DiscountAmount;
                        cart.CartHeader.Discount=coupon.DiscountAmount;
                    }
                }

                _responseDto.Result=cart;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;
                
            }
           
            return _responseDto;
        }


        [HttpPost("ApplyCoupon")]
        public async Task<object> ApplyCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartFromDb=await _db.CartHeaders.FirstAsync(u=>u.UserId==cartDto.CartHeader.UserId);
                
                cartFromDb.CouponId=cartDto.CartHeader.CouponId;
                _db.CartHeaders.Update(cartFromDb);
                await _db.SaveChangesAsync();
                _responseDto.Result = true;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;

            }
            return _responseDto;
        }

        [HttpPost("RemoveCoupon")]
        public async Task<object> RemoveCoupon([FromBody] CartDto cartDto)
        {
            try
            {
                var cartFromDb = await _db.CartHeaders.FirstAsync(u => u.UserId == cartDto.CartHeader.UserId);
                cartFromDb.CouponId = "";
                _db.CartHeaders.Update(cartFromDb);
                await _db.SaveChangesAsync();
                _responseDto.Result = true;
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;

            }
            return _responseDto;
        }


        [HttpPost("CartUpsert")]
        public async Task<ResponseDto> CartUpsert(CartDto cartDto)
        {
            try
            {
                var cartHeaderFromDb=await _db.CartHeaders.AsNoTracking().FirstOrDefaultAsync(u=>u.UserId==cartDto.CartHeader.UserId);
                if (cartHeaderFromDb==null) 
                { 
                    // create header and details
                    CartHeader cartHeader = _mapper.Map<CartHeader>(cartDto.CartHeader);
                    _db.CartHeaders.Add(cartHeader);
                    await _db.SaveChangesAsync();
                    cartDto.CartDetails.First().CartHeaderId=cartHeader.cartHeaderId;
                    _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                    await _db.SaveChangesAsync();
                }
                else
                {
                    var cartDetailsFromDb=await _db.CartDetails.AsNoTracking().FirstOrDefaultAsync(
                        u=>u.ProductId==cartDto.CartDetails.First().ProductId &&
                        u.CartHeaderId==cartHeaderFromDb.cartHeaderId);
                    if (cartHeaderFromDb==null)
                    {
                        cartDto.CartDetails.First().CartHeaderId = cartHeaderFromDb.cartHeaderId;
                        _db.CartDetails.Add(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();
                    }
                    else
                    {
                        cartDto.CartDetails.First().Count += cartDetailsFromDb.count;
                        cartDto.CartDetails.First().CartHeaderId = cartDetailsFromDb.CartHeaderId;
                        cartDto.CartDetails.First().CartDetailsId = cartDetailsFromDb.CartDetailsId;
                        _db.CartDetails.Update(_mapper.Map<CartDetails>(cartDto.CartDetails.First()));
                        await _db.SaveChangesAsync();
                    }
                }
                _responseDto.Result=cartDto;
            }
            catch (Exception ex)
            {
                _responseDto.Message = ex.Message.ToString();
                _responseDto.IsSuccess = false;
            }
            return _responseDto;
        }

        [HttpPost("RemoveCart")]
        public async Task<ResponseDto> RemoveCart([FromBody]int cartDetailsId)
        {
            try
            {
                CartDetails cartDetails=_db.CartDetails.First(u=>u.CartDetailsId==cartDetailsId);
                int totalCountOfCartItem=_db.CartDetails.Where(u=>u.CartHeaderId==cartDetailsId).Count();

               _db.CartDetails.Remove(cartDetails);
                if (totalCountOfCartItem==1)
                {
                    var cartHeaderToRemove=await _db.CartHeaders.FirstOrDefaultAsync(u=>u.cartHeaderId==cartDetails.CartHeaderId);
                    _db.CartHeaders.Remove(cartHeaderToRemove);
                }
               
               await _db.SaveChangesAsync();
                _responseDto.Result = true;
            }
            catch (Exception ex)
            {
                _responseDto.Message = ex.Message.ToString();
                _responseDto.IsSuccess = false;
            }
            return _responseDto;
        }
    }
}
