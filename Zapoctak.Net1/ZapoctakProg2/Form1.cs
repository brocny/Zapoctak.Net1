using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace ZapoctakProg2
{
    #region Form1
    public partial class Form1 : Form
    {
        // level being simulated at the moment
        public Level CurrentLevel { get; private set; }

        public Form1()
        {
            InitializeComponent();
        }


        //some initialization on program startup
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


        //pauses / starts current level
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

        private void SwitchLevelTo(Level level)
        {
            CurrentLevel = level;
            level.Activate();
            textLabel.Text = CurrentLevel.Description;
            
            UpdateButtons();
        }

        int timeCheckCounter = 0;
        private void Timer1_Tick(object sender, EventArgs e)
        {
            CurrentLevel.Tick();
            // check every 10th tick, if the level is finished, if it is check won or lost
            if (timeCheckCounter == 9 && CurrentLevel.CheckTimeLimit())
            {
                CurrentLevel.End();
                UpdateButtons();
                if (CurrentLevel.CheckVictory())
                    textLabel.Text = "Congratulations, You Win!";
                else
                    textLabel.Text = CurrentLevel.EndLevelMessage;
            }
            timeCheckCounter = (timeCheckCounter + 1) % 10;

        }


        private void UpdateButtons()
        {
            nextButton.Enabled = !CurrentLevel.IsLastLevel;

            previousButton.Enabled = !CurrentLevel.IsFirstLevel;

            if (CurrentLevel.IsRunning)
            {
                startButton.Text = "| |";
            }
            else
            {
                startButton.Text = ">";
            }
            
            startButton.Enabled = !CurrentLevel.IsEnded;
        }

        private void PreviousButton_Click(object sender, EventArgs e)
        {
            CurrentLevel.Stop();
            Level previousLevel = CurrentLevel.PreviousLevel;
            SwitchLevelTo(previousLevel);
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            CurrentLevel.Stop();
            Level nextLevel = CurrentLevel.NextLevel;
            SwitchLevelTo(nextLevel);
        }        


        //updates gravity whenever the scrollbar is moved
        private void GravityScrollBar_Scroll(object sender, ScrollEventArgs e)
        {
            CurrentLevel.Gravity = gravityScrollBar.Value;
        }

        private void RestartButton_Click(object sender, EventArgs e)
        {
            CurrentLevel.Reset();
            UpdateButtons();
            textLabel.Text = CurrentLevel.Description;
            CurrentLevel.Activate();
        }


        //enables the use the mousewheel to adjust gravity
        private void Form1_MouseWheel(object sender, MouseEventArgs e)
        {

            int gravityChange = gravityScrollBar.SmallChange * e.Delta / 50;

            if (gravityScrollBar.Value + gravityChange >= gravityScrollBar.Minimum && gravityScrollBar.Value + gravityChange <= gravityScrollBar.Maximum)
                gravityScrollBar.Value += gravityChange;
            else if (gravityScrollBar.Value + gravityChange < gravityScrollBar.Minimum)
                gravityScrollBar.Value = gravityScrollBar.Minimum;
            else if (gravityScrollBar.Value + gravityChange > gravityScrollBar.Maximum)
                gravityScrollBar.Value = gravityScrollBar.Maximum;

            CurrentLevel.Gravity = gravityScrollBar.Value;
        }
    }
    #endregion

}
