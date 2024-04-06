using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgarioModels
{
    public class Food : GameObject
    {
        public float radius { get;}
        public Food(int id, System.Numerics.Vector2 pos, int ARGB, float mass) : base(id, pos, ARGB, mass)
        { 
            radius = (float)Math.Sqrt(mass / 3.14159f); 
        }

    }
}
