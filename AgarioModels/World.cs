using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarioModels
{
    public class World
    {
        public Dictionary<int, Player> players { get; set; }
        public Dictionary<int, Food> foods { get; set; }
        private ILogger logger;
        public World(ILogger logger)
        {
            this.logger = logger;
            players = new();
            foods = new();
        }
    }
}
