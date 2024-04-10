namespace AgarioModels
{
    /// <summary>
    /// Author:    Shu Chen
    /// Partner:   Ping-Hsun Hsieh
    /// Date:      10/4/2024
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
    /// This is a class is the base class of other game objects, contains basics variables like game id, pos, and etc.
    /// </summary>
    public class GameObject
    {
        /// <summary>
        /// Each Game object's special id
        /// </summary>
        public int ID { get; set; }
        /// <summary>
        /// Pos in world space
        /// </summary>
        public System.Numerics.Vector2 pos { get; set; }
        /// <summary>
        /// Pos's X component in world space
        /// </summary>
        public float X { get => pos.X; set => pos = new System.Numerics.Vector2(value, pos.Y); }
        /// <summary>
        /// Pos's Y component in world space
        /// </summary>
        public float Y { get => pos.Y; set => pos = new System.Numerics.Vector2(pos.X, value); }
        /// <summary>
        /// Color of game object
        /// </summary>
        public int ARGBColor { get; set; }
        /// <summary>
        /// Mass of the game object, when update, also update the radius, true mass value is in _mass variable
        /// </summary>
        public float Mass
        { get { return _mass; } set { radius = (float)Math.Sqrt(value / 3.14159f); _mass = value; } }
        /// <summary>
        /// A mass variable that store the mass value
        /// </summary>
        private float _mass { get; set; }
        /// <summary>
        /// Radius of the game object
        /// </summary>
        public float radius { get; set; }
    }
}