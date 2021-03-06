﻿using System.Threading.Tasks;

namespace CoinAPI.Nethereum.Services
{
    public interface INethereumService
    {
        string GenerateNewPrivateKey();

        string GetPublicAddress(string privateKey);

        Task<decimal> GetAccountETHBalance(string accountAddress);

        Task<decimal> GetAccountTokenBalance(string accountAddress);

        Task<string> InvestICO(string privateKey, decimal amount);

        Task<string> CreateOrder(string privateKey, string transactionId, decimal amount, decimal price);

        Task<string> BuyOrder(string privateKey, string transactionId, decimal price);

        Task<string> ClaimDividend(string privateKey);

        Task<decimal> GetPendingDividends(string privateKey);

        Task<decimal> GetTokenPrice();

        Task<decimal> GetAmountRaised();
    }
}