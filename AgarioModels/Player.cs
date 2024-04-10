namespace AgarioModels
{
    /// <summary>
    /// Author:    Shu Chen
    /// Partner:   Ping-Hsun Hsieh
    /// Date:      25/3/2024
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
    /// This is the player class, extended from the game object class, only have one more name variable
    /// </summary>
    public class Player : GameObject
    {
        /// <summary>
        /// Name of the player
        /// </summary>
        public string Name { get; set; }
    }
}