using Nethereum.ABI.FunctionEncoding.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinAPI.Nethereum.Services.Models.Events
{
    // event FundTransfer(address indexed backer, uint indexed amount);
    public class FundTransferEvent
    {
        [Parameter("address", "backer", 1, true)]
        public string Baker { get; set; }

        [Parameter("uint", "amount", 2, true)]
        public int Amount { get; set; }
    }
}
