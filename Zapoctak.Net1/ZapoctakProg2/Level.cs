using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Text;
using System.Diagnostics;
using System.Linq;

namespace ZapoctakProg2
{
    public class Level
    {
        // path to directory containing levels
        public const string LevelPath = @"..\..\level\";
        public const string Firstlevelpath = LevelPath + @"level1.txt";

        public const string WinMessage = "Congratulations, You Win!";

        private GraphicsEngine graphics;
        public GraphicsEngine Graphics => graphics;

        private PhysicsEngine physics;
        public PhysicsEngine Physics => physics;

        private FormInteraction formInteraction;
        public FormInteraction Form => formInteraction;

        //levels create a two-way linked list
        private Level nextLevel;
        /// <returns>The Next Level, <code>null</code> if no next level available</returns>
        /// <remarks>Lazy creation</remarks>
        public Level NextLevel
        {
            get
            {
                if (nextLevelPath == null)
                    return null;

                return nextLevel ?? (nextLevel = new Level(formInteraction, this, nextLevelPath));
            }
        }

        private Level previousLevel;
        public Level PreviousLevel => previousLevel;




        //the file in which the next level is saved
        private string nextLevelPath;

        //the file in which the current level is saved
        private readonly string currentPath;

        private List<Sun> suns;
        public List<Sun> Suns => suns;

        private List<Planet> planets;
        public List<Planet> Planets => planets;

        private List<PowerUp> powerUps;
        public List<PowerUp> PowerUps => powerUps;

        private int[] planetCountByType = { 0, 0, 0 };
        public int[] PlanetCountByType => planetCountByType;

        private List<Func<Level, bool>> winConditions;
        public List<Func<Level, bool>> WinConditions;

        private int timeLimit;
        public int TimeLimit
        {
            get { return timeLimit; }
            set { timeLimit = Math.Max(0, value); }
        }

        private Stopwatch stopwatch;

        //displayed at the end of the level - if player lost, it tells him why
        private string endLevelMessage = "Sorry, You Lose";
        public string EndLevelMessage => endLevelMessage;

        //displayed at the start of the level - tells the player what he needs to do
        private string description;
        public string Description => description;

        public bool IsFirstLevel => previousLevel == null;
        public bool IsLastLevel => nextLevelPath == null;

        private bool isRunning = false;
        public bool IsRunning => isRunning;

        //true once the timelimit has run out
        private bool isEnded = false;
        public bool IsEnded => isEnded;

        public Level(FormInteraction formInteraction, Level previousLevel, string levelPath)
        {
            this.formInteraction = formInteraction;
            this.previousLevel = previousLevel;
            currentPath = levelPath;
            InitLevel();
        }


        //initializes the level to its state described in the input file
        private void InitLevel()
        {
            suns = new List<Sun>();
            planets = new List<Planet>();
            winConditions = new List<Func<Level, bool>>();
            stopwatch = new Stopwatch();
            planetCountByType = new[] { 0, 0, 0 };

            ReadLevelInput();
        }

        //does one complete time tick of this level
        public void Tick()
        {
            graphics.Tick();
            physics.Tick();
            UpdateTimeLabel();
        }

        //prepares this level to be played after switching to it
        public void Activate()
        {
            formInteraction.PictureBox.Image = graphics.Bmp;
            formInteraction.GravityScrollBar.Maximum = physics.MaxGravity;
            formInteraction.GravityScrollBar.Minimum = physics.MinGravity;

            formInteraction.GravityScrollBar.Value = 
                physics.GravityConst > formInteraction.GravityScrollBar.Minimum ? 
                physics.GravityConst : 
                formInteraction.GravityScrollBar.Minimum;

            formInteraction.GravityScrollBar.SmallChange = (physics.MaxGravity - physics.MinGravity) / 20;
            formInteraction.GravityScrollBar.LargeChange = (physics.MaxGravity - physics.MinGravity) / 5;

            graphics.Tick();
            UpdateTimeLabel();
        }

        //returns true iff the time limit for this level has run out
        public bool CheckTimeLimit()
        {
            return stopwatch.ElapsedMilliseconds > 1000 * timeLimit;
        }

        //updates the label displaying remaining time, formatted to seconds with a single decimal place
        public void UpdateTimeLabel()
        {
            formInteraction.TimeLabel.Text = (timeLimit - stopwatch.ElapsedMilliseconds / 1000.0).ToString("N1");
            if (timeLimit - stopwatch.ElapsedMilliseconds / 1000 <= 0)
                formInteraction.TimeLabel.Text = "0,0";
        }


        //starts this level
        public void Start()
        {
            formInteraction.Timer.Enabled = true;
            stopwatch.Start();
            isRunning = true;
        }

        //stops level running
        public void Stop()
        {
            formInteraction.Timer.Enabled = false;
            stopwatch.Stop();
            isRunning = false;
        }

        //Resets this level to its state in the input file
        public void Reset()
        {
            Stop();
            isEnded = false;
            InitLevel();
        }

        //ends the level - called when time ran out is detected 
        public void End()
        {
            Stop();
            isEnded = true;
        }

        


        //returns whether the victory requirements for this level are met
        //level is won if no lose condition is met
        public bool CheckVictory()
        {
            UpdatePlanetCountType();

            foreach (var winCondition in winConditions)
            {
                if (!winCondition.Invoke(this))
                {
                    return false;
                }
            }
            return true;
        }

        private void UpdatePlanetCountType()
        {
            planetCountByType.Initialize();

            foreach (var planet in planets.Where(p => !p.IsDestroyed))
                planetCountByType[(int)planet.Type]++;
        }


        //Loads level info from input text file
        private void ReadLevelInput()
        {
            var reader = new StreamReader(currentPath);
            var inputReader = new LevelInputReader(reader, this);
            try
            {
                planets = inputReader.ReadPlanets();
                suns = inputReader.ReadSuns();
                powerUps = inputReader.ReadPowerUps();
                physics = inputReader.ReadPhysicsEngine();
                graphics = inputReader.ReadGraphicsEngine();
                timeLimit = inputReader.ReadTimeLimit();
                var conditions = inputReader.ReadWinConditions();
                winConditions = conditions.Item1;
                description = conditions.Item2;
                nextLevelPath = inputReader.ReadNextLevelPath();
                description = description + inputReader.ReadDescription();
            }
            catch (IOException)
            {
                description = "Error loading level: Invalid input file format!";
            }
        }
    }



    public class LevelInputReader
    {
        private StreamReader reader;
        private Level level;

        

        public LevelInputReader(StreamReader reader, Level level)
        {
            this.reader = reader;
            this.level = level;
        }

        public List<Sun> ReadSuns()
        {
            var numSuns = int.Parse(ReadLine());
            var suns = new List<Sun>();

            for (var i = 0; i < numSuns; i++)
            {
                var sunLine = ReadLine();
                var s = sunLine.Split();

                suns.Add(new Sun(double.Parse(s[0]), double.Parse(s[1]), double.Parse(s[2]), double.Parse(s[3]),
                    double.Parse(s[4]), double.Parse(s[5]), double.Parse(s[6])));
            }

            return suns;
        }

        public List<Planet> ReadPlanets()
        {
            var numPlanets = int.Parse(ReadLine());
            var planets = new List<Planet>();

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

    public Coordinates ReadCoordinates()
    {
        var coordStrings = ReadLine().Split(' ');

        var coords = new Coordinates(double.Parse(coordStrings[0]), double.Parse(coordStrings[1]),
                double.Parse(coordStrings[2]),
                double.Parse(coordStrings[3]), double.Parse(coordStrings[4]));
            return coords;
    }

        public (List<Func<Level, bool>>, string) ReadWinConditions()
        {
            var numConditions = int.Parse(ReadLine());
            var winConditions = new List<Func<Level, bool>>(numConditions);

            var descriptionBuilder = new StringBuilder("Level requirements: \n");

            for (int i = 0; i < numConditions; i++)
            {
                var line = ReadLine();
                var parsedExpression = ExpressionParser.Parse(line);
                winConditions.Add(parsedExpression);
                descriptionBuilder.AppendLine(ReadLine());
            }

            return (winConditions, descriptionBuilder.ToString());
        }

        public string ReadDescription()
        {
            var descBuilder = new StringBuilder();
            while (!reader.EndOfStream)
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
                line = reader.ReadLine();
            }
            while (line?[0] == '#' && !reader.EndOfStream);

            if (line?[0] == '#') return string.Empty;

            return line;
        }

    }

    public class FormInteraction
    {
        public Timer Timer { get; private set; }
        public ScrollBar GravityScrollBar { get; private set; }
        public PictureBox PictureBox { get; private set; }
        public Label TimeLabel { get; private set; }

        public FormInteraction(Timer timer, ScrollBar scrollBar, PictureBox pictureBox, Label timeLabel)
        {
            Timer = timer;
            GravityScrollBar = scrollBar;
            PictureBox = pictureBox;
            TimeLabel = timeLabel;
        }
    }

}