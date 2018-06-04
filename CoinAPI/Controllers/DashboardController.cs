
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
    [Route("api/[controller]/[action]")]
    public class DashboardController : Controller
    {
        private readonly MongoRepository<UserInfo> mongoRepository;


        private readonly ClaimsPrincipal _caller;
        private readonly INethereumService nethereumService;

        public DashboardController(IHttpContextAccessor httpContextAccessor, INethereumService nethereumService, MongoDbSettings mongoDbSettings)
        {
            _caller = httpContextAccessor.HttpContext.User;
            this.mongoRepository = new MongoRepository<UserInfo>($"{mongoDbSettings.ConnectionString}/{mongoDbSettings.DatabaseName}" );
            this.nethereumService = nethereumService;
        }

        // GET api/dashboard/home
        [HttpGet]        
        public  IActionResult Home()
        {
            // retrieve the user info
            
            var userId = _caller.Claims.Single(c => c.Type == "id");
            var user = mongoRepository.Where(u => u.Username == _caller.Identity.Name).FirstOrDefault();
            var publicAddress = nethereumService.GetPublicAddress(user.PrivateKey);

            return new OkObjectResult(new
            {
                user.Id,
                publicAddress
            });
        }
    }
}
