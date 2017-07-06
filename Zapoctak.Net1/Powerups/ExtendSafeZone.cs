using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZapoctakProg2;

namespace Powerups
{
    public class ExtendSafeZoneParser : IPowerUpParser
    {
        public PowerUp Parse(LevelInputReader reader)
        {
            var coordinates = reader.ReadCoordinates();
            var amount = float.Parse(reader.ReadLine());
            return new ExtendSafeZone(coordinates, amount);
        }

        public string PowerUpId { get; } = "ExtendSafeZone";
    }

    public class ExtendSafeZone : PowerUp
    {
        private static readonly Brush Brush = new SolidBrush(Color.Magenta);
        private static readonly Pen VelocityPen = new Pen(Color.AntiqueWhite, 3);

        private readonly float extensionFactor;

        static ExtendSafeZone()
        {
            VelocityPen.EndCap = LineCap.ArrowAnchor;
        }

        public ExtendSafeZone(Coordinates coordinates, float extensionFactor) : base(coordinates)
        {
            this.extensionFactor = extensionFactor;
        }

        public ExtendSafeZone(double xPos, double yPos, double xVel, double yVel, double radius, float extensionFactor) :
            base(xPos, yPos, xVel, yVel, radius)
        {
            this.extensionFactor = extensionFactor;
        }

        public override void ApplyPlanet(Level level, Planet planet)
        {
            level.Physics.MaxSafeDistance *= extensionFactor;
            level.Graphics.ScaleFactor /= extensionFactor;
        }

        public override void ApplyPowerup(Level level, PowerUp powerUp)
        {
            if (powerUp.GetType() == typeof(ExtendSafeZone))
            {
                level.Physics.MaxSafeDistance *= extensionFactor * 1.1;
                level.Graphics.ScaleFactor *= extensionFactor
            }
            else
            {
                level.Physics.MaxSafeDistance *= extensionFactor;
            }
        }

        public override void ApplySun(Level level, Sun sun)
        {
            level.Physics.MaxSafeDistance *= extensionFactor;
        }

        public override void ApplyTimeOver(Level level)
        {
        }

        public override void ApplyTooFar(Level level)
        {
            level.Physics.MaxSafeDistance *= extensionFactor;
        }

        public override void Draw(Graphics gr, double scaleFactor)
        {
            gr.FillEllipse(Brush, (float)((xPos - radius) * scaleFactor), (float)((yPos - radius) * scaleFactor),
                (float)(2 * radius * scaleFactor), (float)(2 * radius * scaleFactor));

            DrawVelocityArrowFromCentre(gr, scaleFactor, 2, VelocityPen);
        }
    }
}
