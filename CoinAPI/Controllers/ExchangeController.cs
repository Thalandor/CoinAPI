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

        public ExchangeController(IHttpContextAccessor httpContextAccessor, INethereumService nethereumService, MongoDbSettings mongoDbSettings)
        {
            _caller = httpContextAccessor.HttpContext.User;
            this.nethereumService = nethereumService;
            this.mongoRepositoryUserInfo = new MongoRepository<UserInfo>($"{mongoDbSettings.ConnectionString}/{mongoDbSettings.DatabaseName}" );
            this.mongoRepositoryOrder = new MongoRepository<Order>($"{mongoDbSettings.ConnectionString}/{mongoDbSettings.DatabaseName}" );
        }



        [HttpPost]
        [Route("api/Exchange/Sell")]
        public async Task<IActionResult> Sell([FromBody]SellOrderViewModel model)
        {
            var userClaims = _caller.Claims.Single(c => c.Type == "id");
            var user = mongoRepositoryUserInfo.Where(u => u.Username == userClaims.Value).FirstOrDefault();
            var address = this.nethereumService.GetPublicAddress(user.PrivateKey);
            var order = new Order()
            {
                TransactionId = Guid.NewGuid().ToString(),
                Amount = model.Amount,
                Price = model.Price,
                Seller = address
            };
            var tx = await this.nethereumService.CreateOrder(user.PrivateKey, order.TransactionId, order.Amount, order.Price);
            order.TransactionBlockchain = tx;
            mongoRepositoryOrder.Add(order);
            return new OkObjectResult(tx);
        }

        [HttpPost]
        [Route("api/Exchange/Buy")]
        public async Task<IActionResult> Buy([FromBody]BuyOrderViewModel model)
        {
            var userClaims = _caller.Claims.Single(c => c.Type == "id");
            var user = mongoRepositoryUserInfo.Where(u => u.Username == userClaims.Value).FirstOrDefault();
            var order = mongoRepositoryOrder.Where(o => o.TransactionId == model.TransactionId).FirstOrDefault();
            var tx = await this.nethereumService.BuyOrder(user.PrivateKey, model.TransactionId, order.Price);
            mongoRepositoryOrder.Delete(o => o.TransactionId == model.TransactionId);
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