using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ZapoctakProg2
{
    public interface IPowerUpParser
    {
        PowerUp Parse(LevelInputReader reader);
        string PowerUpId { get; }
    }

    public abstract class PowerUp : MovingSpaceObject
    {
        protected PowerUp(double xPos, double yPos, double xVel, double yVel, double radius) : base (xPos, yPos, xVel, yVel, radius)
        {

        }

        protected PowerUp(Coordinates coords): base(coords) { }

        /// <summary>
        /// Will be called after a crash with <code>Planet</code>
        /// </summary>
        public abstract void ApplyPlanet(Level level, Planet planet);
        
        /// <summary>
        /// Will be called after a crash with <code>Sun</code>
        /// </summary>
        public abstract void ApplySun(Level level, Sun sun);
        /// <summary>
        /// Will be called after a crash with <code>Powerup</code>
        /// </summary>
        public abstract void ApplyPowerup(Level level, PowerUp powerUp);
        /// <summary>
        /// Will be called after the planet has left the safe zone
        /// </summary>
        public abstract void ApplyTooFar(Level level);
        /// <summary>
        /// Will be called after the time has run out
        /// </summary>
        /// <param name="level"></param>
        public abstract void ApplyTimeOver(Level level);

    }


    public class ReduceTimePowerUp : PowerUp
    {
        private int amount;
        private Font font;
        private Pen pen = new Pen(Color.GreenYellow);
        private Brush textBrush = new SolidBrush(Color.Black);

        public ReduceTimePowerUp(double xPos, double yPos, double xVel, double yVel, double radius, int amount) : base (xPos, yPos, xVel, yVel, radius)
        {
            this.amount = amount;
            font = new Font(FontFamily.GenericSansSerif, (float)radius / 1.5f);
        }

        public ReduceTimePowerUp(Coordinates coords, int amount) : base(coords)
        {
            this.amount = amount;
            font = new Font(FontFamily.GenericSansSerif, (float) radius / 1.5f);
        }

        public override void Draw(Graphics gr, double scaleFactor)
        {
            gr.DrawEllipse(pen, (float)((xPos - radius) * scaleFactor), (float)((yPos - radius) * scaleFactor),
                (float)(2 * radius * scaleFactor), (float)(2 * radius * scaleFactor));

            gr.DrawString($"-{amount}", font, textBrush, (float)((xPos - radius / 2) * scaleFactor), (float)((yPos - radius / 2) * scaleFactor));
        }

        public override void ApplyPlanet(Level level, Planet planet)
        {
            
        }

        public override void ApplySun(Level level, Sun sun)
        {
            level.TimeLimit -= amount;
        }

        public override void ApplyPowerup(Level level, PowerUp powerUp)
        {
            throw new NotImplementedException();
        }

        public override void ApplyTooFar(Level level)
        {
            throw new NotImplementedException();
        }
        
        public override void ApplyTimeOver(Level level)
        {
            
        }
    }

    public class ReduceTimeParser : IPowerUpParser
    {
        public string PowerUpId => "ReduceTime";

        public PowerUp Parse(LevelInputReader reader)
        {
            var coords = reader.ReadCoordinates();
            var amount = int.Parse(reader.ReadLine());
            return new ReduceTimePowerUp(coords, amount);
        }
    }


}
