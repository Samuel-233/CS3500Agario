using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AgarioModels
{

    public class GameObject
    {
        public int ID { get; set; }
        public System.Numerics.Vector2 pos { get; set; }
        public int ARGBColor { get; set; }
        public float Mass { get; set; }

        public GameObject(int ID, System.Numerics.Vector2 pos, int ARGBColor, float Mass)
        {
            this.ID = ID;
            this.pos = pos;
            this.ARGBColor = ARGBColor;
            this.Mass = Mass;
        }

        public GameObject(int ID, int X, int Y, int ARGBColor, float Mass):this(ID, new System.Numerics.Vector2(X, Y), ARGBColor, Mass){        }
    }
}