using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace ZapoctakProg2
{
    public class LevelInputReader
    {
        private readonly StreamReader streamReader;
        private readonly Level level;

        public LevelInputReader(StreamReader streamReader, Level level)
        {
            this.streamReader = streamReader;
            this.level = level;
        }
        /// <summary>
        /// Reads suns
        /// Format: single line containing the number of suns
        /// Each line contains information about 1 sun in the following format: xPos yPos xVel yVel radius temperature
        /// </summary>
        public List<Sun> ReadSuns()
        {
            var numSuns = int.Parse(ReadLine());
            var suns = new List<Sun>(numSuns);

            for (var i = 0; i < numSuns; i++)
            {
                var sunLine = ReadLine();
                var s = sunLine.Split();

                suns.Add(new Sun(double.Parse(s[0]), double.Parse(s[1]), double.Parse(s[2]), double.Parse(s[3]),
                    double.Parse(s[4]), double.Parse(s[5]), double.Parse(s[6])));
            }

            return suns;
        }

        /// <summary>
        /// Reads planets
        /// Format: single line containing the number of planets
        /// Each line contains information about 1 planet in the following format: xPos yPos xVel yVel radius type
        /// </summary>
        /// <returns></returns>
        public List<Planet> ReadPlanets()
        {
            var numPlanets = int.Parse(ReadLine());
            var planets = new List<Planet>(numPlanets);

            for (var i = 0; i < numPlanets; i++)
            {
                var planetLine = ReadLine();
                var p = planetLine.Split();

                planets.Add(new Planet(double.Parse(p[0]), double.Parse(p[1]), double.Parse(p[2]), double.Parse(p[3]),
                    double.Parse(p[4]), (Planet.PlanetType)int.Parse(p[5])));
            }
            return planets;
        }

        public string ReadNextLevelPath()
        {
            var nextLevelName = ReadLine();
            string nextLevelPath = Level.LevelPath + nextLevelName;
            if (nextLevelName?.ToLower() == "null" || !File.Exists(nextLevelPath))
            {
                return null;
            }

            return nextLevelPath;
        }

        public GraphicsEngine ReadGraphicsEngine()
        {
            var scaleFactor = double.Parse(ReadLine());
            return new GraphicsEngine(level, scaleFactor);
        }

        public PhysicsEngine ReadPhysicsEngine()
        {
            var gravityConst = int.Parse(ReadLine());
            var maxDistance = double.Parse(ReadLine());

            var minGravity = int.Parse(ReadLine());
            var maxGravity = int.Parse(ReadLine());

            var physics = new PhysicsEngine(gravityConst, minGravity, maxGravity, maxDistance, level);
            return physics;
        }

        public int ReadTimeLimit()
        {
            return int.Parse(ReadLine());
        }

        public List<PowerUp> ReadPowerUps()
        {
            var numPowerUps = int.Parse(ReadLine());
            var powerUps = new List<PowerUp>();
            var parsers = PowerUpPluginLoader.Instance.Parsers;

            for (int i = 0; i < numPowerUps; i++)
            {
                var powerUpId = ReadLine();
                if (parsers.TryGetValue(powerUpId, out IPowerUpParser parser))
                {
                    powerUps.Add(parser.Parse(this));
                }
                else
                {
                    throw new FormatException($"Unknown PowerUp id {powerUpId}");
                }
            }

            return powerUps;
        }
        /// <summary>
        /// More convenient way to read <code>Coordinates</code>; fomrmat: <code>xPos yPos xVel yVel radius</code>
        /// </summary>
        public Coordinates ReadCoordinates()
        {
            var coordStrings = ReadLine().Split(' ');

            var coords = new Coordinates(
                double.Parse(coordStrings[0]), 
                double.Parse(coordStrings[1]),
                double.Parse(coordStrings[2]),
                double.Parse(coordStrings[3]), 
                double.Parse(coordStrings[4]));

            return coords;
        }

        public (List<Func<Level, bool>>, string[]) ReadWinConditions()
        {
            var numConditions = int.Parse(ReadLine());
            var winConditions = new List<Func<Level, bool>>(numConditions);

            var descriptions = new string[numConditions];

            for (int i = 0; i < numConditions; i++)
            {
                var line = ReadLine();
                var parsedExpression = ExpressionParser.Parse(line);
                winConditions.Add((Func<Level, bool>)parsedExpression.Compile());
                string desc;
                if ((desc = ReadLine().ToLowerInvariant()) == "auto")
                {
                    descriptions[i] = ExpressionParser.GenerateDescription(parsedExpression);
                }
                else
                {
                    descriptions[i] = desc;
                }
            }

            return (winConditions, descriptions);
        }

        public string ReadDescription()
        {
            var descBuilder = new StringBuilder();
            while (!streamReader.EndOfStream)
            {
                var line = ReadLine();
                if (line != null)
                    descBuilder.AppendLine(line);
            }
            return descBuilder.ToString();
        }

        public string ReadLine()
        {
            string line;
            do
            {
                line = streamReader.ReadLine();
            }
            while (line?[0] == '#' && !streamReader.EndOfStream);

            if (line?[0] == '#') return string.Empty;

            return line;
        }

    }
}