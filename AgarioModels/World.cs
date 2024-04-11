using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgarioModels
{
    //TODO Add Class header
    public class World
    {
        public Dictionary<int, Player> players { get; set; }
        public Dictionary<int, Food> foods { get; set; }
        /// <summary>
        /// The ID of the player for this client
        /// </summary>
        public int playerID { get; set; }
        public bool playerDead { get; set; }
        public float playerRadius;
        private ILogger logger;

        /// <summary>
        /// Constructor of the world class
        /// </summary>
        /// <param name="logger">logger</param>
        public World(ILogger logger)
        {
            this.logger = logger;
            players = new();
            foods = new();
        }
        //TODO Add all func header

        public void InitializeFood(string JSON)
        {
            List<Food> foods = new();
            DeserializeJSON(ref foods, JSON);

            lock (this.foods)
            {
                foreach (Food food in foods)
                {
                    this.foods.TryAdd(food.ID, food);
                }
            }
        }

        public void RemoveFood(string JSON)
        {
            List<int> foodIdToRemove = new();
            DeserializeJSON(ref foodIdToRemove, JSON);
            lock (this.foods)
            {
                foreach (int id in foodIdToRemove)
                {
                    foods.Remove(id);
                }
            }
        }

        public void RemovePlayer(string JSON)
        {
            List<int> playerIdToRemove = new();
            DeserializeJSON(ref playerIdToRemove, JSON);
            lock (this.players)
            {
                foreach (int id in playerIdToRemove)
                {
                    if (playerID == id)
                    {
                        playerDead = true;
                        logger.LogInformation("Player Dead, Waiting for restart");
                    }
                    players.Remove(id);
                }
            }
        }

        public void UpdatePlayer(string JSON)
        {
            List<Player>? players = new();
            DeserializeJSON(ref players, JSON);

            lock (this.players)
            {
                foreach (Player player in players)
                {
                    //Update the radius
                    if(player.ID == playerID && player.radius > playerRadius) playerRadius = player.radius;
                    this.players[player.ID] = player;
                }
            }
        }

        private void DeserializeJSON<T>(ref T result, string JSON)
        {
            try
            {
                result = JsonSerializer.Deserialize<T>(JSON) ?? throw new Exception("Bad JSON");
            }
            catch (Exception ex)
            {
                logger.LogError($"Error Occur during De serialize info, {ex.Message}");
            }
        }
    }
}