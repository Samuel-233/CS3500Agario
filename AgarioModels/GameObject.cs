using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AgarioModels
{
    /*
    A GameObject must at least represent the following data
    A unique id number.This value should be a long integer.
    The location of the game object in the world (x, y). (e.g., 205.6, 199.9)
    I suggest using a built-in type(e.g., System.Numerics.Vector2).  Read up on this library.It is likely one you will use in many future projects.
    Note: the position represents the center of the object (e.g., center or the circle). 
    X and Y properties for the game object (which do not have setters) that return the appropriate location value.  These should reference the location value above.
    An ARGBcolor - just for display purposes. ARGBs are integers.  
    A mass - used to determine how big to draw the circle.  Mass is a float.*/

    public class GameObject
    {
        public int id { get; set; }
        public System.Numerics.Vector2 pos { get; set; }
        public int ARGB { get; set; }
        public float mass { get; set; }

        public GameObject(int id, System.Numerics.Vector2 pos, int ARGB, float mass)
        {
            this.id = id;
            this.pos = pos;
            this.ARGB = ARGB;
            this.mass = mass;
        }
    }
}