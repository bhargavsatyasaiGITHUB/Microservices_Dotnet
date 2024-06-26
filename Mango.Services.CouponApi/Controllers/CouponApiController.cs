﻿using AutoMapper;
using Mango.Services.CouponApi.Data;
using Mango.Services.CouponApi.Models;
using Mango.Services.CouponApi.Models.Dto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;


namespace Mango.Services.CouponApi.Controllers
{
    [Route("api/coupon")]
    [ApiController]
   // [Authorize]
    public class CouponApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private ResponseDto _responseDto;
        private IMapper _mapper;

        public CouponApiController(AppDbContext db,IMapper mapper)
        {
            _db = db;
            _responseDto = new ResponseDto();
            _mapper = mapper;
        }

        [HttpGet]
        public ResponseDto Get()
        {
            try
            {
                IEnumerable<Coupon> objList=_db.coupons.ToList();
                _responseDto.Result = _mapper.Map<IEnumerable<CouponDto>>(objList);
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess=false;
                _responseDto.Message=ex.Message;
                
            }
            return _responseDto;
        }

        [HttpGet]
        [Route("{id:int}")]
        public ResponseDto Get(int id)
        {
            try
            {
                Coupon objList = _db.coupons.First(u=>u.CouponId==id);
                _responseDto.Result= _mapper.Map<CouponDto>(objList);
               
            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;

            }
            return _responseDto;
        }


        [HttpGet]
        [Route("GetByCode/{code}")]
        public ResponseDto GetByCode(string code)
        {
            try
            {
                Coupon objList = _db.coupons.First(u => u.CouponCode.ToLower() == code.ToLower());
               
                _responseDto.Result = _mapper.Map<CouponDto>(objList);

            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;

            }
            return _responseDto;
        }


        [HttpPost]
        [Authorize(Roles ="ADMIN")]
        public ResponseDto Post([FromBody] CouponDto couponDto )
        {
            try
            {
                Coupon obj = _mapper.Map<Coupon>(couponDto);
                _db.coupons.Add(obj);
                _db.SaveChanges();

               

                var options = new Stripe.CouponCreateOptions
                {
                    Duration = "repeating",
                    AmountOff =(long)(couponDto.DiscountAmount*100),
                    DurationInMonths = 3,

                    Name=couponDto.CouponCode,
                    Currency="usd",
                    Id=couponDto.CouponCode
                   
                };
                var service = new Stripe.CouponService();
                service.Create(options);



                _responseDto.Result = _mapper.Map<CouponDto>(obj);

            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;

            }
            return _responseDto;
        }

        [HttpPut]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Put([FromBody] CouponDto couponDto)
        {
            try
            {
                Coupon obj = _mapper.Map<Coupon>(couponDto);
                _db.coupons.Update(obj);
                _db.SaveChanges();
                _responseDto.Result = _mapper.Map<CouponDto>(obj);

            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;

            }
            return _responseDto;
        }

        [HttpDelete]
        [Route("{id:int}")]
        [Authorize(Roles = "ADMIN")]
        public ResponseDto Delete(int id)
        {
            try
            {
                Coupon obj = _db.coupons.First(u => u.CouponId == id);
                _db.coupons.Remove(obj);
                _db.SaveChanges();

                var service = new Stripe.CouponService();
                service.Delete(obj.CouponCode);
              

            }
            catch (Exception ex)
            {
                _responseDto.IsSuccess = false;
                _responseDto.Message = ex.Message;

            }
            return _responseDto;
        }

    }
}
