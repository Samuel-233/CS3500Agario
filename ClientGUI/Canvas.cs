using AgarioModels;
using System.Numerics;
using Font = Microsoft.Maui.Graphics.Font;

namespace ClientGUI
{
    internal class Canvas : IDrawable
    {
        private World world;
        private readonly int width = 650;
        private readonly int height = 650;

        /// <summary>
        /// Record Cam position
        /// </summary>
        public Vector2 camPos { get; set; } = new Vector2(2500f, 2500f);

        /// <summary>
        /// Cam zoom, when self is bigger(radius bigger), the zoom is smaller get from 10/r
        /// </summary>
        private float targetZoomIn = 1;

        /// <summary>
        /// Current Cam zoom, this will gradually go to target zoom, to achieve a lerp animation of zoom
        /// </summary>
        public float currentZoomIn { get; set; } = 0;

        /// <summary>
        /// Maximum zoom so camera won't get inf small
        /// </summary>
        private readonly float maxZoomIn;

        /// <summary>
        /// The length of a canvas diagonal divided by two.
        /// </summary>
        private float halfdiagonal = ((float)Math.Sqrt(2)) / 2.0f;

        /// <summary>
        /// Constructor of the Canvas
        /// </summary>
        /// <param name="world">world data</param>
        /// <param name="gv">Graphic view in MAUI</param>
        public Canvas(World world, GraphicsView gv)
        {
            width = (int)gv.WidthRequest;
            height = (int)gv.HeightRequest;

            this.world = world;
            halfdiagonal *= width;

            maxZoomIn = (float)gv.WidthRequest / 5000f;
        }

        /// <summary>
        /// Will called every time draw the canvas. It will read all foods and players and draw it on the canvas
        /// </summary>
        /// <param name="canvas"></param>
        /// <param name="dirtyRect"></param>
        public void Draw(ICanvas canvas, RectF dirtyRect)
        {
            //If player dead, draw the game over screen.
            if (world.playerDead)
            {
                DrawGameOver(canvas);
                return;
            }

            try
            {
                targetZoomIn = Math.Max(10f / world.players[world.playerID].radius, maxZoomIn);
                currentZoomIn += 0.1f * (targetZoomIn - currentZoomIn);
            }
            catch (Exception e)
            {
                return;
            }

            GetTrueCamPos();

            //Clear the old frame
            canvas.FillColor = Colors.LightBlue;
            canvas.FillRectangle(0, 0, width, height);

            DrawFoods(canvas);
            DrawPlayers(canvas);
            DrawMiniMap(canvas);

            void DrawMiniMap(ICanvas canvas){
                if (currentZoomIn < maxZoomIn + 0.01) return;
                canvas.StrokeSize = 2;
                canvas.DrawRectangle(0, 0, width/10, height/10);
                canvas.FillColor = Color.FromRgba("#00000033");
                canvas.FillRectangle(0, 0, width / 10, height / 10);
                float camWidthOnMap = width / currentZoomIn / 5000 * width / 10;

                //CamPos On the Map, but it center is on the top left
                Vector2 camPosOnMap = camPos / 5000 * width / 10 - new Vector2(camWidthOnMap/2,camWidthOnMap/2);
                canvas.DrawRectangle(camPosOnMap.X, camPosOnMap.Y, camWidthOnMap, camWidthOnMap);
                canvas.FillColor = Color.FromRgba("#ffff3266");
                canvas.FillRectangle(camPosOnMap.X, camPosOnMap.Y, camWidthOnMap, camWidthOnMap);
            }

            //Draw Player
            void DrawPlayers(ICanvas canvas)
            {
                canvas.FontColor = Colors.Black;
                canvas.Font = Font.Default;
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

                        canvas.FontSize = radius;
                        canvas.DrawString(player.Value.Name, screenPos.X, screenPos.Y, width, height, HorizontalAlignment.Left, VerticalAlignment.Top);
                    }
                }
            }
            //Draw Food
            void DrawFoods(ICanvas canvas)
            {
                //Draw players
                lock (world.foods)
                {
                    foreach (var food in world.foods)
                    {
                        if (!ConvertFromWorldToScreen(food.Value.pos, food.Value.radius,
                              out Vector2 screenPos, out float radius))
                            continue;

                        canvas.FillColor = Color.FromInt(food.Value.ARGBColor);
                        canvas.StrokeColor = Colors.Black;
                        canvas.DrawCircle(screenPos, radius);
                        canvas.FillCircle(screenPos, radius);
                    }
                }
            }
            //Draw Game Over Frame
            void DrawGameOver(ICanvas canvas)//TODO Draw other info
            {
                canvas.FillColor = Color.FromRgba("#00000033");
                canvas.FillRectangle(0, 0, width, height);
                canvas.FontColor = Colors.White;
                canvas.FontSize = 36;
                canvas.Font = Font.DefaultBold;
                canvas.DrawString("Game Over!", 0, 60, width, height, HorizontalAlignment.Center, VerticalAlignment.Top);

                canvas.FontSize = 18;
                canvas.Font = Font.Default;
                canvas.DrawString("Left Click to restart", 0, 400, width, height, HorizontalAlignment.Center, VerticalAlignment.Top);
            }
        }

        /// <summary>
        /// Calculate the true camera position. and limit it form seeing the boundary.
        /// </summary>
        private void GetTrueCamPos()
        {
            //To make the player cannot see outside the border, we need to limit the camera position
            camPos = world.players[world.playerID].pos;
            float halfCamSizeAfterZoom = width / currentZoomIn / 2;
            Vector2 topLeft = camPos - new Vector2(halfCamSizeAfterZoom, halfCamSizeAfterZoom);
            topLeft.X = Math.Max(0, topLeft.X);
            //By use min and max func, limit the camera pos in a square that camera cannot see outside
            topLeft = Vector2.Max(new Vector2(0, 0), topLeft);
            topLeft = Vector2.Min(new Vector2(5000, 5000) - new Vector2(halfCamSizeAfterZoom, halfCamSizeAfterZoom) * 2, topLeft);
            camPos = topLeft + new Vector2(halfCamSizeAfterZoom, halfCamSizeAfterZoom);
        }

        /// <summary>
        /// Convert a position from world cord to screen cord
        /// </summary>
        /// <param name="worldPos">World Position</param>
        /// <param name="radiusIn">Object radius</param>
        /// <param name="screenPos">Output Screen position</param>
        /// <param name="radiusOut">output final radius in screen space, with zoom scaled</param>
        /// <returns>return true if we decide to draw the object return false if it is too far from player</returns>
        private bool ConvertFromWorldToScreen(
                                     in Vector2 worldPos, in float radiusIn,
                                     out Vector2 screenPos, out float radiusOut)
        {
            Vector2 ptMinusCam = worldPos - camPos;
            screenPos = new();
            radiusOut = radiusIn;
            //if the distance of cam to pt is less than the half diagonal + radius, then we do not draw it.
            if (Vector2.Dot(ptMinusCam, ptMinusCam) > Math.Pow(halfdiagonal / currentZoomIn + radiusIn, 2)) return false;

            //Cam pos is at the center of the canvas, so add half of the screen width and height
            screenPos = new Vector2(width / currentZoomIn / 2, height / currentZoomIn / 2) + ptMinusCam;
            screenPos *= currentZoomIn;
            radiusOut = radiusIn * currentZoomIn;
            return true;
        }
    }
}