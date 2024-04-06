using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace AgarioModels
{
    public class Player : GameObject
    {
        public float radius { get { return (float)Math.Sqrt(Mass / 3.14159f); } }
        public string Name { get; }
        public Player(string name, int ID, System.Numerics.Vector2 pos, int ARGBColor, float Mass) : base(ID, pos, ARGBColor, Mass)
        { 
            Name = name;
        }

        public Player(string name, int ID, int X, int Y, int ARGBColor, float Mass) : base(ID, X,Y, ARGBColor, Mass)
        {
            Name = name;
        }
    }
}
