﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using CoinAPI.Models;
using CoinAPI.Models.Entities;
using CoinAPI.Nethereum.Services;
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
    public class DividendController : Controller
    {
        private readonly ClaimsPrincipal _caller;
        private readonly INethereumService nethereumService;
        private readonly MongoRepository<UserInfo> mongoRepositoryUserInfo;

        public DividendController(IHttpContextAccessor httpContextAccessor, INethereumService nethereumService, MongoDbSettings mongoDbSettings)
        {
            _caller = httpContextAccessor.HttpContext.User;
            this.nethereumService = nethereumService;
            this.mongoRepositoryUserInfo = new MongoRepository<UserInfo>($"{mongoDbSettings.ConnectionString}/{mongoDbSettings.DatabaseName}" );
        }

        [HttpPost]
        [Route("api/Dividend/claimDividends")]
        public async Task<IActionResult> ClaimDividends()
        {
            var userClaims = _caller.Claims.Single(c => c.Type == "id");
            var user = mongoRepositoryUserInfo.Where(u => u.Username == userClaims.Value).FirstOrDefault();
            var tx = await this.nethereumService.ClaimDividend(user.PrivateKey);
            return new OkObjectResult(tx);
        }

        [HttpGet]
        [Route("api/Dividend/pendingDividends")]
        public async Task<IActionResult> PendingDividends()
        {
            var userClaims = _caller.Claims.Single(c => c.Type == "id");
            var user = mongoRepositoryUserInfo.Where(u => u.Username == userClaims.Value).FirstOrDefault();
            var pendingDividends = await this.nethereumService.GetPendingDividends(user.PrivateKey);
            return new OkObjectResult(pendingDividends);
        }
    }
}