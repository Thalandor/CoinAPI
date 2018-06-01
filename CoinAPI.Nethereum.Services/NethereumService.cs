using CoinAPI.Common.Models.Configuration;
using CoinAPI.Nethereum.Services.Models.Events;
using CoinAPI.Nethereum.Services.Models.Functions;
using Microsoft.Extensions.Options;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Signer;
using Nethereum.StandardTokenEIP20;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System;
using System.Numerics;
using System.Threading.Tasks;

namespace CoinAPI.Nethereum.Services
{
    public class NethereumService : INethereumService
    {

        private readonly SmartContractsAddressConfiguration smartContractAddressConfiguration;

        public NethereumService(IOptions<SmartContractsAddressConfiguration> config)
        {
            this.smartContractAddressConfiguration = config.Value;
        }

        public string GenerateNewPrivateKey()
        {
            var ecKey = EthECKey.GenerateKey();
            var privateKey = ecKey.GetPrivateKey();
            return privateKey;
        }

        public string GetPublicAddress(string privateKey)
        {
            return EthECKey.GetPublicAddress(privateKey);
        }

        public async Task<decimal> GetAccountETHBalance(string accountAddress)
        {
            var web3 = new Web3(this.smartContractAddressConfiguration.NodeUrl);
            var balanceWei = await web3.Eth.GetBalance.SendRequestAsync(accountAddress);
            var balanceEther = Web3.Convert.FromWei(balanceWei.Value);
            return balanceEther;
        }

        public async Task<decimal> GetAccountTokenBalance(string accountAddress)
        {
            var web3 = new Web3(this.smartContractAddressConfiguration.NodeUrl);
            StandardTokenService token = new StandardTokenService(web3, smartContractAddressConfiguration.TokenAddress);
            var balanceWei = await token.GetBalanceOfAsync<BigInteger>(accountAddress);
            var balanceEther = Web3.Convert.FromWei(balanceWei);
            return balanceEther;
        }

        public async Task<bool> InvestICO(string privateKey, decimal amount)
        {
            var web3 = new Web3(new Account(privateKey), this.smartContractAddressConfiguration.NodeUrl);
            var publicAddress = GetPublicAddress(privateKey);
            var weiAmount = Web3.Convert.ToWei(amount);
            var contract = web3.Eth.GetContract(Constants.AbiICO, smartContractAddressConfiguration.ICOAddress);
            //var fundTransferEvent = contract.GetEvent("FundTransfer");
            //var filterAll = await fundTransferEvent.CreateFilterAsync();
            //var filterMe = await fundTransferEvent.CreateFilterAsync(publicAddress);
            var txn = await web3.TransactionManager.SendTransactionAsync(new TransactionInput(null, smartContractAddressConfiguration.ICOAddress, publicAddress, new HexBigInteger(3000000), new HexBigInteger(4), new HexBigInteger(weiAmount)));
            //var log = await fundTransferEvent.GetFilterChanges<FundTransferEvent>(filterMe);
            return true;
        }
                
        public async Task<string> CreateOrder(string privateKey, string transactionId, decimal amount, decimal price)
        {
            var web3 = new Web3(new Account(privateKey), this.smartContractAddressConfiguration.NodeUrl);
            var publicAddress = GetPublicAddress(privateKey);
            var weiPrice = Web3.Convert.ToWei(price);
            var contract = web3.Eth.GetContract(Constants.AbiToken, smartContractAddressConfiguration.TokenAddress);
            var createOrderFunction = contract.GetFunction("createOrder");
            var realAmount = amount * (decimal)Math.Pow(10, 18);
            var transactionInput = new TransactionInput(null, smartContractAddressConfiguration.TokenAddress, publicAddress, new HexBigInteger(3000000), new HexBigInteger(4), null);
            var transactionHash = await createOrderFunction.SendTransactionAsync(transactionInput, transactionId, realAmount, weiPrice);
            return transactionHash;
        }

        public async Task<string> BuyOrder(string privateKey, string transactionId, decimal price)
        {
            var web3 = new Web3(new Account(privateKey), this.smartContractAddressConfiguration.NodeUrl);
            var publicAddress = GetPublicAddress(privateKey);
            var contract = web3.Eth.GetContract(Constants.AbiToken, smartContractAddressConfiguration.TokenAddress);
            var weiPrice = Web3.Convert.ToWei(price);
            var buyOrderFunction = contract.GetFunction("buyOrder");
            var transactionHash = await buyOrderFunction.SendTransactionAsync(new TransactionInput(null, smartContractAddressConfiguration.TokenAddress, publicAddress, new HexBigInteger(3000000), new HexBigInteger(4), new HexBigInteger(weiPrice)), transactionId);
            return transactionHash;
        }

        public async Task<string> ClaimDividend(string privateKey)
        {
            var web3 = new Web3(new Account(privateKey), this.smartContractAddressConfiguration.NodeUrl);
            var publicAddress = GetPublicAddress(privateKey);
            var contract = web3.Eth.GetContract(Constants.AbiToken, smartContractAddressConfiguration.TokenAddress);
            var claimDividendsFunction = contract.GetFunction("claimDividends");            
            var transactionHash = await claimDividendsFunction.SendTransactionAsync(new TransactionInput(null, smartContractAddressConfiguration.TokenAddress, publicAddress, new HexBigInteger(3000000), new HexBigInteger(4), null));
            return transactionHash;
        }

        public async Task<decimal> GetPendingDividends(string privateKey)
        {
            var web3 = new Web3(this.smartContractAddressConfiguration.NodeUrl);
            var publicAddress = GetPublicAddress(privateKey);
            var contract = web3.Eth.GetContract(Constants.AbiToken, smartContractAddressConfiguration.TokenAddress);
            var pendingDividendsFunction = contract.GetFunction("dividends");
            var pendingDividendsWei = await pendingDividendsFunction.CallDeserializingToObjectAsync<long>(publicAddress).ConfigureAwait(false);
            return Web3.Convert.FromWei(pendingDividendsWei);
        }

        public async Task<decimal> GetTokenPrice()
        {
            var web3 = new Web3(this.smartContractAddressConfiguration.NodeUrl);
            var contract = web3.Eth.GetContract(Constants.AbiICO, smartContractAddressConfiguration.ICOAddress);
            var tokenPriceFunction = contract.GetFunction("price");
            var tokenPriceWei = await tokenPriceFunction.CallAsync<long>().ConfigureAwait(false);
            return Web3.Convert.FromWei(tokenPriceWei);
        }

        public async Task<decimal> GetAmountRaised()
        {
            var web3 = new Web3(this.smartContractAddressConfiguration.NodeUrl);
            var contract = web3.Eth.GetContract(Constants.AbiICO, smartContractAddressConfiguration.ICOAddress);
            var fundsRaisedFunction = contract.GetFunction("amountRaised");
            var fundsRaisedWei = await fundsRaisedFunction.CallAsync<long>().ConfigureAwait(false);
            return Web3.Convert.FromWei(fundsRaisedWei);
        }


    }
}
