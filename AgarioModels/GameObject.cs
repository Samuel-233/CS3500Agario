using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AgarioModels
{

    public class GameObject
    {
        public int ID { get; set; }
        public System.Numerics.Vector2 pos { get; set; }
        public float X { get => pos.X; set => pos = new System.Numerics.Vector2(value, pos.Y); }
        public float Y { get => pos.Y; set => pos = new System.Numerics.Vector2(pos.X, value); }
        public int ARGBColor { get; set; }
        public float Mass { get { return _mass; } set { radius = (float)Math.Sqrt(value / 3.14159f); _mass = value; } }
        private float _mass{  get; set; }
        public float radius { get; set; }

        /*        public GameObject(int ID, System.Numerics.Vector2 pos, int ARGBColor, float Mass)
                {
                    this.ID = ID;
                    this.pos = pos;
                    this.ARGBColor = ARGBColor;
                    this.Mass = Mass;
                }

                public GameObject( int X, int Y, int ARGBColor, int ID, float Mass) : this(ID, new System.Numerics.Vector2(X, Y), ARGBColor, Mass) { }*/
    }
}