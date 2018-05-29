using Nethereum.ABI.FunctionEncoding.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace CoinAPI.Nethereum.Services.Models.Functions
{    
    [FunctionOutput]
    public class Order
    {
        [Parameter("address", "owner", 1)]
        public string TransactionId { get; set; }

        [Parameter("address", "owner", 1)]
        public string Owner { get; set; }

        [Parameter("256", "amount", 2)]
        public BigInteger Amount { get; set; }

        [Parameter("uint256", "price", 3)]
        public BigInteger Price { get; set; }
    }
}
