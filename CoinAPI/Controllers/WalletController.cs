using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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

namespace CoinAPI.Controllers
{
    [Authorize("Bearer")]
    [Authorize(policy: "ApiUser")]
    [Produces("application/json")]
    public class WalletController : Controller
    {
        private readonly ClaimsPrincipal _caller;
        private readonly INethereumService nethereumService;
        private readonly MongoRepository<UserInfo> mongoRepository;

        public WalletController(IHttpContextAccessor httpContextAccessor, INethereumService nethereumService)
        {
            _caller = httpContextAccessor.HttpContext.User;
            this.nethereumService = nethereumService;
            this.mongoRepository = new MongoRepository<UserInfo>("mongodb://localhost:27017/TokenDB");
        }

        [HttpGet]
        [Route("api/Wallet/Info")]
        public async Task<IActionResult> GetAccountInfo()
        {
            var userClaims = _caller.Claims.Single(c => c.Type == "id");
            var user = mongoRepository.Where(u => u.Username == userClaims.Value).FirstOrDefault();
            var publicAddress = nethereumService.GetPublicAddress(user.PrivateKey);
            var ethBalance = await nethereumService.GetAccountETHBalance(publicAddress);
            var tokenBalance = await nethereumService.GetAccountTokenBalance(publicAddress);

            return new OkObjectResult(new WalletInfo
            {
                Address = publicAddress,
                ETHBalance = ethBalance,
                TokenBalance = tokenBalance
            });
        }

        [HttpPost]
        [Route("api/Wallet/Invest")]
        public async Task<IActionResult> InvestICO([FromBody]InvestICOViewModel model)
        {
            var userClaims = _caller.Claims.Single(c => c.Type == "id");
            var user = mongoRepository.Where(u => u.Username == userClaims.Value).FirstOrDefault();
            var publicAddress = nethereumService.GetPublicAddress(user.PrivateKey);
            var ethBalance = await nethereumService.GetAccountETHBalance(publicAddress);
            if (ethBalance >= model.Amount)
            {
                var result = await nethereumService.InvestICO(user.PrivateKey, model.Amount);
                if (result)
                {
                    return new OkObjectResult(true);
                }
                else
                {
                    var message = "Transaction error";
                    //HttpError err = new HttpError(message);
                    return new BadRequestObjectResult(message);
                }
            }
            else
            {
                var message = "Not enough funds";
                //HttpError err = new HttpError(message);
                return new BadRequestObjectResult(message);
            }
        }


        [HttpGet]
        [Route("api/Wallet/TokenPrice")]
        public async Task<IActionResult> GetTokenPrice()
        {
            var tokenPrice = await this.nethereumService.GetTokenPrice();
            return new OkObjectResult(tokenPrice);
        }
    }
}