using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AgarioModels
{
    public class World
    {
        public Dictionary<int, Player> players { get; set; }
        public Dictionary<int, Food> foods { get; set; }
        public int playerID { get; set; }
        public string heartBeat { get; set; }
        public bool playerDead { get; set; }
        private ILogger logger;
        


        public World(ILogger logger)
        {
            this.logger = logger;
            players = new();
            foods = new();
        }

        public void InitializeFood(string JSON)
        {
            List<Food>? foods = new();
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
            lock(this.foods){
                foreach (int id in foodIdToRemove)
                {
                    foods.Remove(id);
                }
            }

        }

        //TODO show game over when remove self
        public void RemovePlayer(string JSON)
        {
            List<int> playerIdToRemove = new();
            DeserializeJSON(ref playerIdToRemove, JSON);
            lock(this.players){
                foreach (int id in playerIdToRemove)
                {
                    if (playerID == id) {
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
                    this.players[player.ID] = player;
                }
            }
        }

        private void DeserializeJSON <T>(ref T result, string JSON){
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
