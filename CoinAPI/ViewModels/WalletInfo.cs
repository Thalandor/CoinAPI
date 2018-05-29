using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinAPI.ViewModels
{
    public class WalletInfo
    {
        public string Username { get; set; }
        public string Address { get; set; }
        public decimal ETHBalance { get; set; }
        public decimal TokenBalance { get; set; }
    }
}
