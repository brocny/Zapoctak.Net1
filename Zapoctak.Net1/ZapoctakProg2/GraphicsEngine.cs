using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ZapoctakProg2
{
    public class GraphicsEngine
    {
        // Brush which will be used to draw the survival zone
        public static Brush SafeZoneBrush { get; } = new SolidBrush(Color.FromArgb(40, 40, 45));
        //Brush used for the background color - its color the is the dead zone and the Good zone is drawn on top of it
        public static Brush DeathZoneBrush { get; } = new SolidBrush(Color.FromArgb(60, 10, 10));

        private readonly Level level;
        private readonly List<Sun> suns;
        private readonly List<Planet> planets;
        private readonly List<PowerUp> powerUps;

        private readonly PictureBox pictureBox;
        private readonly Graphics graphics;
        public Bitmap Bmp { get; }

        public double ScaleFactor { get; set; }

        public GraphicsEngine(Level level, double scaleFactor)
        {
            this.level = level;
            this.pictureBox = level.Form.PictureBox;
            this.suns = level.Suns;
            this.planets = level.Planets;
            this.powerUps = level.PowerUps;
            this.ScaleFactor = scaleFactor;
            Bmp = new Bitmap(pictureBox.Width, pictureBox.Height);
            graphics = Graphics.FromImage(Bmp);
            graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            pictureBox.Image = Bmp;
        }

        
        

        //performs one tick of the graphics - draws everything
        public void Tick()
        {
            ClearScreen();
            DrawSafeZone();
            DrawObjects();
        }

        //draws all the spaceobjects
        private void DrawObjects()
        {
            foreach (var sun in suns.Where(s => !s.IsDestroyed))
            {
                sun.Draw(graphics, ScaleFactor);
            }

            foreach (var planet in planets.Where(p => !p.IsDestroyed))
            {
                planet.Draw(graphics, ScaleFactor);
            }

            foreach (var powerUp in powerUps.Where(p => !p.IsDestroyed))
            {
                powerUp.Draw(graphics, ScaleFactor);
            }

            pictureBox.Refresh();
        }

       

        /// <summary>
        /// Draw the zone in which planets can survive
        /// </summary>
        private void DrawSafeZone()
        {
            foreach (var sun in suns)
                graphics.FillEllipse(SafeZoneBrush, (float)((sun.XPos - level.Physics.MaxSafeDistance) * ScaleFactor), (float)((sun.YPos - level.Physics.MaxSafeDistance) * ScaleFactor),
                    2 * (float)(level.Physics.MaxSafeDistance * ScaleFactor), 2 * (float)(level.Physics.MaxSafeDistance * ScaleFactor));
        }

        private void ClearScreen()
        {
            graphics.FillRectangle(DeathZoneBrush, new Rectangle(0, 0, Bmp.Width, Bmp.Height));
        }
    }
}
