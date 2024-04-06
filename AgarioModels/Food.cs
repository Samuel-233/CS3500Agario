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
        
        public Food(int ID, System.Numerics.Vector2 pos, int ARGBColor, float Mass) : base(ID, pos, ARGBColor, Mass)
        { 
            radius = (float)Math.Sqrt(Mass / 3.14159f); 
        }

        public Food(int ID, int X, int Y, int ARGBColor, float Mass) : base(ID, X, Y, ARGBColor, Mass)
        {
            radius = (float)Math.Sqrt(Mass / 3.14159f);
        }

    }
}
