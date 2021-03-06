﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;

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
        { }

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
        public abstract void ApplyTimeOver(Level level);

    }


    public class ReduceTimePowerUp : PowerUp
    {
        private readonly int reduceAmount;
        private static readonly Font Font = new Font(FontFamily.GenericSansSerif, 9.5f);
        private static readonly Pen CircumferencePen = new Pen(Color.GreenYellow, 3f);
        private static readonly Pen VelocityPen = new Pen(Color.GreenYellow, 3f) {EndCap = LineCap.ArrowAnchor};
        private static readonly Brush TextBrush = new SolidBrush(Color.GreenYellow);

        public ReduceTimePowerUp(double xPos, double yPos, double xVel, double yVel, double radius, int reduceAmount) : base (xPos, yPos, xVel, yVel, radius)
        {
            this.reduceAmount = reduceAmount;
        }

        public ReduceTimePowerUp(Coordinates coords, int reduceAmount) : base(coords)
        {
            this.reduceAmount = reduceAmount;
        }

        public override void Draw(Graphics graphics, double scaleFactor)
        {
            graphics.DrawEllipse(CircumferencePen, (float)((xPos - radius) * scaleFactor), (float)((yPos - radius) * scaleFactor),
                (float)(2 * radius * scaleFactor), (float)(2 * radius * scaleFactor));

            graphics.DrawString($"-{reduceAmount}", Font, TextBrush, (float)((xPos - radius / 2) * scaleFactor), (float)((yPos - radius / 2) * scaleFactor));

            DrawVelocityArrowFromCircumference(graphics, scaleFactor, VelocityPen);
        }

        public override void ApplyPlanet(Level level, Planet planet)
        {
            switch (planet.Type)
            {
                case Planet.PlanetType.Good:
                    level.TimeLimit -= reduceAmount;
                    break;
                case Planet.PlanetType.Neutral:
                    break;
                case Planet.PlanetType.Bad:
                    level.TimeLimit += reduceAmount;
                    break;
            }
        }

        public override void ApplySun(Level level, Sun sun)
        {
            level.TimeLimit -= reduceAmount;
        }

        public override void ApplyPowerup(Level level, PowerUp powerUp)
        {
            if (powerUp.GetType() == typeof(ReduceTimePowerUp))
            {
                level.TimeLimit -= (int)Math.Ceiling(1.5 * reduceAmount);
            }
        }

        public override void ApplyTooFar(Level level)
        {// Do nothing
        }
        
        public override void ApplyTimeOver(Level level)
        {// Do nothing   
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
