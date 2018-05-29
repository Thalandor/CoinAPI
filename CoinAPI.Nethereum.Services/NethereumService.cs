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
            var transactionInput = new TransactionInput(null, smartContractAddressConfiguration.TokenAddress, publicAddress, new HexBigInteger(3000000), new HexBigInteger(4), null);
            var transactionHash = await createOrderFunction.SendTransactionAsync(transactionInput, transactionId, publicAddress, amount, weiPrice);
            return transactionHash;
        }

        public async Task<string> BuyOrder(string privateKey, string transactionId)
        {
            var web3 = new Web3(new Account(privateKey), this.smartContractAddressConfiguration.NodeUrl);
            var publicAddress = GetPublicAddress(privateKey);
            var contract = web3.Eth.GetContract(Constants.AbiToken, smartContractAddressConfiguration.TokenAddress);
            var buyOrderFunction = contract.GetFunction("buyOrder");
            var orderFunction = contract.GetFunction("order");
            var order = await orderFunction.CallDeserializingToObjectAsync<Order>(transactionId, 0);
            var transactionHash = await buyOrderFunction.SendTransactionAsync(new TransactionInput(null, smartContractAddressConfiguration.TokenAddress, publicAddress, new HexBigInteger(3000000), new HexBigInteger(4), new HexBigInteger(order.Price)), transactionId);
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

    }
}
