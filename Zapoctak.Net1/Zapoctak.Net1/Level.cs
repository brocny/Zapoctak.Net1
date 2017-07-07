using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // levels are stored in a two-way linked list
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

        /// <summary>
        /// Path to file containing information about the next level
        /// </summary>
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
        /// <summary>
        /// Index using <code>Planet.PlanetType</code>
        /// </summary>
        public int[] PlanetCountByType => planetCountByType;

        private List<Func<Level, bool>> winConditions;
        public List<Func<Level, bool>> WinConditions => winConditions;

        private int timeLimit;
        public int TimeLimit
        {
            get { return timeLimit; }
            set { timeLimit = Math.Max(0, value); }
        }

        private Stopwatch stopwatch;
        public Stopwatch Stopwatch => stopwatch;

        /// <summary>
        /// Displayed after the end of the level if player lost
        /// </summary>
        private string loseMessage = "Sorry, You Lose - Failed Requirements: ";
        public string LoseMessage => loseMessage;

        /// <summary>
        /// Shown at the top - used to display level objectives and/or any additional info
        /// </summary>
        public string Description { get; internal set; } = "Level requirements: \n";

        private string[] goalDescriptions;

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

        


        /// <summary>
        /// Check whether all win conditions are met
        /// </summary>
        /// <returns>True if all win conditions are met</returns>
        public bool CheckVictory()
        {
            UpdatePlanetCountByType();
            var victory = true;
            for (var i = 0; i < winConditions.Count; i++)
            {
                var winCondition = winConditions[i];
                if (!winCondition.Invoke(this))
                {
                    victory = false;
                    loseMessage += "\n" + goalDescriptions[i];
                }
            }
            return victory;
        }

        /// <summary>
        /// Update <code>PlanetCountByType</code> to contain the number of planets of each type that are not destroyed
        /// </summary>
        private void UpdatePlanetCountByType()
        {
            planetCountByType.Initialize();

            foreach (var planet in planets.Where(p => !p.IsDestroyed))
                planetCountByType[(int)planet.Type]++;
        }

        private string ConcatDescription()
        {
            var descBuilder = new StringBuilder();
            foreach (var goalDescription in goalDescriptions)
            {
                descBuilder.AppendLine(goalDescription);
            }
            return descBuilder.ToString();
        }


        /// <summary>
        /// Load level information from <code>currentPath</code>
        /// </summary>
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
                goalDescriptions = conditions.Item2;
                nextLevelPath = inputReader.ReadNextLevelPath();
                Description += ConcatDescription() + inputReader.ReadDescription();
            }
            catch (IOException)
            {
                Description = "Error loading level: Invalid input file format!";
            }
        }
    }


    public class FormInteraction
    {
        public Timer Timer { get; private set; }
        public ScrollBar GravityScrollBar { get; private set; }
        public PictureBox PictureBox { get; private set; }
        public Label TimeLabel { get; private set; }
        public Label TextLabel { get; private set; }

        public FormInteraction(Timer timer, ScrollBar scrollBar, PictureBox pictureBox, Label timeLabel, Label textLabel)
        {
            Timer = timer;
            GravityScrollBar = scrollBar;
            PictureBox = pictureBox;
            TimeLabel = timeLabel;
            TextLabel = textLabel;
        }
    }

}