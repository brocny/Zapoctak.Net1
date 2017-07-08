using System.Drawing;
using System.Drawing.Drawing2D;
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

    /// <summary>
    /// Increase the maximum safe distance upon contact with any other object
    /// </summary>
    public class ExtendSafeZone : PowerUp
    {
        private static readonly Pen CircumferencePen = new Pen(Color.DarkMagenta, 3f);
        private static readonly Pen VelocityPen = new Pen(Color.DarkMagenta, 3);
        private static readonly Brush TextBrush = new SolidBrush(Color.DarkMagenta);
        private static readonly Font Font = new Font(FontFamily.GenericSansSerif, 9.5f);

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
        }

        public override void ApplyPowerup(Level level, PowerUp powerUp)
        {
            // effect is amplified upon collison with the same type of powerup
            if (powerUp.GetType() == typeof(ExtendSafeZone))
            {
                level.Physics.MaxSafeDistance *= extensionFactor * 1.1;
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
        { // Do nothing
        }

        public override void ApplyTooFar(Level level)
        {
            level.Physics.MaxSafeDistance *= extensionFactor;
        }

        public override void Draw(Graphics graphics, double scaleFactor)
        {
            graphics.DrawEllipse(CircumferencePen, 
                (float)((xPos - radius) * scaleFactor), (float)((yPos - radius) * scaleFactor),
                (float)(2 * radius * scaleFactor), (float)(2 * radius * scaleFactor));

            graphics.DrawString($"x{extensionFactor:N1}", Font, TextBrush, (float)((xPos - radius / 2) * scaleFactor), (float)((yPos - radius / 2) * scaleFactor));

            DrawVelocityArrowFromCircumference(graphics, scaleFactor, VelocityPen);
        }
    }
}
