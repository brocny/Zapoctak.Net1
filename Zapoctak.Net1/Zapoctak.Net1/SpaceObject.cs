using System;
using System.Drawing;

namespace ZapoctakProg2
{
    public struct Coordinates
    {
        private readonly double xPos;
        private readonly double yPos;
        private readonly double xVel;
        private readonly double yVel;
        private readonly double radius;
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

    public abstract class SpaceObject
    {
        /// <summary>
        /// Reduces the amount objects move per tick
        /// </summary>
        public int SlowdownFactor { get; set; } = 20;
        /// <summary>
        /// Reduces acceleration due to gravity
        /// </summary>
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
        { }

        public virtual bool IsDestroyed { get; set; }

        
        protected double xPos, yPos;
        /// <summary>
        /// Position on the X axis (positive towards left)
        /// </summary>
        public double XPos => xPos;
        /// <summary>
        /// Position on the Y axis (positive towards bottom)
        /// </summary>
        public double YPos => yPos;

        //velocity along the x and y axes
        protected double xVel, yVel;
        public double XVel => xVel;
        public double YVel => yVel;

        //object radius - for drawing & detecting collisions
        protected double radius;
        public double Radius => radius;
        
        /// <returns>Distance to <code>toObject</code></returns>
        public double DistanceTo(SpaceObject toObject)
        {
            return Math.Sqrt((xPos - toObject.XPos) * (xPos - toObject.XPos) +
                            (yPos - toObject.YPos) * (yPos - toObject.YPos));

        }

        public abstract void Draw(Graphics graphics, double scaleFactor);

    }

    public abstract class MovingSpaceObject : SpaceObject
    {
        protected MovingSpaceObject(double xPos, double yPos, double xVel, double yVel, double radius) : base (xPos, yPos, xVel, yVel, radius)
        { }

        protected MovingSpaceObject(Coordinates coords) : base(coords) { }
        
        public bool IsImmortal { get; set; }

        private bool isDestroyed;
        public override bool IsDestroyed
        {
            get { return isDestroyed; }
            set
            {
                if (IsImmortal && value)
                    return;
                isDestroyed = value;
            }
        }

        /// <summary>
        /// Draw the <code>SpaceObject</code>
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="scaleFactor"></param>
        public abstract override void Draw(Graphics graphics, double scaleFactor);


        /// <summary>
        /// Update <code>SpaceObject</code>'s position after one tick
        /// </summary>
        public void UpdatePosition()
        {
            xPos += xVel / SlowdownFactor;
            yPos += yVel / SlowdownFactor;
        }

        /// <summary>
        /// Update <code>SpaceObject</code>'s velocity based on how much it would have accelerated towards <code>sun</code> in a single tick
        /// </summary>
        public void UpdateVelocity(Sun sun, double gravityConst)
        {
            var sunDistance = DistanceTo(sun);
            var sunDistance3 = sunDistance * sunDistance * sunDistance;

            xVel -= gravityConst * sun.Mass * (xPos - sun.XPos) / sunDistance3 / SlowdownFactor / 100;
            yVel -= gravityConst * sun.Mass * (yPos - sun.YPos) / sunDistance3 / SlowdownFactor / 100;
        }
        

        /// <returns>True if collision has occured between <code>MovingSpaceObject</code> and <code>withObject</code></returns>
        public bool HasCollidedWith(SpaceObject withObject)
        {
            return DistanceTo(withObject) < radius + withObject.Radius;
        }
        
        protected void DrawVelocityArrowFromCentre(Graphics gr, double scaleFactor, Pen pen, float arrowSize = 2)
        {
            var startPoint = new PointF((float)(xPos * scaleFactor), (float)(yPos * scaleFactor));
            var endPoint = new PointF((float)((xPos + arrowSize * xVel) * scaleFactor), (float)((yPos + arrowSize * yVel) * scaleFactor));
            gr.DrawLine(pen, startPoint, endPoint);
        }

        protected void DrawVelocityArrowFromCircumference(Graphics gr, double scaleFactor, Pen pen, float arrowSize = 2)
        {
            var factor = Math.Sqrt(xVel * xVel + yVel * yVel);
            var xDelta = radius * xVel / factor;
            var yDelta = radius * yVel / factor;
            var startPoint = new PointF(
                (float)((xPos + xDelta)*scaleFactor), 
                (float)((yPos + yDelta)*scaleFactor));

            var endPoint = new PointF(
                (float)((xPos + xDelta + arrowSize * xVel) * scaleFactor), 
                (float)((yPos + yDelta + arrowSize * yVel) * scaleFactor));
            gr.DrawLine(pen, startPoint, endPoint);
        }
    }
}
