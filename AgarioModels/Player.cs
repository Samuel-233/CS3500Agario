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
        public float radius { get { return (float)Math.Sqrt(mass / 3.14159f); } }
        public Player(int id, Vector2 pos, int ARGB, float mass) : base(id, pos, ARGB, mass)
        {
        }
    }
}
