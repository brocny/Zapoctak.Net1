using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace ZapoctakProg2
{
    #region LoseConditions
    public class RequirementsNotMetException : Exception
    {
        public RequirementsNotMetException() { }
        public RequirementsNotMetException(string message) : base(message) { }


    }

    public abstract class LoseCondition
    {
        protected int minmaxNumber;

        protected LoseCondition(int minmaxNumber, int[] planetTypeCount)
        {
            this.minmaxNumber = minmaxNumber;
            this.PlanetTypeCount = planetTypeCount;
        }

        protected int[] PlanetTypeCount;

        public abstract void Check();
    }

    public class TooManyBadPlanets : LoseCondition
    {
        public TooManyBadPlanets(int maxNumber, int[] PlanetTypeCount) : base(maxNumber, PlanetTypeCount) { }
        public override void Check()
        {
            if (PlanetTypeCount[(int)Planet.PlanetType.Bad] > minmaxNumber)
                throw new RequirementsNotMetException(string.Format("Too many Bad planets - maximum of {0} required", minmaxNumber));
        }
        public override string ToString()
        {
            
            return string.Format("Maximum Bad planets: {0}", minmaxNumber);
        }
    }

    public class TooFewTotalPlanets : LoseCondition
    {
        public TooFewTotalPlanets(int minNumber, int[] PlanetTypeCount) : base(minNumber, PlanetTypeCount) { }
        public override void Check()
        {
            var planetCount = 0;
            foreach (var i in PlanetTypeCount)
                planetCount += i;
            if (planetCount < minmaxNumber)
                throw new RequirementsNotMetException(string.Format("Too few total planets - minimum of {0} required", minmaxNumber));
        }
        public override string ToString()
        {
            return string.Format("Minimum total planets: {0}", minmaxNumber);
        }
    }

    public class TooFewGoodPlanets : LoseCondition
    {
        public TooFewGoodPlanets(int minNumber, int[] PlanetTypeCount) : base(minNumber, PlanetTypeCount) { }
        public override void Check()
        {
            if (PlanetTypeCount[(int)Planet.PlanetType.Good] < minmaxNumber)
                throw new RequirementsNotMetException(string.Format("Too few Good planets - minimum of {0} required", minmaxNumber));
        }
        public override string ToString()
        {
            return string.Format("Minimum Good planets: {0}", minmaxNumber);
        }
    }
    #endregion
}
