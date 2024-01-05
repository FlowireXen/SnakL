using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SnakL
{
    public partial class MainWindow : Window
    {
        string GameTitle = "SnakL";
        string AppDataFolder = "SnakL";
        string AppDataFile = "Scores.snkl";
        byte BitShift = 4; //""BitShift""
        //HACK: Filestructure Hardcoded.
        string FileDate = "-- "; //Read from File

        #region Variables
        double BoardSizeX = 500.0; // Links, Rechts
        double BoardSizeY = 500.0; // Hoch, Runter
        double BoardKillMargin = 5.0; //Error Margin for the Board Border

        Action RenderAction;    //Render-Logic
        Action GameLogicAction; //Special Game-Logic

        Timer GameTime = null;
        double TimerDelay = 10.0;
        bool TimePaused = true;

        Random RNG = new Random();
        bool PerformanceTick = true; //50% Performance Increase
        bool GameOverState = false;
        int DeathDelay = 10;
        bool OptionsActive = false;
        int ColorIndex = 1; // 0 = No Change, 1 = Normal, 2 = Alternative, 3 = Gray

        double Speed = 1.5;
        double SpeedTurn = 1.5; //Only added for the Options lol
        double SpeedMult = 1.0;
        double SpeedDefaultMult = 1.0;
        double SpeedFast = 1.75;
        double SpeedSlow = 0.5;

        bool KeyLeft = false;
        bool KeyRight = false;
        bool KeySprint = false;
        bool KeySneak = false;
        bool KeyResetEachInput = false; //Only added for the Options lol

        double PlayerX = 0.0; // Links, Rechts
        double PlayerY = 0.0; // Hoch, Runter
        double PlayerHitBox = 10.0; //Hitbox
        double PlayerVisualSize = 20.0;
        double PlayerDegree = 0.0;
        RotateTransform PlayerVisualRotation = new RotateTransform() { Angle = 0.0 };
        Thickness PlayerVisualPosition = new Thickness(0, 0, 0, 0);
        SolidColorBrush PlayerBrush; // = new SolidColorBrush(new Color() { A = 255, R = 0, G = 0, B = 220 });

        double BodyLenght = 0;
        int BodyStartLenght = 50;
        int BodyGraceHead = 10; //Only added for the Options lol
        int BodyGraceTail = 4;
        Point BodyNextPoint = new Point();
        PointCollection BodyPoints = new PointCollection();

        double FoodX = 0.0; // Links, Rechts
        double FoodY = 0.0; // Hoch, Runter
        int FoodEaten = 0;
        int FoodEatenHighscore = 0;
        int FoodEatenPrevHighscore = 0;
        double FoodValue = 12.0;
        double FoodEatMargin = 5.0; //Margin x 2 = "Hitbox"
        double FoodSpawnMargin = 5.0;
        double FoodVisualSize = 20.0;
        Thickness FoodVisualPosition = new Thickness(0, 0, 0, 0);
        SolidColorBrush FoodBrush; // = new SolidColorBrush(new Color() { A = 255, R = 255, G = 69, B = 0 });

        double SneakSpike = 1.5;
        double SprintWave = 0.0;
        double SprintWaveChange = 1.6;
        double SprintWaveMax = 1.6;
        double SprintWaveMin = -1.6;

        double Stamina = 100.0;
        double StaminaMin = -5.0;
        double StaminaMax = 100.0; // 100 also helps with the Progressbar
        double StaminaRegen = 0.5;
        double StaminaRegenBroken = 0.3;
        double StaminaDrain = 1.2;
        bool StaminaBroken = false;
        SolidColorBrush StaminaBrush; // = new SolidColorBrush(new Color() { A = 255, R = 30, G = 160, B = 240 });
        SolidColorBrush StaminaBrokenBrush; // = new SolidColorBrush(new Color() { A = 255, R = 128, G = 180, B = 215 });

        double Score = 0.0;
        double ScoreHighscore = 0.0;
        double ScorePrevHighscore = 0.0;
        double ScoreValue = 1.0; //Value to Add

        int ComboCurrent = 0;
        int ComboCurrentHighscore = 0; //Most Eaten fruit in a Row
        int ComboCurrentPrevHighscore = 0;
        int ComboInitiateAfter = 2;
        double ComboMult = 1.0;
        double ComboMultHighscore = 1.0;
        double ComboMultPrevHighscore = 1.0;
        double ComboDefaultMult = 1.0;
        double ComboMultAdd = 0.5;
        int ComboAscensionAfter = 10;
        double ComboMultAddAscension = 0.2;
        SolidColorBrush[] ComboTexBrushes = // Int of "ScoreMult / 2"
        {
            new SolidColorBrush(new Color() { A = 255, R = 70, G = 180, B = 0 }),  //  1.0+ Dark Green
            new SolidColorBrush(new Color() { A = 255, R = 110, G = 220, B = 0 }), //  2.0+ Light Green
            new SolidColorBrush(new Color() { A = 255, R = 240, G = 190, B = 0 }), //  4.0+ Yellow
            new SolidColorBrush(new Color() { A = 255, R = 255, G = 160, B = 0 }), //  6.0+ Gold
            new SolidColorBrush(new Color() { A = 255, R = 255, G = 60, B = 0 }),  //  8.0+ Orange-Red
            new SolidColorBrush(new Color() { A = 255, R = 250, G = 0, B = 140 }), // 10.0+ Magenta
            new SolidColorBrush(new Color() { A = 255, R = 170, G = 0, B = 210 }), // 12.0+ Purple
            new SolidColorBrush(new Color() { A = 255, R = 0, G = 70, B = 250 }),  // 14.0+ Blue
            new SolidColorBrush(new Color() { A = 255, R = 0, G = 200, B = 250 }), // 16.0+ Cyan (Final)
        };
        double ComboTime = 100.0;
        double ComboTimeMin = -5.0;
        double ComboTimeMax = 100.0; // 100 also helps with the Progressbar
        double ComboTimeInitiate = 72.0;
        double ComboTimeDrain = 0.275;
        double ComboTimeDefaultDrain = 0.275;
        double ComboTimeDrainMin = 0.05;
        int ComboTimeSlowDownAfter = 10; //Atleast Eaten X to start slowing down
        int ComboTimeSlowDownStep = 2;  //Every "Modulo" do the Slowdown Math, also used in the SlowDownEffect
        double ComboTimeSlowDownEffect = 0.005;
        SolidColorBrush ComboTimeBrush; // = new SolidColorBrush(new Color() { A = 255, R = 240, G = 110, B = 30 });

        SolidColorBrush WindowBrush; // = new SolidColorBrush(new Color() { A = 255, R = 240, G = 248, B = 255 });
        SolidColorBrush CenterBrush; // = new SolidColorBrush(new Color() { A = 128, R = 0, G = 0, B = 0 });

        string[] StartupFlavorText = {
            "Not Snake",
            "Buy the Demo!",
            "yippee.mp4",
            "SnakL",
            "Flag is Win",
            "BLOOD IS FUEL",
            "FUBAR!",
            "AITA?",
            "Cool Game!",
            "Pizza Soup",
            "Summer Night Snow",
            "Buzz Buzz Gonna Get Ya'",
            "That's just a Triangle, Bro.",
            "Boykisser Edition",
            "Jester Edition",
            "Maxwell Edition",
            @"\n",
            "ᓚᘏᗢ",
            "???",
            "Flowire"
        };
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            //Set Culture-Info: Changes "1,25" to "1.25"
            System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

            //Initial Setup for Timer
            GameTime = new Timer();
            GameTime.AutoReset = true;
            GameTime.Elapsed += GameTime_Tick;

            //Initial Setup for Player
            Body.Points = BodyPoints;
            PlayerVisualRotation = new RotateTransform() { Angle = PlayerDegree };
            PlayerModel.LayoutTransform = PlayerVisualRotation;

            //Score Paths
            AppDataFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppDataFolder);
            AppDataFile = Path.Combine(AppDataFolder, AppDataFile);

            //Create the Render-Logic
            RenderAction = new Action(() =>
            {
                PlayerVisualRotation.Angle = PlayerDegree;

                PlayerVisualPosition.Left = PlayerX - (PlayerVisualSize + (PlayerVisualSize / 2) - PlayerHitBox) / 2;
                PlayerVisualPosition.Top = PlayerY - (PlayerVisualSize + (PlayerVisualSize / 2) - PlayerHitBox) / 2;
                PlayerModelBounds.Margin = PlayerVisualPosition;

                FoodVisualPosition.Left = FoodX;
                FoodVisualPosition.Top = FoodY;
                Food.Margin = FoodVisualPosition;

                Coordinates.Content =
                "X:" + PlayerX.ToString("000") + "  " +
                "Y:" + PlayerY.ToString("000") + "  " +
                "R:" + PlayerDegree.ToString("000");

                ScoreLabel.Content = "Score: " + Score.ToString("000") + " | " + FoodEaten.ToString("000");

                StaminaBar.Value = Stamina;
                if (StaminaBroken)
                    StaminaBar.Foreground = StaminaBrokenBrush;
                else StaminaBar.Foreground = StaminaBrush;

                if (ComboMult > ComboDefaultMult)
                {
                    CenterLabel.Content = "Combo x" + ComboMult.ToString("0.0");
                    int ColorIndex = (int)(ComboMult / 2);
                    if (ColorIndex < ComboTexBrushes.Length)
                        CenterLabel.Foreground = ComboTexBrushes[ColorIndex];
                    ComboBar.Visibility = Visibility.Visible;
                    ComboBar.Value = ComboTime;
                }
                else
                {
                    CenterLabel.Content = GameTitle;
                    CenterLabel.Foreground = CenterBrush;
                    ComboBar.Visibility = Visibility.Collapsed;
                }
            });

            //Create the specialized Game-Logic
            GameLogicAction = new Action(() =>
            {
                if (PerformanceTick)
                {
                    BodyPoints.Add(BodyNextPoint);
                    if (BodyPoints.Count >= BodyLenght)
                        BodyPoints.RemoveAt(0);
                }
                PerformanceTick = !PerformanceTick;

                // This can also work somewhere else, IF, you keep track of the points in a Seperate List...
                // Doesn't really increase Performance tho
                bool LostGrace = false;
                for (int Index = BodyPoints.Count - 1 - BodyGraceHead; Index >= BodyGraceTail; Index--)
                {
                    if (BodyPoints[Index].X > PlayerX && BodyPoints[Index].X < PlayerX + PlayerHitBox)
                    {
                        if (BodyPoints[Index].Y > PlayerY && BodyPoints[Index].Y < PlayerY + PlayerHitBox)
                        {
                            if (LostGrace)
                            {
                                GameOverState = true;
                                return;
                            }
                            LostGrace = true;
                        }
                    }
                }
            });

            //Get Previous Scores
            LoadScore();

            //Random 1 in 3 Green Mode owo
            if (RNG.Next(0, 3) == 0)
                ColorIndex = 2;

            //Setup the Game Boiii
            GameSetup(true);
        }

        private void GameSetup(bool InitialSetup = false)
        {
            if (GameTime != null)
                GameTime.Interval = TimerDelay;
            PerformanceTick = true;

            ApplyColors();

            Board.Width = BoardSizeX;
            Board.Height = BoardSizeY;

            PlayerModelBounds.Width = PlayerVisualSize + (PlayerVisualSize / 2);
            PlayerModelBounds.Height = PlayerVisualSize + (PlayerVisualSize / 2);

            Food.Width = FoodVisualSize;
            Food.Height = FoodVisualSize;

            PlayerDegree = 0.0;
            FoodEaten = 0;
            Score = 0.0;
            ComboCurrent = 0;

            StaminaBroken = false;
            StaminaBar.Maximum = StaminaMax;
            Stamina = StaminaMax;

            BodyLenght = BodyStartLenght;

            ComboBar.Maximum = ComboTimeMax;
            ComboTime = ComboTimeMax;
            ComboMult = ComboDefaultMult;
            ComboTimeDrain = ComboTimeDefaultDrain;

            PlayerX = (BoardSizeX / 2) - (PlayerHitBox / 2);
            PlayerY = (BoardSizeY / 2) - (PlayerHitBox / 2) + PlayerVisualSize * 2;

            BodyNextPoint.X = PlayerX + PlayerHitBox / 2;
            BodyNextPoint.Y = PlayerY + PlayerHitBox / 2;
            BodyPoints.Clear();

            FoodX = (BoardSizeX / 2) - (FoodVisualSize / 2);
            FoodY = (BoardSizeY / 2) - (FoodVisualSize / 2) - FoodVisualSize * 2;

            PlayerModel.Points.Clear();
            PlayerModel.Points = new PointCollection()
            {
                new Point(1, PlayerVisualSize * 0.9), //Unten Links
                new Point(PlayerVisualSize / 2, 0), //Oben Mitte
                new Point(PlayerVisualSize, PlayerVisualSize * 0.9) //Unten Rechts
            };

            RenderAction.Invoke();

            if (InitialSetup)
            {
                TopLabel.Content = GameTitle;
                CenterLabel.Content = "";
                BoardOverlay.Visibility = Visibility.Visible;

                string Previous =
                    "Food:" + FoodEatenPrevHighscore.ToString("000") +
                    "  Score:" + ScorePrevHighscore.ToString("000") +
                    "  Combo:" + ComboMultPrevHighscore.ToString("0.0") +
                    "  " + ComboCurrentPrevHighscore;
                int PreviousWhiteSpace = Previous.Length - FileDate.Length - 8; // 8 = "Previous"
                string DetailText = "Highscores:" + Environment.NewLine + Environment.NewLine + "Previous";
                for (int Y = PreviousWhiteSpace; Y > 0; Y--)
                    DetailText += " ";
                DetailText += FileDate + Environment.NewLine + Previous
                    + Environment.NewLine + Environment.NewLine + Environment.NewLine + "Start with \"Enter\""
                    + Environment.NewLine + "Settings with \"F1\"" + Environment.NewLine + Environment.NewLine +
                    "Controls:" + Environment.NewLine + "▲/W ◀/A ▼/S ▶/D";
                BottomText.Text = DetailText;

                FlavorTitle();
            }
        }

        private void ApplyColors()
        {
            switch (ColorIndex)
            {
                case 1: //Deep Blue (R = 0, G = 0, B = 255)
                    //Strong Colors
                    PlayerBrush = new SolidColorBrush(new Color() { A = 255, R = 0, G = 0, B = 220 }); //Blue
                    FoodBrush = new SolidColorBrush(new Color() { A = 255, R = 255, G = 69, B = 0 }); //Orange-Red
                    StaminaBrush = new SolidColorBrush(new Color() { A = 255, R = 30, G = 160, B = 240 }); //Light Blue
                    ComboTimeBrush = new SolidColorBrush(new Color() { A = 255, R = 240, G = 110, B = 30 }); //Deep Orange
                    //Weak Colors
                    StaminaBrokenBrush = new SolidColorBrush(new Color() { A = 255, R = 128, G = 180, B = 215 }); //Lighter Blue
                    CenterBrush = new SolidColorBrush(new Color() { A = 128, R = 0, G = 0, B = 0 }); //50% Alpha Black
                    WindowBrush = new SolidColorBrush(new Color() { A = 255, R = 240, G = 248, B = 255 }); //White-Blue
                    break;

                case 2: //Lettuce Green (R = 160, G = 210, B = 120)
                    //Strong Colors
                    PlayerBrush = new SolidColorBrush(new Color() { A = 255, R = 0, G = 128, B = 0 }); //Dark Deep Green
                    FoodBrush = new SolidColorBrush(new Color() { A = 255, R = 155, G = 40, B = 250 }); //Purple
                    StaminaBrush = new SolidColorBrush(new Color() { A = 255, R = 155, G = 250, B = 80 }); //Saturated Lettuce-Green
                    ComboTimeBrush = new SolidColorBrush(new Color() { A = 255, R = 240, G = 90, B = 90 }); //Brick Red
                    //Weak Colors
                    StaminaBrokenBrush = new SolidColorBrush(new Color() { A = 255, R = 120, G = 210, B = 140 }); //Melon-Green
                    CenterBrush = new SolidColorBrush(new Color() { A = 192, R = 0, G = 0, B = 0 }); //75% Alpha Black
                    WindowBrush = new SolidColorBrush(new Color() { A = 255, R = 235, G = 248, B = 228 }); //Lettuce Background
                    break;

                case 3: //Gray (R = 128, G = 128, B = 128)
                    //Strong Colors
                    PlayerBrush = new SolidColorBrush(new Color() { A = 255, R = 0, G = 0, B = 0 }); //Black
                    FoodBrush = new SolidColorBrush(new Color() { A = 255, R = 0, G = 0, B = 0 }); //Black
                    StaminaBrush = new SolidColorBrush(new Color() { A = 255, R = 96, G = 96, B = 96 }); //Dark Gray
                    ComboTimeBrush = new SolidColorBrush(new Color() { A = 255, R = 0, G = 0, B = 0 }); //Black
                    //Weak Colors
                    StaminaBrokenBrush = new SolidColorBrush(new Color() { A = 255, R = 120, G = 120, B = 120 }); //Darkish Gray
                    CenterBrush = new SolidColorBrush(new Color() { A = 255, R = 128, G = 128, B = 128 }); //Pure Gray
                    WindowBrush = new SolidColorBrush(new Color() { A = 255, R = 240, G = 240, B = 240 }); //Light Silver
                    break;

                default: return; //Do Nothing.
            }

            Background = WindowBrush;
            Food.Stroke = FoodBrush;
            CenterLabel.Foreground = CenterBrush;
            Body.Stroke = PlayerBrush;
            PlayerModel.Stroke = PlayerBrush;
            PlayerModel.Fill = PlayerBrush;
            if (StaminaBroken)
                StaminaBar.Foreground = StaminaBrokenBrush;
            else StaminaBar.Foreground = StaminaBrush;
            ComboBar.Foreground = ComboTimeBrush;

            ColorIndex = 0;
        }

        private void GameTime_Tick(object sender, EventArgs e)
        {
            Movement();
            Eat();

            if (ComboCurrent > 0)
                if (ComboTime <= ComboTimeMin)
                {
                    ComboCurrent = 0;
                    ComboMult = ComboDefaultMult;
                }
                else ComboTime -= ComboTimeDrain;

            Dispatcher.BeginInvoke(RenderAction);
            Dispatcher.BeginInvoke(GameLogicAction);

            if (KeyResetEachInput) //Special Case for a Custom Option
            {
                KeyLeft = false;
                KeyRight = false;
            }

            if (GameOverState) //Trash but Works   /shrug
                Dispatcher.BeginInvoke((Action)(() => { GameOver(); }));
        }

        private void Movement()
        {
            if (KeyLeft)
                PlayerDegree -= SpeedTurn;
            if (KeyRight)
                PlayerDegree += SpeedTurn;

            if (PlayerDegree >= 360.0)
                PlayerDegree -= 360.0;
            if (PlayerDegree < 0.0)
                PlayerDegree += 360.0;

            double radians = PlayerDegree * Math.PI / 180;
            double sine = Math.Sin(radians);
            if (sine == double.NaN)
                sine = 0;
            double cose = Math.Cos(radians);
            if (cose == double.NaN)
                cose = 0;

            PlayerX += Speed * sine * SpeedMult;
            PlayerY -= Speed * cose * SpeedMult;

            BodyNextPoint.X = PlayerX + PlayerHitBox / 2;
            BodyNextPoint.Y = PlayerY + PlayerHitBox / 2;

            if (PerformanceTick)
            {
                if (KeySneak && !StaminaBroken)
                {
                    SneakSpike *= -1;
                    BodyNextPoint.X += SneakSpike * cose;
                    BodyNextPoint.Y += SneakSpike * sine;
                    if (Stamina >= StaminaMin)
                    {
                        Stamina -= StaminaDrain;
                        if (Stamina <= StaminaMin)
                        {
                            StaminaBroken = true;
                            MovementMultCheck();
                        }
                    }
                }
                else if (KeySprint)
                {
                    SprintWave += SprintWaveChange;
                    if (SprintWave >= SprintWaveMax || SprintWave <= SprintWaveMin)
                        SprintWaveChange *= -1;
                    BodyNextPoint.Y += SprintWave * sine * SpeedMult;
                    BodyNextPoint.X += SprintWave * cose * SpeedMult;
                }

                if (StaminaBroken)
                    Stamina += StaminaRegenBroken;
                else if (!KeySneak && Stamina < StaminaMax)
                    Stamina += StaminaRegen;
                if (StaminaBroken && Stamina >= StaminaMax)
                {
                    StaminaBroken = false;
                    MovementMultCheck();
                }
            }

            if (PlayerX < 0 - BoardKillMargin || PlayerX + PlayerHitBox > BoardSizeX + BoardKillMargin ||
                PlayerY < 0 - BoardKillMargin || PlayerY + PlayerHitBox > BoardSizeY + BoardKillMargin)
                GameOverState = true;
        }

        private void Eat()
        {
            if (FoodX <= PlayerX + FoodEatMargin && FoodX >= PlayerX - PlayerHitBox - FoodEatMargin)
                if (FoodY <= PlayerY + FoodEatMargin && FoodY >= PlayerY - PlayerHitBox - FoodEatMargin)
                {
                    FoodEaten++;
                    if (FoodEaten > FoodEatenHighscore)
                        FoodEatenHighscore = FoodEaten;
                    BodyLenght += FoodValue;

                    FoodX = (BoardSizeX - (PlayerVisualSize + FoodSpawnMargin) * 2) * RNG.NextDouble() + PlayerVisualSize + FoodSpawnMargin;
                    FoodY = (BoardSizeY - (PlayerVisualSize + FoodSpawnMargin) * 2) * RNG.NextDouble() + PlayerVisualSize + FoodSpawnMargin;

                    ComboCurrent++;
                    if (ComboCurrent > ComboCurrentHighscore)
                        ComboCurrentHighscore = ComboCurrent;
                    Score += ScoreValue * ComboMult;
                    if (Score > ScoreHighscore)
                        ScoreHighscore = Score;
                    if (ComboCurrent > ComboInitiateAfter)
                    {
                        ComboTime = ComboTimeMax;
                        if (ComboCurrent > ComboAscensionAfter)
                            ComboMult += ComboMultAddAscension;
                        else ComboMult += ComboMultAdd;
                    }
                    else ComboTime = ComboTimeInitiate;

                    if (ComboMult > ComboMultHighscore)
                        ComboMultHighscore = ComboMult;

                    if (ComboTimeDrain > ComboTimeDrainMin && FoodEaten > ComboTimeSlowDownAfter && FoodEaten % ComboTimeSlowDownStep == 0)
                    {
                        ComboTimeDrain = ComboTimeDefaultDrain - ((FoodEaten - ComboTimeSlowDownAfter) / ComboTimeSlowDownStep * ComboTimeSlowDownEffect);
                        if (ComboTimeDrainMin >= ComboTimeDrain)
                            ComboTimeDrain = ComboTimeDrainMin;
                    }
                }
        }

        private void GameOver()
        {
            GameTime.Stop();
            Task.Delay((int)(TimerDelay + DeathDelay)).Wait();
            TimePaused = true;

            //FlavorTitle();
            SetTitle("Game Over");
            TopLabel.Content = "Game Over!";
            CenterLabel.Content = " ";

            string CurrentDate = DateTime.Today.ToString("yyyyMMdd");
            string Current =
                "Food:" + FoodEatenHighscore.ToString("000") +
                "  Score:" + ScoreHighscore.ToString("000") +
                "  Combo:" + ComboMultHighscore.ToString("0.0") +
                "  " + ComboCurrentHighscore;
            int CurrentWhiteSpace = Current.Length - CurrentDate.Length - 7; // 7 = "Current"

            string Previous =
                "Food:" + FoodEatenPrevHighscore.ToString("000") +
                "  Score:" + ScorePrevHighscore.ToString("000") +
                "  Combo:" + ComboMultPrevHighscore.ToString("0.0") +
                "  " + ComboCurrentPrevHighscore;
            int PreviousWhiteSpace = Previous.Length - FileDate.Length - 8; // 8 = "Previous"

            bool NewBest =
                FoodEatenHighscore > FoodEatenPrevHighscore ||
                ScoreHighscore > ScorePrevHighscore ||
                ComboMultHighscore > ComboMultPrevHighscore ||
                ComboCurrentHighscore > ComboCurrentPrevHighscore;

            string DetailText = "Highscores";
            if (NewBest) DetailText += " (New Best)";
            DetailText += ":" + Environment.NewLine + Environment.NewLine + "Current";
            for (int X = CurrentWhiteSpace; X > 0; X--)
                DetailText += " ";
            DetailText += CurrentDate + Environment.NewLine + Current +
                Environment.NewLine + Environment.NewLine + "Previous";
            for (int Y = PreviousWhiteSpace; Y > 0; Y--)
                DetailText += " ";
            DetailText += FileDate + Environment.NewLine + Previous
                + Environment.NewLine + Environment.NewLine;
            DetailText += "Restart with \"Enter\""
                + Environment.NewLine + "Settings with \"F1\"";

            BottomText.Text = DetailText;
            BoardOverlay.Visibility = Visibility.Visible;

            SaveScore(); //Diese Reihenfolge ist Wichtig!

            FoodEatenHighscore = 0;
            ScoreHighscore = 0.0;
            ComboMultHighscore = 0.0;
            ComboCurrentHighscore = 0;
        }

        private void SaveScore()
        {
            bool NewHigh = false;
            string HighScores = DateTime.Today.ToString("yyyyMMdd") + "_";

            if (FoodEatenHighscore > FoodEatenPrevHighscore)
            {
                FoodEatenPrevHighscore = FoodEatenHighscore;
                NewHigh = true;
            }
            HighScores += FoodEatenPrevHighscore + "_";

            if (ScoreHighscore > ScorePrevHighscore)
            {
                ScorePrevHighscore = ScoreHighscore;
                NewHigh = true;
            }
            HighScores += ScorePrevHighscore.ToString("0.00") + "_";

            if (ComboMultHighscore > ComboMultPrevHighscore)
            {
                ComboMultPrevHighscore = ComboMultHighscore;
                NewHigh = true;
            }
            HighScores += ComboMultPrevHighscore.ToString("0.00") + "_";

            if (ComboCurrentHighscore > ComboCurrentPrevHighscore)
            {
                ComboCurrentPrevHighscore = ComboCurrentHighscore;
                NewHigh = true;
            }
            HighScores += ComboCurrentPrevHighscore;

            if (NewHigh)
                try
                {
                    if (!Directory.Exists(AppDataFolder))
                        Directory.CreateDirectory(AppDataFolder);
                    using (var sw = new StreamWriter(AppDataFile, false))
                        foreach (char Char in HighScores)
                            sw.Write((int)(Char + BitShift) + " ");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error Saving Score-File:", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
        }

        private void LoadScore()
        {
            if (!File.Exists(AppDataFile))
                return;

            string ScoreFileContent = "";
            try
            {
                using (StreamReader streamReader = new StreamReader(AppDataFile))
                    ScoreFileContent = streamReader.ReadToEnd();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Reading Score-File:", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            string MixedString = "";
            foreach (string FilePart in ScoreFileContent.Split(' '))
            {
                bool success = int.TryParse(FilePart, out int CharByte);
                if (success)
                    MixedString += (char)(CharByte - BitShift);
            }

            string[] MixSplit = MixedString.Split('_');
            if (MixSplit.Length == 5)
            {
                int Hardcoded = 0;
                bool Success;
                int OutInt;
                double OutDouble;
                foreach (string MixPart in MixSplit)
                {
                    switch (Hardcoded)
                    {
                        case 0:
                            FileDate = MixPart;
                            break;
                        case 1:
                            Success = int.TryParse(MixPart, out OutInt);
                            if (Success)
                                FoodEatenPrevHighscore = OutInt;
                            break;
                        case 2:
                            Success = double.TryParse(MixPart, out OutDouble);
                            if (Success)
                                ScorePrevHighscore = OutDouble;
                            break;
                        case 3:
                            Success = double.TryParse(MixPart, out OutDouble);
                            if (Success)
                                ComboMultPrevHighscore = OutDouble;
                            break;
                        case 4:
                            Success = int.TryParse(MixPart, out OutInt);
                            if (Success)
                                ComboCurrentPrevHighscore = OutInt;
                            break;
                        default: break;
                    }
                    Success = false;
                    Hardcoded++;
                }
            }
            else return; //Invalid File.
        }

        private void MovementMultCheck()
        {
            if (KeySneak && !StaminaBroken)
                SpeedMult = SpeedSlow;
            else if (KeySprint)
                SpeedMult = SpeedFast;
            else SpeedMult = SpeedDefaultMult;
        }

        private void Pause()
        {
            if (OptionsActive)
                return;
            TimePaused = !TimePaused;
            if (TimePaused)
            {
                GameTime.Stop();
                SetTitle("Paused");
            }
            else
            {
                if (GameOverState)
                {
                    GameOverState = false;
                    GameSetup();
                }
                BoardOverlay.Visibility = Visibility.Collapsed;
                SetTitle();
                GameTime.Start();
            }
        }

        private void SetTitle(string TitleText = null)
        {
            if (string.IsNullOrWhiteSpace(TitleText)) Title = GameTitle;
            else Title = GameTitle + " - " + TitleText;
        }

        private void FlavorTitle() => SetTitle(StartupFlavorText[RNG.Next(0, StartupFlavorText.Length)]); //Funny Title.png

        private void KeyHandler(Key Key, bool StateDown)
        {
            switch (Key)
            {
                case Key.A:
                case Key.Left:
                case Key.NumPad4:
                    KeyLeft = StateDown;
                    break;

                case Key.D:
                case Key.Right:
                case Key.NumPad6:
                    KeyRight = StateDown;
                    break;

                case Key.W:
                case Key.Up:
                case Key.NumPad8:
                case Key.LeftShift:
                case Key.RightShift:
                    KeySprint = StateDown;
                    MovementMultCheck();
                    break;

                case Key.S:
                case Key.Down:
                case Key.NumPad2:
                case Key.LeftCtrl:
                case Key.RightCtrl:
                    KeySneak = StateDown;
                    MovementMultCheck();
                    break;

                case Key.P:
                case Key.Enter:
                case Key.Space:
                    if (StateDown)
                        Pause();
                    break;

                case Key.F1:
                    if (StateDown && TimePaused)
                        if (OptionsActive)
                            OptionsClose();
                        else OptionsOpen();
                    break;
            }
        }

        private void Window_KeyPress(object sender, KeyEventArgs e)
        {
            if (!e.IsRepeat)
                KeyHandler(e.Key, e.IsDown);
        }

        private void Window_Closing(object sender, object e)
        {
            if (GameTime != null)
                GameTime.Stop();
            Task.Delay((int)TimerDelay + 10).Wait();
        }

        private void OptionsOpen()
        {
            OptionsActive = true;

            SetTitle("Options");
            OptionsView.Visibility = Visibility.Visible;
            ScoreLabel.Content = "Save and Quit Options with \"F1\"!";
            Coordinates.Content = "";

            int VisualCounter = 0;
            string MaxOptions = "/40";

            OptionsGrid.Children.Add(new OptionsHelper("Open Save-Folder?", "--", "0", "\"1\"=True, \"0\"=False") { Tag = 99 });
            OptionsGrid.Children.Add(new OptionsHelper("Color Scheme", ++VisualCounter + MaxOptions, ColorIndex.ToString("0"), "\"0\"=No Change, \"1\"=Blue, \"2\"=Green, \"3\"=Gray") { Tag = 40 });
            OptionsGrid.Children.Add(new OptionsHelper("Board Width", ++VisualCounter + MaxOptions, BoardSizeX.ToString("0.000"), null) { Tag = 1 });
            OptionsGrid.Children.Add(new OptionsHelper("Board Height", ++VisualCounter + MaxOptions, BoardSizeY.ToString("0.000"), null) { Tag = 2 });
            OptionsGrid.Children.Add(new OptionsHelper("Border Kill-Margin", ++VisualCounter + MaxOptions, BoardKillMargin.ToString("0.000"), null) { Tag = 3 });
            OptionsGrid.Children.Add(new OptionsHelper("Game-Tick every...", ++VisualCounter + MaxOptions, TimerDelay.ToString("0.000"), "Milliseconds") { Tag = 4 });
            OptionsGrid.Children.Add(new OptionsHelper("Death-Freeze", ++VisualCounter + MaxOptions, DeathDelay.ToString("0"), "Milliseconds") { Tag = 5 });
            OptionsGrid.Children.Add(new OptionsHelper("Don't repeat Input?", ++VisualCounter + MaxOptions, KeyResetEachInput ? "1" : "0", "\"1\"=True, \"0\"=False") { Tag = 11 });
            OptionsGrid.Children.Add(new OptionsHelper("Speed", ++VisualCounter + MaxOptions, Speed.ToString("0.000"), null) { Tag = 6 });
            OptionsGrid.Children.Add(new OptionsHelper("Turn Speed", ++VisualCounter + MaxOptions, SpeedTurn.ToString("0.000"), null) { Tag = 7 });
            OptionsGrid.Children.Add(new OptionsHelper("Default Speed Mult.", ++VisualCounter + MaxOptions, SpeedDefaultMult.ToString("0.000"), null) { Tag = 8 });
            OptionsGrid.Children.Add(new OptionsHelper("Fast Speed Mult.", ++VisualCounter + MaxOptions, SpeedFast.ToString("0.00"), null) { Tag = 9 });
            OptionsGrid.Children.Add(new OptionsHelper("Slow Speed Mult.", ++VisualCounter + MaxOptions, SpeedSlow.ToString("0.000"), null) { Tag = 10 });
            OptionsGrid.Children.Add(new OptionsHelper("Player Hitbox", ++VisualCounter + MaxOptions, PlayerHitBox.ToString("0.000"), null) { Tag = 12 });
            OptionsGrid.Children.Add(new OptionsHelper("Player Visual Size", ++VisualCounter + MaxOptions, PlayerVisualSize.ToString("0.000"), null) { Tag = 13 });
            OptionsGrid.Children.Add(new OptionsHelper("Start Lenght", ++VisualCounter + MaxOptions, BodyStartLenght.ToString("0"), null) { Tag = 14 });
            OptionsGrid.Children.Add(new OptionsHelper("Collission Start", ++VisualCounter + MaxOptions, BodyGraceHead.ToString("0"), "Body-Segments") { Tag = 15 });
            OptionsGrid.Children.Add(new OptionsHelper("Collission End", ++VisualCounter + MaxOptions, BodyGraceTail.ToString("0"), "Body-Segments") { Tag = 16 });
            OptionsGrid.Children.Add(new OptionsHelper("Food Value", ++VisualCounter + MaxOptions, FoodValue.ToString("0.000"), "Adds X Body-Segments") { Tag = 17 });
            OptionsGrid.Children.Add(new OptionsHelper("Eat Margin", ++VisualCounter + MaxOptions, FoodEatMargin.ToString("0.000"), "Margin*2 = \"Food Hitbox\"") { Tag = 18 });
            OptionsGrid.Children.Add(new OptionsHelper("Food Spawn Margin", ++VisualCounter + MaxOptions, FoodSpawnMargin.ToString("0.000"), null) { Tag = 19 });
            OptionsGrid.Children.Add(new OptionsHelper("Food Visual Size", ++VisualCounter + MaxOptions, FoodVisualSize.ToString("0.000"), null) { Tag = 20 });
            OptionsGrid.Children.Add(new OptionsHelper("Stamina Min.", ++VisualCounter + MaxOptions, StaminaMin.ToString("0.000"), null) { Tag = 21 });
            OptionsGrid.Children.Add(new OptionsHelper("Stamina Max.", ++VisualCounter + MaxOptions, StaminaMax.ToString("0.000"), null) { Tag = 22 });
            OptionsGrid.Children.Add(new OptionsHelper("Stamina Regen", ++VisualCounter + MaxOptions, StaminaRegen.ToString("0.000"), null) { Tag = 23 });
            OptionsGrid.Children.Add(new OptionsHelper("Stamina Broken Regen", ++VisualCounter + MaxOptions, StaminaRegenBroken.ToString("0.000"), null) { Tag = 24 });
            OptionsGrid.Children.Add(new OptionsHelper("Stamina Drain", ++VisualCounter + MaxOptions, StaminaDrain.ToString("0.000"), null) { Tag = 25 });
            OptionsGrid.Children.Add(new OptionsHelper("Score Value", ++VisualCounter + MaxOptions, ScoreValue.ToString("0.000"), "Per Food") { Tag = 26 });
            OptionsGrid.Children.Add(new OptionsHelper("Initiate Combo after...", ++VisualCounter + MaxOptions, ComboInitiateAfter.ToString("0"), null) { Tag = 27 });
            OptionsGrid.Children.Add(new OptionsHelper("Combo Default Mult.", ++VisualCounter + MaxOptions, ComboDefaultMult.ToString("0.000"), null) { Tag = 28 });
            OptionsGrid.Children.Add(new OptionsHelper("Add Combo Mult.", ++VisualCounter + MaxOptions, ComboMultAdd.ToString("0.000"), null) { Tag = 29 });
            OptionsGrid.Children.Add(new OptionsHelper("Combo Ascension after...", ++VisualCounter + MaxOptions, ComboAscensionAfter.ToString("0"), null) { Tag = 30 });
            OptionsGrid.Children.Add(new OptionsHelper("Add Ascension Mult.", ++VisualCounter + MaxOptions, ComboMultAddAscension.ToString("0.000"), null) { Tag = 31 });
            OptionsGrid.Children.Add(new OptionsHelper("Combo-Time Min.", ++VisualCounter + MaxOptions, ComboTimeMin.ToString("0.000"), null) { Tag = 32 });
            OptionsGrid.Children.Add(new OptionsHelper("Combo-Time Max.", ++VisualCounter + MaxOptions, ComboTimeMax.ToString("0.000"), null) { Tag = 33 });
            OptionsGrid.Children.Add(new OptionsHelper("Post Combo Combo-Time", ++VisualCounter + MaxOptions, ComboTimeInitiate.ToString("0.000"), null) { Tag = 34 });
            OptionsGrid.Children.Add(new OptionsHelper("Combo-Time Drain", ++VisualCounter + MaxOptions, ComboTimeDefaultDrain.ToString("0.000"), null) { Tag = 35 });
            OptionsGrid.Children.Add(new OptionsHelper("Combo-Time Min. Drain", ++VisualCounter + MaxOptions, ComboTimeDrainMin.ToString("0.000"), null) { Tag = 36 });
            OptionsGrid.Children.Add(new OptionsHelper("Slow-Down after...", ++VisualCounter + MaxOptions, ComboTimeSlowDownAfter.ToString("0"), "Slows down Combo-Time after X Food") { Tag = 37 });
            OptionsGrid.Children.Add(new OptionsHelper("Slow-Down every...*", ++VisualCounter + MaxOptions, ComboTimeSlowDownStep.ToString("0"), "*Editing may cause Side-Effects") { Tag = 38 });
            OptionsGrid.Children.Add(new OptionsHelper("Slow-Down Value", ++VisualCounter + MaxOptions, ComboTimeSlowDownEffect.ToString("0.000"), null) { Tag = 39 });
        }

        private void OptionsClose()
        {
            try
            {
                OptionsHelper OpHelp;
                foreach (Control cntrl in OptionsGrid.Children)
                {
                    OpHelp = cntrl as OptionsHelper;
                    switch (OpHelp.Tag)
                    {
                        case 1:
                            BoardSizeX = OpHelp.GetDouble();
                            if (GameOverState) break;
                            Board.Width = BoardSizeX;
                            break;
                        case 2:
                            BoardSizeY = OpHelp.GetDouble();
                            if (GameOverState) break;
                            Board.Height = BoardSizeY;
                            break;
                        case 3: BoardKillMargin = OpHelp.GetDouble(); break;
                        case 4:
                            TimerDelay = OpHelp.GetDouble();
                            if (GameOverState) break;
                            if (GameTime != null)
                                GameTime.Interval = TimerDelay;
                            break;
                        case 5: DeathDelay = OpHelp.GetInt(); break;
                        case 6: Speed = OpHelp.GetDouble(); break;
                        case 7: SpeedTurn = OpHelp.GetDouble(); break;
                        case 8: SpeedDefaultMult = OpHelp.GetDouble(); break;
                        case 9: SpeedFast = OpHelp.GetDouble(); break;
                        case 10: SpeedSlow = OpHelp.GetDouble(); break;
                        case 11: KeyResetEachInput = OpHelp.GetInt() == 1 ? true : false; break;
                        case 12: PlayerHitBox = OpHelp.GetDouble(); break;
                        case 13:
                            PlayerVisualSize = OpHelp.GetDouble();
                            if (GameOverState) break;
                            PlayerModelBounds.Width = PlayerVisualSize + (PlayerVisualSize / 2);
                            PlayerModelBounds.Height = PlayerVisualSize + (PlayerVisualSize / 2);
                            PlayerModel.Points.Clear();
                            PlayerModel.Points = new PointCollection()
                            {
                                new Point(1, PlayerVisualSize * 0.9), //Unten Links
                                new Point(PlayerVisualSize / 2, 0), //Oben Mitte
                                new Point(PlayerVisualSize, PlayerVisualSize * 0.9) //Unten Rechts
                            };
                            break;
                        case 14:
                            BodyStartLenght = OpHelp.GetInt();
                            if (GameOverState) break;
                            if (BodyStartLenght > BodyLenght)
                                BodyLenght = BodyStartLenght;
                            break;
                        case 15: BodyGraceHead = OpHelp.GetInt(); break;
                        case 16: BodyGraceTail = OpHelp.GetInt(); break;
                        case 17: FoodValue = OpHelp.GetDouble(); break;
                        case 18: FoodEatMargin = OpHelp.GetDouble(); break;
                        case 19: FoodSpawnMargin = OpHelp.GetDouble(); break;
                        case 20:
                            FoodVisualSize = OpHelp.GetDouble();
                            if (GameOverState) break;
                            Food.Width = FoodVisualSize;
                            Food.Height = FoodVisualSize;
                            break;
                        case 21: StaminaMin = OpHelp.GetDouble(); break;
                        case 22:
                            StaminaMax = OpHelp.GetDouble();
                            if (GameOverState) break;
                            StaminaBar.Maximum = StaminaMax;
                            break;
                        case 23: StaminaRegen = OpHelp.GetDouble(); break;
                        case 24: StaminaRegenBroken = OpHelp.GetDouble(); break;
                        case 25: StaminaDrain = OpHelp.GetDouble(); break;
                        case 26: ScoreValue = OpHelp.GetDouble(); break;
                        case 27: ComboInitiateAfter = OpHelp.GetInt(); break;
                        case 28:
                            ComboDefaultMult = OpHelp.GetDouble();
                            if (GameOverState) break;
                            if (ComboDefaultMult > ComboMult)
                                ComboMult = ComboDefaultMult;
                            break;
                        case 29: ComboMultAdd = OpHelp.GetDouble(); break;
                        case 30: ComboAscensionAfter = OpHelp.GetInt(); break;
                        case 31: ComboMultAddAscension = OpHelp.GetDouble(); break;
                        case 32: ComboTimeMin = OpHelp.GetDouble(); break;
                        case 33:
                            ComboTimeMax = OpHelp.GetDouble();
                            if (GameOverState) break;
                            ComboBar.Maximum = ComboTimeMax;
                            break;
                        case 34: ComboTimeInitiate = OpHelp.GetDouble(); break;
                        case 35: ComboTimeDefaultDrain = OpHelp.GetDouble(); break;
                        case 36: ComboTimeDrainMin = OpHelp.GetDouble(); break;
                        case 37: ComboTimeSlowDownAfter = OpHelp.GetInt(); break;
                        case 38: ComboTimeSlowDownStep = OpHelp.GetInt(); break;
                        case 39: ComboTimeSlowDownEffect = OpHelp.GetDouble(); break;
                        case 40:
                            ColorIndex = OpHelp.GetInt();
                            ApplyColors();
                            break;
                        case 99: if (OpHelp.GetInt() == 1) Process.Start(AppDataFolder); break;
                    }
                }
            }
            catch (Exception ex) { MessageBox.Show(ex.Message, "Options Error:"); }
            finally
            {
                OptionsGrid.Children.Clear();
                OptionsActive = false;
                OptionsView.Visibility = Visibility.Collapsed;
                ScoreLabel.Content = "Options Saved!";
                Coordinates.Content = "";
                if (GameOverState)
                    SetTitle("Game Over");
                else if (TimePaused)
                    SetTitle("Paused");
                else SetTitle();
            }
        }
    }
}
