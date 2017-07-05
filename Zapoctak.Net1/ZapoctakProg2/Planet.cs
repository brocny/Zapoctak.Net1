﻿using System;
using System.Drawing;

namespace ZapoctakProg2
{
    public class Planet : MovingSpaceObject
    {
        public enum PlanetType
        {
            Good, Neutral, Bad
        }

        public Planet(double xPos, double yPos, double xVel, double yVel, double radius, PlanetType type) : base(xPos, yPos, xVel, yVel, radius)
        {
            planetType = type;

            switch (planetType)
            {
                case PlanetType.Bad:
                    color = Color.FromArgb(255, 0, 0);
                    break;
                case PlanetType.Good:
                    color = Color.FromArgb(0, 255, 0);
                    break;
                case PlanetType.Neutral:
                    color = Color.FromArgb(255, 255, 255);
                    break;
            }

            brush = new SolidBrush(color);
            velocityPen.EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
        }
        public PlanetType planetType;

        private Brush brush;
        private Color color;

        private Pen velocityPen = new Pen(Color.White, 3f);


        public const double MinimumRadius = 1;

        //shrinks the planets radius if it is too close to the given Sun
        public void ApplyBurns(Sun sun)
        {
            var sunSurfaceDistance = DistanceTo(sun) - sun.Radius;
            if (sunSurfaceDistance < sun.Temperature / 20)
                radius = radius * (1 - sun.Temperature / 2000.0 / (sunSurfaceDistance * sunSurfaceDistance));
        }

        /// <summary>
        /// Draws the planet using the provided <code>Graphics</code> and <code>scaleFactor</code>
        /// </summary>
        private const int ArrowSize = 2;
        public override void Draw(Graphics gr, double scaleFactor)
        {
            gr.FillEllipse(brush, (float)((xPos - radius) * scaleFactor), (float)((yPos - radius) * scaleFactor),
                (float)(2 * radius * scaleFactor), (float)(2 * radius * scaleFactor));

            gr.DrawLine(velocityPen, (float)(xPos * scaleFactor), (float)(yPos * scaleFactor),
                (float)((xPos + ArrowSize * xVel) * scaleFactor), (float)((yPos + ArrowSize * yVel) * scaleFactor));
        }

    }
}