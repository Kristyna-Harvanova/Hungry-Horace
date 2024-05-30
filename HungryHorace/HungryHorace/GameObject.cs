using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Text.Json.Serialization;

namespace HungryHorace
{
    abstract class GameObject
    {
        public Map map;
        public VectorFloat position; // In pixels.
        public Direction direction;
        public bool enabled = true;
        public int speed;  // In pixels for one click.

        public GameObject() { }

        public GameObject(Map map, int x, int y, Direction direction, int speed)
        {
            this.map = map;
            position = new VectorFloat(x, y);
            this.direction = direction;
            this.speed = speed;
        }

        /// <summary>
        /// Actualize object information (if should move, etc.)
        /// </summary>
        public abstract void Update();

        /// <summary>
        /// Draw object in bitmap.
        /// </summary>
        /// <param name="g"></param>
        public abstract void Draw(Graphics g);

        /// <summary>
        /// Returns tile coordinates, where is object currently. In tiles unit.
        /// </summary>
        /// <returns></returns>
        public VectorInt PositionInTiles()
        {
            return (VectorInt)((position + VectorFloat.One * GameManager.tileSize / 2) / GameManager.tileSize); 
        }

        /// <summary>
        /// If is object on parameter type tile, coordinates are returned. Else null is returned.
        /// </summary>
        /// <param name="tileType"></param>
        /// <returns></returns>
        public Tile IsInTile(TileType tileType)
        {
            // Left upper corner:
            if (map[(VectorInt)((position + VectorFloat.Zero * (GameManager.tileSize - 1)) / GameManager.tileSize)].type == tileType)
                return new Tile(tileType, (VectorInt)((position + VectorFloat.Zero * (GameManager.tileSize - 1)) / GameManager.tileSize));

            // Right upper:
            if (map[(VectorInt)((position + VectorFloat.Right * (GameManager.tileSize - 1)) / GameManager.tileSize)].type == tileType)
                return new Tile(tileType, (VectorInt)((position + VectorFloat.Right * (GameManager.tileSize - 1)) / GameManager.tileSize));
            
            // Right bottom:
            if (map[(VectorInt)((position + VectorFloat.One * (GameManager.tileSize - 1)) / GameManager.tileSize)].type == tileType)
                return new Tile(tileType, (VectorInt)((position + VectorFloat.One * (GameManager.tileSize - 1)) / GameManager.tileSize));
            
            // Left bottom:
            if (map[(VectorInt)((position + VectorFloat.Down * (GameManager.tileSize - 1)) / GameManager.tileSize)].type == tileType)
                return new Tile(tileType, (VectorInt)((position + VectorFloat.Down * (GameManager.tileSize - 1)) / GameManager.tileSize));
            
            return null;
        }
    }
}