using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ZapoctakProg2
{
    public partial class Form1 : Form
    {
        /// <summary>
        /// 
        /// </summary>
        /// <returns>Level being currently played</returns>
        public Level CurrentLevel { get; private set; }

        public Form1()
        {
            InitializeComponent();
        }


        /// <summary>
        /// Initialize objects on program startup
        /// </summary>
        private void Form1_Load(object sender, EventArgs e)
        {
            var form = new FormInteraction(timer1, gravityScrollBar, pictureBox1, timeLabel);
            var powerUpLoader = PowerUpPluginLoader.Instance;
            powerUpLoader.LoadPlugins();
            CurrentLevel = new Level(form, null, Level.Firstlevelpath);
            UpdateButtons();
            CurrentLevel.Activate();
            textLabel.Text = CurrentLevel.Description;
        }


        /// <summary>
        /// Pause/Resume current level
        /// </summary>
        private void StartStopButton_Click(object sender, EventArgs e)
        {
            if (CurrentLevel.IsRunning)
            {
                CurrentLevel.Stop();
                startButton.Text = ">";
            }
            else
            {
                CurrentLevel.Start();
                startButton.Text = "| |";
            }
        }

        /// <summary>
        /// Make <code>level</code> the current level
        /// </summary>
        private void SwitchLevelTo(Level level)
        {
            CurrentLevel = level;
            level.Activate();
            textLabel.Text = CurrentLevel.Description;
            
            UpdateButtons();
        }

        int timeCheckCounter = 0;
        /// <summary>
        /// Controls ticks of the level - simulation as well as animation
        /// </summary>
        private void Timer1_Tick(object sender, EventArgs e)
        {
            CurrentLevel.Tick();
            // check every 10th tick, if the level is finished, if it is check won or lost
            if (timeCheckCounter == 9 && CurrentLevel.CheckTimeLimit())
            {
                CurrentLevel.End();
                UpdateButtons();
                textLabel.Text = CurrentLevel.CheckVictory() ? Level.WinMessage : CurrentLevel.EndLevelMessage;
            }
            timeCheckCounter = (timeCheckCounter + 1) % 10;

        }

        private void UpdateButtons()
        {
            nextButton.Enabled = !CurrentLevel.IsLastLevel;

            previousButton.Enabled = !CurrentLevel.IsFirstLevel;

            startButton.Text = CurrentLevel.IsRunning ? "| |" : ">";
            
            startButton.Enabled = !CurrentLevel.IsEnded;
        }
        /// <summary>
        /// Switch to previous level, disabled if no previous level
        /// </summary>
        private void PreviousButton_Click(object sender, EventArgs e)
        {
            CurrentLevel.Stop();
            Level previousLevel = CurrentLevel.PreviousLevel;
            SwitchLevelTo(previousLevel);
        }
        /// <summary>
        /// Switch to next level, disabled if no next level
        /// </summary>
        private void NextButton_Click(object sender, EventArgs e)
        {
            CurrentLevel.Stop();
            Level nextLevel = CurrentLevel.NextLevel;
            SwitchLevelTo(nextLevel);
        }        


        /// <summary>
        /// Update gravity whenever <code>gravityScrollBar</code> is moved
        /// </summary>
        private void GravityScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            CurrentLevel.Physics.GravityConst = gravityScrollBar.Value;
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            CurrentLevel.Reset();
            UpdateButtons();
            textLabel.Text = CurrentLevel.Description;
            CurrentLevel.Activate();
        }


        /// <summary>
        /// Enables gravity to adjusted by scrolling the mousewheel
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {
            int gravityChange = gravityScrollBar.SmallChange * e.Delta / 50;

            if (gravityScrollBar.Value + gravityChange >= gravityScrollBar.Minimum && gravityScrollBar.Value + gravityChange <= gravityScrollBar.Maximum)
                gravityScrollBar.Value += gravityChange;
            else if (gravityScrollBar.Value + gravityChange < gravityScrollBar.Minimum)
                gravityScrollBar.Value = gravityScrollBar.Minimum;
            else if (gravityScrollBar.Value + gravityChange > gravityScrollBar.Maximum)
                gravityScrollBar.Value = gravityScrollBar.Maximum;

            CurrentLevel.Physics.GravityConst = gravityScrollBar.Value;
        }
    }
}
