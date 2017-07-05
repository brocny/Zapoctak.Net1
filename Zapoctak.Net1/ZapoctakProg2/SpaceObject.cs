using System;
using System.Drawing;

namespace ZapoctakProg2
{
    #region SpaceObject

    public interface IDrawable
    {
        void Draw(Graphics gr, double scaleFactor);
    }

    public struct Coordinates
    {
        private double xPos, yPos, xVel, yVel, radius;
        public double XPos => xPos;
        public double YPos => yPos;
        public double XVel => xVel;
        public double YVel => yVel;
        public double Radius => radius;

        public Coordinates(double xPos, double yPos, double xVel, double yVel, double radius)
        {
            this.xPos = xPos;
            this.yPos = yPos;
            this.xVel = xVel;
            this.yVel = yVel;
            this.radius = radius;
        }
    }

    public abstract class SpaceObject : IDrawable
    {
        public const int SlowdownFactor = 20;
        public const int GravityReductionFactor = 100;

        protected SpaceObject(double xPos, double yPos, double xVel, double yVel, double radius)
        {
            this.xPos = xPos;
            this.yPos = yPos;
            this.xVel = xVel;
            this.yVel = yVel;
            this.radius = radius;
        }

        protected SpaceObject(Coordinates coords)
            : this(coords.XPos, coords.YPos, coords.XVel, coords.YVel, coords.Radius)
        {
            
        }

        public bool IsDestroyed { get; set; }


        //position on the x and y axes
        protected double xPos, yPos;
        public double XPos { get { return xPos; } }
        public double YPos { get { return yPos; } }

        //velocity along the x and y axes
        protected double xVel, yVel;
        public double XVel { get { return xVel; } }
        public double YVel { get { return yVel; } }

        //object radius - for drawing & detecting collisions
        protected double radius;
        public double Radius => radius;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="toObject"></param>
        /// <returns>Distance to <code>toObject</code></returns>
        public double DistanceTo(SpaceObject toObject)
        {
            return Math.Sqrt((xPos - toObject.XPos) * (xPos - toObject.XPos) +
                            (yPos - toObject.YPos) * (yPos - toObject.YPos));

        }
        public abstract void Draw(Graphics gr, double scaleFactor);

    }
    #endregion

    public abstract class MovingSpaceObject : SpaceObject
    {
        protected MovingSpaceObject(double xPos, double yPos, double xVel, double yVel, double radius) : base (xPos, yPos, xVel, yVel, radius)
        {
            
        }

        protected MovingSpaceObject(Coordinates coords) : base(coords) { }

        public abstract override void Draw(Graphics gr, double scaleFactor);

        public void UpdateVel(Sun sun, double gravityConst)
        {
            var sunDistance = DistanceTo(sun);
            var sunDistance3 = sunDistance * sunDistance * sunDistance;

            xVel -= gravityConst * sun.Mass * (xPos - sun.XPos) / sunDistance3 / SlowdownFactor / 100;
            yVel -= gravityConst * sun.Mass * (yPos - sun.YPos) / sunDistance3 / SlowdownFactor / 100;
        }

        public bool HasCrashedWith(SpaceObject withObject)
        {
            return DistanceTo(withObject) < radius + withObject.Radius;
        }

        public void UpdatePos()
        {
            xPos += xVel / SlowdownFactor;
            yPos += yVel / SlowdownFactor;
        }



    }
}
