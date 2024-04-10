namespace AgarioModels
{
    //TODO Add Class header
    public class GameObject
    {
        public int ID { get; set; }
        public System.Numerics.Vector2 pos { get; set; }
        public float X { get => pos.X; set => pos = new System.Numerics.Vector2(value, pos.Y); }
        public float Y { get => pos.Y; set => pos = new System.Numerics.Vector2(pos.X, value); }
        public int ARGBColor { get; set; }
        public float Mass
        { get { return _mass; } set { radius = (float)Math.Sqrt(value / 3.14159f); _mass = value; } }
        private float _mass { get; set; }
        public float radius { get; set; }
    }
}