using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HungryHorace
{
    class StateOfGame
    {
        // Constant variables:
        public int headerHeight { get; set; }
        public int tileSize { get; set; }
        public int horaceSpeed { get; set; }
        public int enemySpeed { get; set; }

        // Centering of displaying:
        public Size clientSize { get; set; }
        public VectorInt gameOffset { get; set; }
        public int maxWidth { get; set; }
        public int maxHeight { get; set; }

        // Constant variables for game implementation:
        public int foodPoints { get; set; }
        public int specialPoints { get; set; }
        public int[] pointsForEnemies { get; set; }
        public int pointsForSpecialFood { get; set; }

        // Used objects:
        public List<GameObject> gameObjects { get; set; }

        public VectorFloat entrancePosition { get; set; }

        // Game progress variables:
        public bool started { get; set; }
        public int scoreTotal { get; set; }
        public int scoreCurrentLevel { get; set; }
        public int highestScore { get; set; } 
        public bool enemyWin { get; set; }

        public int nextLevel { get; set; }
        public (string name, int width, int height)[] levelList { get; set; }

        // Variables for threading:
        public bool[] alreadyEnabled { get; set; }


        public static void Save()
        {
            GameManager.pause = true;

            var state = new StateOfGame
            {
                headerHeight = GameManager.headerHeight,
                tileSize = GameManager.tileSize,
                horaceSpeed = GameManager.horaceSpeed,
                enemySpeed = GameManager.enemySpeed,

                // Centering of displaying:
                clientSize = GameManager.clientSize,
                gameOffset = GameManager.gameOffset,
                maxWidth = GameManager.maxWidth,
                maxHeight = GameManager.maxHeight,

                // Constant variables for game implementation:
                foodPoints = GameManager.foodPoints,
                specialPoints = GameManager.specialPoints,
                pointsForEnemies = GameManager.pointsForEnemies,
                pointsForSpecialFood = GameManager.pointsForSpecialFood,

                // Used objects:
                gameObjects = GameManager.gameObjects,

                entrancePosition = GameManager.entrancePosition,

                // Game progress variables:
                started = GameManager.started,    
                scoreTotal = GameManager.scoreTotal,
                scoreCurrentLevel = GameManager.scoreCurrentLevel,
                highestScore = GameManager.highestScore,
                enemyWin = GameManager.enemyWin,   

                nextLevel = GameManager.nextLevel,
                levelList = GameManager.levelList,

                // Variables for threading:
                alreadyEnabled = GameManager.alreadyEnabled,
            };

            GameManager.map.Write();

            string fileName = "HungryHorace.json";

            var options = new JsonSerializerOptions { IncludeFields = true };
            options.Converters.Add(new GameObjectConverter());
            options.Converters.Add(new MapConverter());
            options.Converters.Add(new TileArrayConverter());

            string json = JsonSerializer.Serialize(state, options);
            File.WriteAllText(fileName, json);
        }

        private static int Mod(int x, int m)
        {
            return (x % m + m) % m;
        }

        public static void Restore()
        {
            string fileName = "HungryHorace.json";
            string json = File.ReadAllText(fileName);
            var options = new JsonSerializerOptions { IncludeFields = true };
            options.Converters.Add(new GameObjectConverter());
            options.Converters.Add(new MapConverter()); 
            options.Converters.Add(new TileArrayConverter()); 

            StateOfGame state = JsonSerializer.Deserialize<StateOfGame>(json, options);

            if (!GameManager.started)
                GameManager.btnStart.PerformClick();

            GameManager.enemyWin = state.enemyWin;
            GameManager.horaceSpeed = state.horaceSpeed;
            GameManager.enemySpeed = state.enemySpeed;
            GameManager.scoreCurrentLevel = state.scoreCurrentLevel;
            GameManager.scoreTotal = state.scoreTotal;
            GameManager.nextLevel = state.nextLevel;

            GameManager.EnableButtons(false);

            foreach (var thread in GameManager.threads)
            {
                if (thread.IsAlive)
                    thread.Join();
            }

            GameManager.horaceInNextLevel = false;
            GameManager.alreadyEnabled = state.alreadyEnabled;
            GameManager.g.Clear(GameManager.backgroundColor);

            GameManager.threads = new List<Thread>();
            // GameManager.map = new Map((state.nextLevel - 1) % state.levelList.Length, GameManager.backgroundColor, true);
            GameManager.map = new Map(Mod(state.nextLevel - 1, state.levelList.Length), GameManager.backgroundColor, true);

            GameManager.gameObjects = new List<GameObject>();

            // Horace with his new entrance position.
            GameManager.entrancePosition = state.gameObjects[0].position;
            GameManager.horace = new Horace(GameManager.map, (int)GameManager.entrancePosition.x, (int)GameManager.entrancePosition.y, Direction.Right, GameManager.horaceSpeed);
            GameManager.gameObjects.Add(GameManager.horace);

            // Other objects with their normal entrance for the progress of the game.
            GameManager.entrancePosition = state.entrancePosition;
            for (int i = 1; i < state.gameObjects.Count; i++)
                GameManager.gameObjects.Add(state.gameObjects[i]);

            for (int i = 1; i < GameManager.gameObjects.Count; i++)
                GameManager.threads.Add(new Thread(GameManager.gameObjects[i].Update));

            GameManager.pointsForEnemies = new int[GameManager.threads.Count];
            for (int i = 0; i < GameManager.pointsForEnemies.Length; i++)
                GameManager.pointsForEnemies[i] = i * 80;

            // If new set of levels starts speed is increased.
            if (GameManager.nextLevel == 0 && GameManager.horaceSpeed < GameManager.tileSize)
            {
                GameManager.horaceSpeed += 2;
                GameManager.enemySpeed += 2;
            }

            for (int i = 1; i < GameManager.gameObjects.Count; i++)
            {
                if (GameManager.alreadyEnabled[i - 1] && GameManager.gameObjects[i].enabled)
                    GameManager.threads[i - 1].Start();
            }
        }
    }
}
