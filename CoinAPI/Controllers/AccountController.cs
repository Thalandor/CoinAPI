using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AspNetCore.Identity.MongoDbCore.Infrastructure;
using AutoMapper;
using CoinAPI.Helpers;
using CoinAPI.Models;
using CoinAPI.Models.Entities;
using CoinAPI.Nethereum.Services;
using CoinAPI.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoRepository2;

namespace CoinAPI.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly IMapper mapper;
        private readonly INethereumService nethereumService;
        private readonly MongoDbSettings mongoDbSettings;
        private readonly MongoRepository<UserInfo> mongoRepository;

        public AccountController(UserManager<ApplicationUser> userManager, IMapper mapper, INethereumService nethereumService, MongoDbSettings mongoDbSettings)
        {
            this.userManager = userManager;
            this.mapper = mapper;
            this.nethereumService = nethereumService;
            this.mongoDbSettings = mongoDbSettings;
            this.mongoRepository = new MongoRepository<UserInfo>("mongodb://localhost:27017/TokenDB");
        }
        
        [HttpPost]
        public async Task<IActionResult> Post([FromBody]RegistrationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdentity = mapper.Map<ApplicationUser>(model);

            var result = await userManager.CreateAsync(userIdentity, model.Password);

            if (!result.Succeeded) return new BadRequestObjectResult(Errors.AddErrorsToModelState(result, ModelState));

            var privateKey = this.nethereumService.GenerateNewPrivateKey();            
            mongoRepository.Add(new UserInfo()
            {
                Username = userIdentity.UserName,
                PrivateKey = privateKey
            });

            return new OkObjectResult("Account created");
        }
    }
}
