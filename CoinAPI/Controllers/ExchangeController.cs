using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using CoinAPI.Models;
using CoinAPI.Models.Entities;
using CoinAPI.Nethereum.Services;
using CoinAPI.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoRepository2;
using Newtonsoft.Json;

namespace CoinAPI.Controllers
{
    [Authorize("Bearer")]
    [Authorize(policy: "ApiUser")]
    [Produces("application/json")]
    public class ExchangeController : Controller
    {
        private readonly ClaimsPrincipal _caller;
        private readonly INethereumService nethereumService;
        private readonly MongoRepository<UserInfo> mongoRepositoryUserInfo;
        private readonly MongoRepository<Order> mongoRepositoryOrder;

        public ExchangeController(IHttpContextAccessor httpContextAccessor, INethereumService nethereumService)
        {
            _caller = httpContextAccessor.HttpContext.User;
            this.nethereumService = nethereumService;
            this.mongoRepositoryUserInfo = new MongoRepository<UserInfo>("mongodb://localhost:27017/TokenDB");
            this.mongoRepositoryOrder = new MongoRepository<Order>("mongodb://localhost:27017/TokenDB");
        }



        [HttpPost]
        [Route("api/Exchange/Sell")]
        public IActionResult Sell([FromBody]SellOrderViewModel model)
        {
            var userClaims = _caller.Claims.Single(c => c.Type == "id");
            var user = mongoRepositoryUserInfo.Where(u => u.Username == userClaims.Value).FirstOrDefault();
            var address = this.nethereumService.GetPublicAddress(user.PrivateKey);
            var order = new Order()
            {
                TransacionId = Guid.NewGuid().ToString(),
                Amount = model.Amount,
                Price = model.Price,
                Seller = address
            };
            var tx = this.nethereumService.CreateOrder(user.PrivateKey, order.TransacionId, order.Amount, order.Price);
            mongoRepositoryOrder.Add(order);
            return new OkObjectResult(tx);
        }

        [HttpPost]
        [Route("api/Exchange/Buy")]
        public IActionResult Buy([FromBody]BuyOrderViewModel model)
        {
            var userClaims = _caller.Claims.Single(c => c.Type == "id");
            var user = mongoRepositoryUserInfo.Where(u => u.Username == userClaims.Value).FirstOrDefault();
            var tx = this.nethereumService.BuyOrder(user.PrivateKey, model.TransactionId);
            mongoRepositoryOrder.Delete(o => o.TransacionId == model.TransactionId);
            return new OkObjectResult(tx);
        }

        [HttpGet]
        [Route("api/Exchange/Orders")]
        public IActionResult GetOrders()
        {           
            return new OkObjectResult(mongoRepositoryOrder.ToList());
        }

    }
}