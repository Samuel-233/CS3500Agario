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
        private System.Numerics.Vector2 camPos = new System.Numerics.Vector2(2500f, 2500f);
        /// <summary>
        /// Cam zoom, when self is bigger(radius bigger), the zoom is smaller get from 10/r
        /// </summary>
        float targetZoom = 1;

        float currentZoom = 0;

        

        /// <summary>
        /// Maximum zoom 
        /// </summary>
        readonly float maxZoom;

        /// <summary>
        /// The length of a canvas diagonal divided by two. 
        /// </summary>
        float halfdiagonal = ((float)Math.Sqrt(2)) / 2.0f;

        public Canvas(World world, GraphicsView gv)
        {
            width = (int)gv.WidthRequest;
            height = (int)gv.HeightRequest;

            this.world = world;
            halfdiagonal *= width;
            this.gv = gv;

            maxZoom = (float)gv.WidthRequest / 5000f;
            /*
            //TODO Remove this debug
            for (int i = 0; i < 100; i++)
            {
                this.world.foods.Add(i, new Food(i, new System.Numerics.Vector2(40 * i + 10 * i, 2500), 1, 1256.636f));
            }
            this.world.foods.Add(100, new Food(100, new System.Numerics.Vector2(2175, 2175), 1, 331000f));
            this.world.foods.Add(101, new Food(101, new System.Numerics.Vector2(3150, 3150), 1, 1327000f));*/
        }
        //TODO Make camera cannot see out side the border
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            if(world.playerDead) {
            //T
                canvas.FillColor = Color.FromRgba("00000033");
                canvas.FillRectangle(0, 0, width, height);
                return;
            }


            try{
                camPos = world.players[world.playerID].pos;
            }catch(Exception e){
                return;
            }
            
            
            targetZoom = Math.Max(10f / world.players[world.playerID].radius, maxZoom);
            currentZoom += 0.1f * (targetZoom - currentZoom);

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

                    canvas.FillColor = Color.FromInt(food.Value.ARGBColor);
                    canvas.StrokeColor = Colors.Black;
                    canvas.DrawCircle(screenPos, radius);
                    canvas.FillCircle(screenPos, radius);
                }

                lock (world.players)
                {
                    foreach (var player in world.players)
                    {
                        if (!ConvertFromWorldToScreen(player.Value.pos, player.Value.radius,
                              out System.Numerics.Vector2 screenPos, out float radius))
                            continue;

                        canvas.FillColor = Color.FromInt(player.Value.ARGBColor);
                        canvas.StrokeColor = Colors.Black;
                        canvas.DrawCircle(screenPos, radius);
                        canvas.FillCircle(screenPos, radius);
                        /*                        canvas.FontColor = Colors.White;
                                                canvas.FontSize = */

                    }
                    gv.Invalidate();

                }
            }

            


            bool ConvertFromWorldToScreen(
                 in System.Numerics.Vector2 worldPos, in float radiusIn,
                 out System.Numerics.Vector2 screenPos, out float radiusOut)
            {
                System.Numerics.Vector2 ptMinusCam = worldPos - camPos;
                screenPos = new();
                radiusOut = radiusIn;
                //if the distance of cam to pt is less than the half diagonal + radius, then we do not draw it.
                if (System.Numerics.Vector2.Dot(ptMinusCam, ptMinusCam) > Math.Pow(halfdiagonal / currentZoom + radiusIn, 2)) return false;

                //Cam pos is at the center of the canvas, so add half of the screen width and height
                screenPos = new System.Numerics.Vector2(width / currentZoom / 2, height / currentZoom / 2) + ptMinusCam;
                screenPos *= currentZoom;
                radiusOut = radiusIn * currentZoom;
                return true;
            }
        }
    }
}
