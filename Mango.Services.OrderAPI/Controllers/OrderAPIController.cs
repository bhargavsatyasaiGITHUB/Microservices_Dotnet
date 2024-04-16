using AutoMapper;

using Microsoft.AspNetCore.Mvc;
using Mango.Services.OrderAPI.Models.Dto;
using Mango.Services.OrderAPI.Data;
using Mango.Services.ShoppingCartAPI.Service.IService;
using Microsoft.AspNetCore.Authorization;
using Mango.Services.OrderAPI.Utility;
using Mango.Services.OrderAPI.Models;
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
        [Authorize]
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
    }
}
