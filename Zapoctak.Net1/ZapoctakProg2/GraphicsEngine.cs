using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace ZapoctakProg2
{
    public class GraphicsEngine
    {
        public GraphicsEngine(Level level, double scaleFactor)
        {

            this.pictureBox = level.Form.PictureBox;
            this.suns = level.Suns;
            this.planets = level.Planets;
            this.powerUps = level.PowerUps;
            this.scaleFactor = scaleFactor;
            this.maxDistance = level.Physics.MaxDistance;
            bmp = new Bitmap(pictureBox.Width, pictureBox.Height);
            gr = Graphics.FromImage(bmp);
            gr.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            pictureBox.Image = bmp;
        }

        
        private Level level;
        private List<Sun> suns;
        private List<Planet> planets;
        private List<PowerUp> powerUps;

        private PictureBox pictureBox;
        private Graphics gr;
        private Bitmap bmp;
        public Bitmap Bmp { get { return bmp; } }

        private double scaleFactor;

        private double maxDistance;

        //performs one tick of the graphics - draws everything
        public void Tick()
        {
            ClearScreen();
            DrawMaxDistance();
            DrawObjects();
        }

        //draws all the spaceobjects
        private void DrawObjects()
        {
            foreach (var sun in suns.Where(s => !s.IsDestroyed))
            {
                sun.Draw(gr, scaleFactor);
            }

            foreach (var planet in planets.Where(p => !p.IsDestroyed))
            {
                planet.Draw(gr, scaleFactor);
            }

            foreach (var powerUp in powerUps.Where(p => !p.IsDestroyed))
            {
                powerUp.Draw(gr, scaleFactor);
            }

            pictureBox.Refresh();
        }

        // Brush which will be used to draw the survival zone
        private Brush distBrush = new SolidBrush(Color.FromArgb(50, 50, 50));

        /// <summary>
        /// Draw the zone in which planets can survive
        /// </summary>
        private void DrawMaxDistance()
        {
            foreach (var sun in suns)
                gr.FillEllipse(distBrush, (float)((sun.XPos - maxDistance) * scaleFactor), (float)((sun.YPos - maxDistance) * scaleFactor),
                    2 * (float)(maxDistance * scaleFactor), 2 * (float)(maxDistance * scaleFactor));
        }

        //Brush used for the background color - its color the is the dead zone and the Good zone is drawn on top of it
        private Brush whiteBrush = new SolidBrush(Color.FromArgb(60, 45, 45));

        private void ClearScreen()
        {
            gr.FillRectangle(whiteBrush, new Rectangle(0, 0, bmp.Width, bmp.Height));
        }
    }
}
