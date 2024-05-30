using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Text.Json.Serialization;

namespace HungryHorace
{
    class Horace : GameObject
    {
        public Horace() { }

        public Horace(Map map, int x, int y, Direction direction, int speed) : base(map, x, y, direction, speed) { }

        /// <summary>
        /// If is Horace at Exit, new level is loaded.
        /// </summary>
        private void CheckExit()  
        {
            if (IsInTile(TileType.Exit))
            {
                GameManager.horaceInNextLevel = true;
                GameManager.LoadNextLevel();
            }
        }

        private void EatFood(TileType typeOfFood, int score)
        {
            // Coordinates of food.
            var food = IsInTile(typeOfFood);
            if (food)
            {
                map[food.position].type = TileType.Empty;
                GameManager.scoreCurrentLevel += score;
            }
        }

        private void CheckBell() 
        {
            var bell = IsInTile(TileType.Bell); 
            if (bell)
            {
                map[bell.position].type = TileType.Empty;
                GameManager.scoreCurrentLevel += GameManager.specialPoints;
                foreach (GameObject obj in GameManager.gameObjects)
                {
                    // Every enemy on map can now be eaten by Horace.
                    if (obj is Enemy && obj.enabled)
                        ((Enemy)obj).state = EnemyState.Hunted;
                }
            }
        }

        /// <summary>
        /// Horace is moving according to arrow keys.
        /// </summary>
        private void HandleInput()
        {
            if (Input.Pressed[(int)Direction.Left])
            {
                direction = Direction.Left;
                position += VectorFloat.Left * speed;

                // If next step is not valid, coordinates of occurrence remain same.
                if (IsInTile(TileType.Wall) || IsInTile(TileType.Entrance))
                {
                    position -= VectorFloat.Left * speed;
                    position.x = (int)(position.x / GameManager.tileSize) * GameManager.tileSize;
                }
            }
            if (Input.Pressed[(int)Direction.Up])
            {
                direction = Direction.Up;
                position += VectorFloat.Up * speed;
                if (IsInTile(TileType.Wall) || IsInTile(TileType.Entrance))
                {
                    position -= VectorFloat.Up * speed;
                    position.y = (int)(position.y / GameManager.tileSize) * GameManager.tileSize;
                }
            }
            if (Input.Pressed[(int)Direction.Right])
            {
                direction = Direction.Right;
                position += VectorFloat.Right * speed;
                if (IsInTile(TileType.Wall) || IsInTile(TileType.Entrance))
                {
                    position -= VectorFloat.Right * speed;
                    position.x = (int)(Math.Ceiling((float)position.x / (float)GameManager.tileSize) * GameManager.tileSize);
                }
            }
            if (Input.Pressed[(int)Direction.Down])
            {
                direction = Direction.Down;
                position += VectorFloat.Down * speed;
                if (IsInTile(TileType.Wall) || IsInTile(TileType.Entrance))
                {
                    position -= VectorFloat.Down * speed;
                    position.y = (int)(Math.Ceiling((float)position.y / (float)GameManager.tileSize) * GameManager.tileSize);
                }
            }
        }

        public override void Update()   
        {
            HandleInput();
            EatFood(TileType.NormalFood, GameManager.foodPoints);
            EatFood(TileType.SpecialFood, GameManager.specialPoints);
            CheckBell();
            CheckExit();
        }

        public override void Draw(Graphics g)  
        {
            Rectangle rect = new Rectangle((int)position.x, (int)position.y, GameManager.tileSize, GameManager.tileSize);
            g.DrawImage(GameManager.horaceImage, rect);
        }
    }
}
