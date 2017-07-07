using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Threading;
using ZapoctakProg2;

namespace Powerups
{
    class TemporaryDestroy : PowerUp
    {
        private readonly int destructionTime;

        private static readonly Pen CircumferencePen = new Pen(Color.Red, 3f);
        private static readonly Pen VelocityPen = new Pen(Color.Red, 3f) {EndCap = LineCap.ArrowAnchor};

        public TemporaryDestroy(double xPos, double yPos, double xVel, double yVel, double radius, int destructionTime) : base(xPos, yPos, xVel, yVel, radius)
        {
            this.destructionTime = destructionTime;
        }

        public TemporaryDestroy(Coordinates coords, int destructionTime) : base(coords)
        {
            this.destructionTime = destructionTime;
        }

        public override void Draw(Graphics graphics, double scaleFactor)
        {
            graphics.DrawEllipse(CircumferencePen, (float)((xPos - radius) * scaleFactor), (float)((yPos - radius) * scaleFactor),
                (float)(2 * radius * scaleFactor), (float)(2 * radius * scaleFactor));

            graphics.DrawLine(CircumferencePen,(float)(xPos + radius * 1.1),(float)(yPos + radius * 1.1),(float)(xPos - radius * 1.1),(float)(yPos - radius * 1.1));
            graphics.DrawLine(CircumferencePen, (float)(xPos + radius * 1.1), (float)(yPos - radius * 1.1), (float)(xPos - radius * 1.1), (float)(yPos + radius * 1.1));

            DrawVelocityArrowFromCircumference(graphics, scaleFactor, VelocityPen);
        }

        public override void ApplyPlanet(Level level, Planet planet)
        {
            planet.IsDestroyed = true;
            level.Form.Timer.Tick += new TimeKeeper(level, planet, level.Stopwatch.ElapsedMilliseconds, destructionTime).CheckTime;
        }

        public override void ApplySun(Level level, Sun sun)
        {
            sun.IsDestroyed = true;
            level.Form.Timer.Tick += new TimeKeeper(level, sun, level.Stopwatch.ElapsedMilliseconds, destructionTime).CheckTime;
        }

        public override void ApplyPowerup(Level level, PowerUp powerUp)
        {
            powerUp.IsDestroyed = true;
            level.Form.Timer.Tick += new TimeKeeper(level, powerUp, level.Stopwatch.ElapsedMilliseconds, destructionTime).CheckTime;
        }

        public override void ApplyTooFar(Level level)
        { // Do nothing 
        }

        public override void ApplyTimeOver(Level level)
        { // Do nothing
        }

        class TimeKeeper
        {
            private readonly Level level;
            private readonly long startTimeMs;
            private readonly SpaceObject resurrect;
            private readonly long endTimeMs;

            public TimeKeeper(Level level, SpaceObject resurrect, long startTimeMs, int timeDurationMs)
            {
                this.level = level;
                this.startTimeMs = startTimeMs;
                endTimeMs = startTimeMs + timeDurationMs;
                this.resurrect = resurrect;
            }

            public void CheckTime(object sender, EventArgs e)
            {
                if (level.IsEnded)
                {
                    level.Form.Timer.Tick -= CheckTime;
                    return;
                }

                if (level.Stopwatch.ElapsedMilliseconds > endTimeMs)
                {
                    resurrect.IsDestroyed = false;
                    level.Form.Timer.Tick -= CheckTime;
                }
            }
        }
    }

    class TemporaryDestroyParser : IPowerUpParser
    {
        public PowerUp Parse(LevelInputReader reader)
        {
            var coordinates = reader.ReadCoordinates();
            var time = int.Parse(reader.ReadLine());
            return new TemporaryDestroy(coordinates, time);
        }

        public string PowerUpId => "TempDestroy";
    }
}
