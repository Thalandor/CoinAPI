using System.Threading.Tasks;

namespace CoinAPI.Nethereum.Services
{
    public interface INethereumService
    {
        string GenerateNewPrivateKey();

        string GetPublicAddress(string privateKey);

        Task<decimal> GetAccountETHBalance(string accountAddress);

        Task<decimal> GetAccountTokenBalance(string accountAddress);

        Task<bool> InvestICO(string privateKey, decimal amount);

        Task<string> CreateOrder(string privateKey, string transactionId, decimal amount, decimal price);

        Task<string> BuyOrder(string privateKey, string transactionId);

        Task<string> ClaimDividend(string privateKey);
    }
}