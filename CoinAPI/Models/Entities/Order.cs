using MongoRepository2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace CoinAPI.Models.Entities
{
    [Serializable]
    public class Order: Entity
    {
        [DataMember]
        public string TransactionBlockchain { get; set; }
        [DataMember]
        public string TransactionId { get; set; }
        [DataMember]
        public string Seller { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public decimal Price { get; set; }

    }
}
