using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Data;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Mango.Services.OrderAPI.Utility;
using Mango.Services.OrderAPI.Models;
using Stripe;
using Stripe.Checkout;
namespace Mango.Services.OrderAPI.Controllers
{
    [Route("api/order")]
    [ApiController]
    public class OrderAPIController : ControllerBase
    {

        private ResponseDto _responseDto;
        private IMapper _mapper;
        private readonly AppDbContext _db;
        private IProductService _productService;
      


        public OrderAPIController(AppDbContext db,
            IMapper mapper, IProductService productService)
        {
            _db = db;
            _productService = productService;
            this._responseDto = new ResponseDto();
            _mapper = mapper;
        }
       // [Authorize]
        [HttpPost("CreateOrder")]
        public async Task<ResponseDto> CreateOrder([FromBody] CartDto cart)
        {
            try
            {
                OrderHeaderDto orderHeaderDto = _mapper.Map<OrderHeaderDto>(cart.CartHeader);
                orderHeaderDto.OrderTime=DateTime.Now;
                orderHeaderDto.Status=SD.Status_Pending;
                orderHeaderDto.OrderDetails = _mapper.Map<IEnumerable<OrderdetailsDto>>(cart.CartDetails);

                OrderHeader orderCreated= _db.OrderHeaders.Add(_mapper.Map<OrderHeader>(orderHeaderDto)).Entity;
                await _db.SaveChangesAsync();

                orderHeaderDto.OrderHeaderId=orderCreated.OrderHeaderId;
                _responseDto.Result=orderHeaderDto;
            }
            catch (Exception ex)
            {

                _responseDto.IsSuccess=false;
                _responseDto.Message=ex.Message;
            }
            return _responseDto;
        }

        [Authorize]
        [HttpPost("CreateStripeSession")]
        public async Task<ResponseDto> CreateStripeSession([FromBody] StripeRequestDto stripeRequestDto)
        {
            try
            {

                var options = new Stripe.Checkout.SessionCreateOptions
                {
                    SuccessUrl = stripeRequestDto.ApprovedUrl,
                    CancelUrl = stripeRequestDto.CancelUrl,
                    LineItems = new List<Stripe.Checkout.SessionLineItemOptions>(),
                
                   
               
                    Mode = "payment",
                };

                var DiscountObj = new List<SessionDiscountOptions>()
                {
                    new SessionDiscountOptions()
                    {
                        Coupon=stripeRequestDto.OrderHeader.CouponId
                    }
                };

                foreach (var item in stripeRequestDto.OrderHeader.OrderDetails)
                {
                    var sessionLineItem = new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            UnitAmount = (long)(item.Price * 100),
                            Currency = "usd",
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = item.Product.Name
                            }
                        },
                        Quantity = item.Count
                    };
                    options.LineItems.Add(sessionLineItem);
                }
                if (stripeRequestDto.OrderHeader.Discount > 0)
                {
                    options.Discounts=DiscountObj;
                }
                {

                }
                var service = new Stripe.Checkout.SessionService();
               Session session= service.Create(options);
                stripeRequestDto.StripeSessionUrl = session.Url;
                OrderHeader orderHeader=_db.OrderHeaders.First(u=>u.OrderHeaderId==stripeRequestDto.OrderHeader.OrderHeaderId);
                orderHeader.StripeSessionId=session.Id;
                _db.SaveChanges();
                _responseDto.Result = stripeRequestDto;
            }
            catch (Exception ex)
            {

                _responseDto.Message = ex.Message;
                _responseDto.IsSuccess=false;
            }
            return _responseDto;
        }

        [Authorize]
        [HttpPost("ValidateStripeSession")]
        public async Task<ResponseDto> ValidateStripeSession([FromBody] int  orderHeaderId)
        {
            try
            {
                OrderHeader orderHeader = _db.OrderHeaders.First(u => u.OrderHeaderId == orderHeaderId);

                var service = new Stripe.Checkout.SessionService();
                Session session = service.Get(orderHeader.StripeSessionId);

                var paymentIntentService=new PaymentIntentService();
                PaymentIntent paymentIntent = paymentIntentService.Get(session.PaymentIntentId);

                if (paymentIntent.Status=="Succeeded")
                {
                    // then payment is successful :)
                    orderHeader.PaymentIntentId= paymentIntent.Id;

                    orderHeader.Status = SD.Status_Approved;
                    _db.SaveChanges();
                    _responseDto.Result = _mapper.Map<OrderHeaderDto>(orderHeader);
                }

              
            }
            catch (Exception ex)
            {

                _responseDto.Message = ex.Message;
                _responseDto.IsSuccess = false;
            }
            return _responseDto;
        }

    }
}
