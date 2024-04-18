using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace AgarioModels
{
    /// <summary>
    /// Author:    Shu Chen
    /// Partner:   Ping-Hsun Hsieh
    /// Date:      18/4/2024
    /// Course:    CS 3500, University of Utah, School of Computing
    /// Copyright: CS 3500 and Shu Chen - This work may not
    ///            be copied for use in Academic Coursework.
    ///
    /// I, Shu Chen, certify that I wrote this code from scratch and
    /// did not copy it in part or whole from another source.  All
    /// references used in the completion of the assignments are cited
    /// in my README file.
    ///
    /// File Contents
    /// World class, store all game object information, also has the id of this client, and status to track current player is dead or not
    /// </summary>
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

        /// <summary>
        /// Initialize the food from the json
        /// </summary>
        /// <param name="JSON">a json contain all foods</param>
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

        /// <summary>
        /// Use json info to remove foods
        /// </summary>
        /// <param name="JSON">json that contains foods that need to remove</param>
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

        /// <summary>
        /// Use json info to remove players
        /// </summary>
        /// <param name="JSON">json that contains players that need to remove</param>
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

        /// <summary>
        /// Update player position by using json
        /// </summary>
        /// <param name="JSON">json that contains player info</param>
        public void UpdatePlayer(string JSON)
        {
            List<Player>? players = new();
            DeserializeJSON(ref players, JSON);

            lock (this.players)
            {
                foreach (Player player in players)
                {
                    //Update the radius
                    if (player.ID == playerID && player.radius > playerRadius) playerRadius = player.radius;
                    this.players[player.ID] = player;
                }
            }
        }

        /// <summary>
        /// De serialize json in to a object
        /// </summary>
        /// <typeparam name="T">the target type of object</typeparam>
        /// <param name="result">result that will be returned</param>
        /// <param name="JSON">json info that contain this object</param>
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