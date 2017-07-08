using System;
using System.Drawing;
using System.Globalization;

namespace ZapoctakProg2
{
    public class Sun : SpaceObject
    {
        public Sun(double xPos, double yPos, double xVel, double yVel, double radius, double temperature, double mass) : base(xPos, yPos, xVel, yVel, radius)
        {
            this.temperature = temperature;
            this.mass = mass;
            color = TemperatureToColor();
            brush = new SolidBrush(color);
        }

        public Sun(Coordinates coords, double temperature, double mass) : 
            this(coords.XPos, coords.YPos, coords.XVel, coords.YVel, coords.Radius, temperature, mass) { }

        //affects the strength of the gravity field produced by the sun
        private double mass;
        public double Mass { get { return mass; } }

        //affects how much it burns nearby planets & the displayed color of the sun
        private double temperature;
        public double Temperature { get { return temperature; } }

        private Color color;
        private Brush brush;
        private static readonly Font MassFont = new Font(FontFamily.GenericSansSerif, 9.5f);

        /// <summary>
        /// Computes the color of the sun based on its temperature
        /// </summary>
        /// <returns><code>Color</code> corresponding to <code>temperature</code></returns>
        private Color TemperatureToColor()
        {
            //algorithm to compute RGB values from absolute temperature source: http://www.tannerhelland.com/4435/convert-temperature-rgb-algorithm-code/
            double red;
            double green;
            double blue;
            //calculate red
            if (temperature <= 6600)
            {
                red = 255;
            }
            else
            {
                red = temperature / 100 - 60;
                red = 330 * Math.Pow(red, -0.133);
                if (red < 0) red = 0;
                if (red > 255) red = 255;
            }
            //calculate green
            if (temperature <= 6600)
                green = 99.5 * Math.Log(temperature / 100) - 161;
            else
                green = 288 * Math.Pow(temperature / 100, -0.0755);
            if (green > 255) green = 255;
            if (green < 0) green = 0;
            //calculate blue
            if (temperature >= 6600)
            {
                blue = 255;
            }
            else
            {
                if (temperature <= 1900)
                {
                    blue = 0;
                }
                else
                {
                    blue = temperature / 100 - 10;
                    blue = 138.5 * Math.Log(blue) - 305;
                    if (blue < 0) blue = 0;
                    if (blue > 255) blue = 255;
                }
            }

            return Color.FromArgb((int)red, (int)green, (int)blue);
        }

  
        public override void Draw(Graphics graphics, double scaleFactor)
        {
            //font used to display the mass of the sun

            graphics.FillEllipse(brush, (float)((xPos - radius) * scaleFactor), (float)((yPos - radius) * scaleFactor),
                (float)(2 * radius * scaleFactor), (float)(2 * radius * scaleFactor));

            graphics.DrawString(mass.ToString(CultureInfo.InvariantCulture), MassFont, Brushes.Black, (float)((xPos - 3.7 * scaleFactor * mass.ToString(CultureInfo.InvariantCulture).Length) * scaleFactor), (float)((yPos - 7) * scaleFactor));
        }

    }
}