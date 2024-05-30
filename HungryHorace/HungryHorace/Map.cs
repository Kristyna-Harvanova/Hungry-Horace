using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace HungryHorace
{
    /// <summary>
    /// Types of tiles on map.
    /// </summary>
    enum TileType 
    {
        Empty = '0',  
        Wall = 'X',
        NormalFood = 'F', 
        SpecialFood = 'S',
        Bell = 'B',
        Exit = 'E',
        Entrance = 'e'
    }

    /// <summary>
    /// Represents one Tile on Map.
    /// </summary>
    class Tile
    {
        public TileType type;
        public VectorInt position; 

        public int cost;    // Number of tiles on the way from this tile (current position) to start tile.
        public int distance; // The smallest number of tiles (without obstacles, i.e. Wall) on the way from this tile (current position) to target tile.
        public Tile parent; // Previous tile on the way to this tile (current position).

        public Tile () { }

        public Tile(TileType type, VectorInt position)
        {
            this.type = type;
            this.position = position;
        }

        public Tile(TileType type, int x, int y) : this(type, new VectorInt(x, y)) { }

        /// <summary>
        /// Returns true, if Tile exits.
        /// </summary>
        /// <param name="coordinates"></param>
        public static implicit operator bool(Tile coordinates)
        { return coordinates != null; }

        public void SetCost(int cost)
        { this.cost = cost; }

        public void SetParent(Tile parent)
        { this.parent = parent; }

        public void SetDistance(Tile target)
        {
            distance = Math.Abs(target.position.x - position.x) + Math.Abs(target.position.y - position.y);
        }
    }

    class Map : FormObject
    {
        // Size of map in Tile unit.
        public int width;
        public int height;

        public Tile[,] map;

        public Map() { }

        public Map(int level, Color backgroundColor, bool restored = false)
        {
            Read(level, restored);
            
            // Setting size of map in pixels.
            size = new Size(width * GameManager.tileSize, height * GameManager.tileSize);
            Initialize(backgroundColor);
        }

        /// <summary>
        /// Indexes in map using VectorInt. No need to use coordinates.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public Tile this[VectorInt v] 
        {
            get { return map[v.x, v.y]; }
            set { map[v.x, v.y] = value; }
        }

        /// <summary>
        /// Load map from file, initialize game objects.
        /// </summary>
        /// <param name="level"></param>
        public void Read(int level, bool restored)
        {
            var data = GameManager.levelList[level];
            System.IO.StreamReader textFile;

            if (restored)
                textFile = new System.IO.StreamReader("CurrentMapState.txt");
            else
                textFile = new System.IO.StreamReader("Levels/" + data.name);

            width = data.width;
            height = data.height;

            map = new Tile[width, height]; 

            VectorInt horaceCoords = new VectorInt();   
            VectorInt enemyCoords = new VectorInt();

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    TileType tileType = (TileType)Input.ReadChar(textFile); 
                    map[x, y] = new Tile(tileType, x, y); 

                    // Starting position of Horace is from Entrance.
                    if (map[x, y].type == TileType.Entrance)    
                        horaceCoords.Set(x, y);

                    // Starting position of enemies is from Exit.
                    if (map[x, y].type == TileType.Exit) 
                        enemyCoords.Set(x, y);
                }
            }

            if (!restored)
            {
                // Starting position of Horace can not be exactly in Entrance, but in Empty tile near. Testing all neighbour tiles.
                foreach (Direction dir in Enum.GetValues(typeof(Direction)))
                {
                    VectorInt newPosition = horaceCoords + VectorInt.FromDirection(dir);
                    if (IsInMap(newPosition) && this[newPosition].type != TileType.Wall && this[newPosition].type != TileType.Entrance)
                    {
                        horaceCoords = newPosition;
                        break;
                    }
                }
            
                GameManager.entrancePosition = horaceCoords * GameManager.tileSize;

                horaceCoords *= GameManager.tileSize;    // Horace position translate from Tile unit to pixels.
                GameManager.horace = new Horace(this, horaceCoords.x, horaceCoords.y, Direction.Right, GameManager.horaceSpeed);
                GameManager.gameObjects.Add(GameManager.horace);

                enemyCoords *= GameManager.tileSize;
                EnemyA enemy1 = new EnemyA(this, enemyCoords.x, enemyCoords.y, Direction.Left, GameManager.enemySpeed);
                GameManager.gameObjects.Add(enemy1);
                EnemyB enemy2 = new EnemyB(this, enemyCoords.x, enemyCoords.y, Direction.Left, GameManager.enemySpeed);
                GameManager.gameObjects.Add(enemy2);
                EnemyC enemy3 = new EnemyC(this, enemyCoords.x, enemyCoords.y, Direction.Left, GameManager.enemySpeed);
                GameManager.gameObjects.Add(enemy3);
            }

            textFile.Close();
        }

        public void Write()
        {
            System.IO.StreamWriter file = new System.IO.StreamWriter("CurrentMapState.txt");

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                    file.Write((char)map[x, y].type);

                file.WriteLine();
            }

            file.Close();
        }

        /// <summary>
        /// Returns true, if this position is in map.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool IsInMap(VectorInt v)
        {
            return 0 <= v.x && v.x < width && 0 <= v.y && v.y < height;
        }

        /// <summary>
        /// Draws map and objects on map.
        /// </summary>
        /// <param name="g"></param>
        public override void Draw(Graphics g)    
        {
            base.Draw(g);

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    switch (map[x, y].type)
                    {
                        case TileType.Wall:
                            bitmapGraphics.FillRectangle(Brushes.RosyBrown, x * GameManager.tileSize, y * GameManager.tileSize, GameManager.tileSize, GameManager.tileSize);
                            break;

                        case TileType.NormalFood:
                            // Size is equal to 3/4 of one Tile.
                            Rectangle rect = new Rectangle(x * GameManager.tileSize, y * GameManager.tileSize + GameManager.tileSize / 2, 3 * GameManager.tileSize / 4, 3 * GameManager.tileSize / 4);
                            bitmapGraphics.DrawImage(GameManager.normalFoodImage, rect);
                            break;

                        case TileType.SpecialFood:
                            rect = new Rectangle(x * GameManager.tileSize, y * GameManager.tileSize, GameManager.tileSize, GameManager.tileSize);
                            bitmapGraphics.DrawImage(GameManager.specialFoodImage, rect);
                            break;

                        case TileType.Bell:
                            // tileSize/2 is substracted for displaying in the center of tile.
                            rect = new Rectangle(x * GameManager.tileSize - GameManager.tileSize / 2, y * GameManager.tileSize + GameManager.tileSize / 2, GameManager.tileSize, GameManager.tileSize);
                            bitmapGraphics.DrawImage(GameManager.bellImage, rect);
                            break;

                        case TileType.Entrance: case TileType.Exit:
                            rect = new Rectangle(x * GameManager.tileSize, y * GameManager.tileSize, GameManager.tileSize, GameManager.tileSize);
                            bitmapGraphics.DrawImage(GameManager.arrowImage, rect);
                            break;

                        default:
                            break;
                    }
                }
            }

            // Drawing map.
            foreach (GameObject obj in GameManager.gameObjects)
            {
                if (obj.enabled)
                    obj.Draw(bitmapGraphics);
            }

            // Drawing objects.
            g.DrawImage(bitmap, GameManager.gameOffset.x, GameManager.headerHeight * GameManager.tileSize + GameManager.gameOffset.y + (GameManager.maxHeight - height) / 2 * GameManager.tileSize); 
        }

        /// <summary>
        /// Find the shortest path in map.
        /// </summary>
        /// <param name="start"></param>
        /// <param name="finish"></param>
        /// <returns></returns>
        public List<Tile> NextStep(Tile start, Tile finish)
        {
            //  Algorithm A* is used (from tile "start" to tile "finish").

            start.SetDistance(finish);

            // From activeTiles shortest path is being find, starting with "start".
            List<Tile> activeTiles = new List<Tile>();   
            activeTiles.Add(start); 

            List<Tile> visitedTiles = new List<Tile>();

            // Testing all activeTiles.
            while (activeTiles.Any())
            {
                // Tile with the smallest evaluation (cost + distance) in order to work on the currently best path.
                Tile checkTile = activeTiles.OrderByDescending(tile => tile.cost + tile.distance).Last();

                // If "finish" was found, path is retraced and tiles in th shortest path are found.
                if (checkTile.position == finish.position)
                {   
                    Tile tile = checkTile;
                    List<Tile> pathTiles = new List<Tile>();

                    while (true)
                    {
                        // Adding previous tile (parent).
                        pathTiles.Add(tile);
                        tile = tile.parent;

                        // If tile without parent was found, it is the "start", tiles are orders from "start" to "finish".
                        if (tile == null)
                        {
                            pathTiles.Reverse();
                            return pathTiles;
                        }
                    }
                }

                // If "finish" was not found.
                visitedTiles.Add(checkTile);
                activeTiles.Remove(checkTile); 

                // Finding potencial next tiles in the way.
                List<Tile> walkableTiles = GetWalkableTiles(checkTile, finish);
                foreach (var walkableTile in walkableTiles)
                {
                    // If tile was already visited.
                    if (visitedTiles.Any(tile => tile.position == walkableTile.position))
                        continue;

                    // If tile is already in activeTiles, go there now. Path through tested tile to active is equally long.
                    if (activeTiles.Any(tile => tile.position == walkableTile.position))
                    {   
                        Tile existingTile = activeTiles.First(tile => tile.position == walkableTile.position);
                        if (existingTile.cost + existingTile.distance > checkTile.cost + checkTile.distance)
                        {
                            activeTiles.Remove(existingTile);
                            activeTiles.Add(walkableTile);
                        }
                    }

                    // Or just put to activeTiles.
                    else
                        activeTiles.Add(walkableTile);
                }
            }

            // If the shortest path is not found. This should not happen here, working in map, target is in the same map.
            return null;    
        }

        /// <summary>
        /// Returns tiles, that are potential next steps to the target.
        /// </summary>
        /// <param name="currentTile"></param>
        /// <param name="targetTile"></param>
        /// <returns></returns>
        private List<Tile> GetWalkableTiles(Tile currentTile, Tile targetTile)
        { 
            List<Tile> walkableTiles = new List<Tile>(); 

            // Next step can be done in directions to left, up, right, down.
            foreach (Direction dir in Enum.GetValues(typeof(Direction)))   
            {
                // Coordinates of tested tile.
                VectorInt direction = VectorInt.FromDirection(dir);
                VectorInt next = currentTile.position + direction;
                
                if (IsInMap(next) && this[next].type != TileType.Wall)
                {
                    Tile tile = new Tile(TileType.Empty, next);
                    tile.SetCost(currentTile.cost + 1);
                    tile.SetDistance(targetTile);
                    tile.SetParent(currentTile);
                    walkableTiles.Add(tile);
                }
            }

            return walkableTiles;
        }
    }
}
