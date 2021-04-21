using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnceMi.AspNetCore.Config
{
    public class ConnectionStringsNode
    {
        public string NpgsqlDbConnectionString { get; set; }

        public string RedisDataConnectionString { get; set; }
    }
}
