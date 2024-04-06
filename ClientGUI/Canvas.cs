using AgarioModels;

namespace ClientGUI
{
    internal class Canvas : IDrawable
    {
        World world;
        private readonly int width = 650;
        private readonly int height = 650;
        private readonly GraphicsView gv;
        /// <summary>
        /// Record Cam position
        /// </summary>
        private System.Numerics.Vector2 camPos = new System.Numerics.Vector2(2500f,2500f);
        /// <summary>
        /// Cam zoom, when self is bigger(radius bigger), the zoom is smaller get from 20/r
        /// </summary>
        float zoom = 1;
        /// <summary>
        /// The length of a canvas diagonal divided by two. 
        /// </summary>
        float halfdiagonal = ((float)Math.Sqrt(2)) / 2.0f;

        float temp = 1;

        public Canvas(World world, GraphicsView gv) 
        {
            width = (int)gv.WidthRequest;
            height = (int)gv.HeightRequest;

            this.world = world;
            halfdiagonal *= width;
            this.gv = gv;


            for(int i = 0; i <100; i++){
                this.world.foods.Add(i,new Food(i, new System.Numerics.Vector2(40 * i + 10*i, 2500), 1, 1256.636f));
            }
            this.world.foods.Add(100, new Food(100, new System.Numerics.Vector2(2175,2175), 1, 331000f));
            this.world.foods.Add(101, new Food(101, new System.Numerics.Vector2(3150, 3150), 1, 1327000f));
        }

        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            int alpha = 255;
            int red = 255;
            int green = 0;
            int blue = 0;

            canvas.FillColor = Colors.LightBlue;
            canvas.FillRectangle(0, 0, width, height);
            //Draw players
            lock (world.foods)
            {
                foreach (var food in world.foods)
                {
                    if (!ConvertFromWorldToScreen(food.Value.pos, food.Value.radius,
                          out System.Numerics.Vector2 screenPos, out float radius))
                        continue;

                    canvas.FillColor = Color.FromRgba(red, blue, green, alpha);
                    canvas.StrokeColor = Colors.Black;
                    canvas.DrawCircle(screenPos,radius);
                    canvas.DrawCircle(screenPos, radius);
                    green += 3;
                    blue += 8;
                    green %= 255;
                    blue %= 255;
                }
                temp += 0.01f;
                zoom = 0.5f;
                gv.Invalidate();
                /*                if (invalidateAlwaysCB.IsChecked)
                                {
                                    gv.Invalidate();
                                }

                                if (moveOnInvalidateCB.IsChecked)
                                {
                                    foreach (var box in boxes)
                                    {
                                        box.X++;
                                        box.Y++;
                                    }
                                }*/
            }
        }




        private bool ConvertFromWorldToScreen(
              in System.Numerics.Vector2 worldPos, in float radiusIn,
              out System.Numerics.Vector2 screenPos, out float radiusOut)
        {
            System.Numerics.Vector2 ptMinusCam = worldPos - camPos;
            screenPos = new();
            radiusOut = radiusIn;
            //if the distance of cam to pt is less than the half diagonal + radius, then we do not draw it.
            if(System.Numerics.Vector2.Dot(ptMinusCam, ptMinusCam)>Math.Pow(halfdiagonal/zoom+radiusIn,2))return false;

            //Cam pos is at the center of the canvas, so add half of the screen width and height
            screenPos = new System.Numerics.Vector2(width/zoom/2, height/zoom/2) + ptMinusCam;
            screenPos *= zoom;
            radiusOut = radiusIn * zoom;
            return true;
        }
    }
}
