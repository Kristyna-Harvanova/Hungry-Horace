using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Threading;


// Hladovec Herbert (inspirováno hrou Hungry Horace)
// Kristýna Harvanová, 2. ročník Bc. studia
// zápočtový program (hra), letní semestr 2023
// předmět Pokročilé programování v jazyce C# NPRG038, garant: Mgr. Pavel Ježek, Ph.D.
// cvičení 22bNPRG038x05, učitel: RNDr. Jan Pacovský

namespace HungryHorace
{
    /// <summary>
    /// Manages the whole game.
    /// </summary>
    static class GameManager    
    {
        //DONE: vypocet alogritmu priser v separatnich vlaknech (delegati - pointer na funkci)
        //DONE: dalsi nepritel
        //DONE: serializace do JSON
        //DONE: lambda funkce jsou v: Map.cz, NextStep()


        // Constant variables:
        public static int headerHeight = 3; // In tile unit.
        public static int tileSize;    // Number of pixels for one square-shaped tile. Set automatically according to display size.
        public static int horaceSpeed = 6;  // The amount of pixels Horace moves per one click.
        public static int enemySpeed = 3;      // Must be smaller than tileSize.

        public static Color backgroundColor = Color.LightCyan;  // United for every FormObject.
        public static Brush backgroundBrush = Brushes.LightCyan;
        public static Color stringColor = Color.DarkRed;    // United for every FormObject.
        public static Brush stringBrush = Brushes.DarkRed;

        public static Image horaceImage = Image.FromFile("Images/Horace.PNG");
        public static Image enemyHuntingImage = Image.FromFile("Images/EnemyHunting.PNG");
        public static Image enemyHuntedImage = Image.FromFile("Images/EnemyHunted.PNG");
        public static Image normalFoodImage = Image.FromFile("Images/Flower.PNG");
        public static Image specialFoodImage = Image.FromFile("Images/Cake.PNG");
        public static Image bellImage = Image.FromFile("Images/Present.PNG");
        public static Image arrowImage = Image.FromFile("Images/Arrow.png");

        // Centering of displaying:
        public static Graphics g;
        public static Size clientSize;
        public static VectorInt gameOffset;
        public static int maxWidth = 0;
        public static int maxHeight = 0;

        // Constant variables for game implementation:
        public static int foodPoints = 10;
        public static int specialPoints = 100;
        public static int[] pointsForEnemies; // Amount of points required for other enemies to show up.
        public static int pointsForSpecialFood = 400;

        // Used objects:
        public static StartScreen startScreen;
        public static Header header;
        public static Map map;
        public static EndScreen endScreen;
        public static List<GameObject> gameObjects;

        public static Button btnStart = new Button();   // Start of game.
        public static Button btnEnd = new Button();     // End of application.

        // For choosing number of enemies.
        /*//public static GroupBox enemies = new GroupBox();
        public static RadioButton oneEnemy = new RadioButton();
        public static RadioButton twoEnemy = new RadioButton();
        public static RadioButton threeEnemy = new RadioButton();*/

        public static string messageSave = "Pro uložení aktuálního stavu vaší hry stisněkte Ctrl+S.";
        public static string messageLoad = "Pro znovuobnovení poslední uložené hry stiskněte Ctrl+L.";
        public static string titleHelp = "Nápověda";

        public static Horace horace;
        public static VectorFloat entrancePosition = new VectorFloat();

        // Game progress variables:
        public static bool started;     // Implicit: false. After startButton click: true (begin of game Draw()).
        public static int scoreTotal;  
        public static int scoreCurrentLevel;
        public static int highestScore; // Stored in external file in order to maintain when closed application.
        public static bool enemyWin;    // Implicit: false. After enemy wins: true.

        public static int nextLevel = 0; // Index of map for next level.
        public static (string name, int width, int height)[] levelList;

        // Variables for threading:
        public static List<Thread> threads = new List<Thread>();
        public static bool horaceInNextLevel = false;
        public static bool[] alreadyEnabled = new bool[0];

        public static bool pause; // If true, GameManager.Update() in Form1.cs is not doing anything and threads stop computing.


        /// <summary>
        /// Set variables to default values.
        /// </summary>
        public static void LoadNextLevel()
        {
            foreach (var thread in threads)
            {
                if (thread.IsAlive)
                    thread.Join();
            }

            horaceInNextLevel = false;

            for (int i = 0; i < alreadyEnabled.Length; i++)
                alreadyEnabled[i] = false;

            scoreTotal += scoreCurrentLevel;
            scoreCurrentLevel = 0;
            g.Clear(backgroundColor);   

            gameObjects = new List<GameObject>(); 
            threads = new List<Thread>();
            map = new Map(nextLevel, backgroundColor);

            for (int i = 1; i < gameObjects.Count; i++)
                threads.Add(new Thread(gameObjects[i].Update));

            alreadyEnabled = new bool[threads.Count];

            pointsForEnemies = new int[threads.Count];
            for (int i = 0; i < pointsForEnemies.Length; i++)
                pointsForEnemies[i] = i * 80;

            nextLevel = (nextLevel + 1) % levelList.Length;

            // If new set of levels starts speed is increased.
            if (nextLevel == 0 && horaceSpeed < tileSize)
            {
                horaceSpeed += 2; 
                enemySpeed += 2;
            }
        }

        /// <summary>
        /// Set required values to default.
        /// </summary>
        public static void PrepareNewGame()
        {
            enemyWin = false;   
            horaceSpeed = 6;
            enemySpeed = 3;
            scoreCurrentLevel = 0;
            scoreTotal = 0;
            nextLevel = 0;
        }

        /// <summary>
        /// Set size of bitmap. Load information for particular levels.
        /// </summary>
        /// <param name="path"></param>
        public static void ReadMapsInformation(string path)
        {
            // File with information about all game levels.
            System.IO.StreamReader textFile = new System.IO.StreamReader(path);
            List<(string name, int width, int height)> list = new List<(string, int, int)>();

            while (!textFile.EndOfStream) 
            {
                string name = Input.ReadString(textFile);
                int w = Input.ReadInt(textFile);
                int h = Input.ReadInt(textFile);

                if (maxWidth < w)
                    maxWidth = w;

                if (maxHeight < h)
                    maxHeight = h;

                list.Add((name, w, h));
            }

            levelList = list.ToArray();
        }

        public static void EnableButtons(bool enable)
        {
            btnStart.Enabled = enable;
            btnStart.Visible = enable;
            btnEnd.Enabled = enable;
            btnEnd.Visible = enable;
        }

        private static void btnStart_Click(object sender, EventArgs e)
        {
            MessageBox.Show(messageSave, titleHelp);

            // If button start was clicked once at the beginning, whole game must be loaded.
            if (!started)
                started = true;

            else
                PrepareNewGame();
            
            EnableButtons(false); 
            LoadNextLevel();
        }

        /// <summary>
        /// End application.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void btnEnd_Click(object sender, EventArgs e)
        {
            Form1.ActiveForm.Close();
        }

        /// <summary>
        /// Initialize game buttons for controlling Horace.
        /// </summary>
        public static void InitializeButtons()
        {
            // Start button (size and position adjustment according to dislay size).
            btnStart.Size = new Size(7 * tileSize, 4 * tileSize);
            btnStart.Location = new Point(clientSize.Width / 2 - btnStart.Width - 3 * tileSize, clientSize.Height - 8 * tileSize);

            /*
            // For English version:
            btnStart.Text = "PLAY";
            */

            btnStart.Text = "HRA";
            btnStart.BackColor = Color.RosyBrown;
            btnStart.ForeColor = Color.Beige;
            btnStart.Font = new Font(FontFamily.GenericMonospace, tileSize, FontStyle.Bold);
            btnStart.FlatStyle = FlatStyle.Flat;
            btnStart.FlatAppearance.BorderColor = stringColor;
            btnStart.Click += btnStart_Click;

            // End button.
            btnEnd.Size = new Size(7 * tileSize, 4 * tileSize);
            btnEnd.Location = new Point(clientSize.Width / 2 + 3 * tileSize, clientSize.Height - 8 * tileSize);

            /*
            // For English version:
            btnEnd.Text = "EXIT";
            */

            btnEnd.Text = "KONEC";
            btnEnd.BackColor = Color.RosyBrown;
            btnEnd.ForeColor = Color.Beige;
            btnEnd.Font = new Font(FontFamily.GenericMonospace, tileSize, FontStyle.Bold);
            btnEnd.FlatStyle = FlatStyle.Flat;
            btnEnd.FlatAppearance.BorderColor = stringColor;
            btnEnd.Click += btnEnd_Click;
        }

        /// <summary>
        /// Load game.
        /// </summary>
        /// <param name="clientSize"></param>
        /// <param name="g"></param>
        public static void Initialize(Size clientSize, Graphics g)
        {
            GameManager.g = g;
            GameManager.clientSize = clientSize;
              
            // File with information about all levels.
            ReadMapsInformation("MapsLevels.txt");

            // Maximization size of tileSize.
            tileSize = Math.Min(clientSize.Width / maxWidth, clientSize.Height / (maxHeight + headerHeight));

            // Centering.
            gameOffset = new VectorInt(clientSize.Width - maxWidth * tileSize, clientSize.Height - (maxHeight + headerHeight) * tileSize) / 2;

            // File for highestScore storage in order to its maintenance when not running application.
            StreamReader file = new StreamReader("HighestScore.txt");

            // Encryption.
            highestScore = Input.TranslateToInt(file);
            file.Close();

            startScreen = new StartScreen(clientSize.Width, clientSize.Height, backgroundColor);
            header = new Header(maxWidth * tileSize, headerHeight * tileSize, backgroundColor); 
            endScreen = new EndScreen(clientSize.Width, clientSize.Height, backgroundColor);
            InitializeButtons();
        }

        /// <summary>
        /// Actualization of game progress.
        /// </summary>
        public static void Update() 
        {
            if (!started)
            {
                startScreen.Draw(g);
            }
            else if (enemyWin)
            {
                foreach (var thread in threads)
                {
                    // Some of the enemies may not be enabled already and therefore have started thread.
                    if (thread.IsAlive)
                        thread.Join();
                }

                if (scoreTotal > highestScore)
                {
                    highestScore = scoreTotal;
                    StreamWriter file = new StreamWriter("HighestScore.txt");
                    file.WriteLine(Input.TranslateFromInt(highestScore));
                    file.Close();
                }

                endScreen.Draw(g);
                EnableButtons(true);
            }

            // Through playing:
            else
            {
                header.Draw(g);
                map.Draw(g);

                if (horace.enabled)
                    horace.Update();

                // The thread computing gameObject's route should start work only, when the gameObject is already in the game.
                for (int i = 1; i < gameObjects.Count; i++)
                {
                    if (scoreCurrentLevel > pointsForEnemies[i - 1] && !gameObjects[i].enabled)
                        gameObjects[i].enabled = true;

                    if (!alreadyEnabled[i - 1] && gameObjects[i].enabled)
                    {
                        threads[i - 1].Start();
                        alreadyEnabled[i - 1] = true;
                    }
                }
            }
        }
    }
}
