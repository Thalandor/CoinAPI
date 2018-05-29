using MongoDB.Bson.Serialization.Attributes;
using MongoRepository2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinAPI.Models.Entities
{
    public class UserInfo: Entity
    {
        public string Username { get; set; }

        public string PrivateKey { get; set; }
    }
}
