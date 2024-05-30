using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HungryHorace
{
    public enum EnemyState
    {
        Hunting,    // Normal state of enemy, it hunts its target.
        Hunted      // State of enemy, if Horace enters TileType.Bell. Enemy can be eaten and Horace gets 100 points.
    }

    // There will be inherited classes from this Enemy class in order to have various types of Enemies.
    // If there was only one type (all had same target), they could merge into themselves ("One enemy could disappear").
    abstract class Enemy : GameObject
    {   
        public EnemyState state = EnemyState.Hunting;

        public Enemy() { }

        public Enemy(Map map, int x, int y, Direction direction, int speed) : base(map, x, y, direction, speed) { }

        public void MoveToTarget(VectorInt targetPositionInTiles)
        {
            // Coordinates of the center of the tile, where is enemy currently.
            VectorInt positionInTiles = (VectorInt)((position + VectorFloat.One * GameManager.tileSize / 2) / GameManager.tileSize);

            // If Horace goes to another level, there is no need to chase him (return).
            if (!map.IsInMap((VectorInt)(GameManager.horace.position / GameManager.tileSize)))    
                return;

            // If any kind of enemy (chasing different targets) is near to Horace (one tile from him).
            if ((GameManager.horace.position - position).GetMagnitude() < GameManager.tileSize) 
            {
                if (state == EnemyState.Hunting)
                {
                    GameManager.enemyWin = true;
                    GameManager.scoreTotal += GameManager.scoreCurrentLevel;
                    return;
                }
                else if (state == EnemyState.Hunted)
                {
                    GameManager.scoreCurrentLevel += GameManager.specialPoints;

                    // "New" hunting enemies entering through Entrance must appear.
                    position.Set(GameManager.entrancePosition); 
                    state = EnemyState.Hunting;
                    return;
                }
            }

            // Finding out another step to target.
            List<Tile> pathTiles = map.NextStep(map[positionInTiles], map[targetPositionInTiles]);

            // If target was found (return).
            if (pathTiles == null || pathTiles.Count < 2)
                return;

            // Finding out next direction (current position to next step).
            int nextTile = 1;
            VectorFloat difference = pathTiles[nextTile].position * GameManager.tileSize - position;
            float distance = difference.GetMagnitude();

            // If speed is slow (next tile is current tile).
            if (distance > GameManager.tileSize)
            {
                nextTile = 0;
                difference = pathTiles[nextTile].position * GameManager.tileSize - position;
                distance = difference.GetMagnitude();
            }

            // If speed is high.
            if (distance < speed)
            {
                float over = speed - distance;
                VectorFloat direction = pathTiles[nextTile + 1].position - pathTiles[nextTile].position;
                direction.SetMagnitude(over);   
                position = pathTiles[nextTile].position * GameManager.tileSize + direction;
            }

            // Moving according to speed.
            else
            {
                position += difference / distance * speed;
            }
        }

        /// <summary>
        /// Find next tile, where should Enemy move.
        /// </summary>
        public abstract void FindPath();

        /// <summary>
        /// Adjust appearance according to current EnemyState (Hunting or Hunted).
        /// </summary>
        /// <param name="g"></param>
        public override void Draw(Graphics g)
        {   
            Rectangle rect = new Rectangle((int)position.x, (int)position.y, GameManager.tileSize, GameManager.tileSize);

            if (state == EnemyState.Hunting)
                g.DrawImage(GameManager.enemyHuntingImage, rect);

            else if (state == EnemyState.Hunted)
                g.DrawImage(GameManager.enemyHuntedImage, rect);

            /*
            // this will draw path to Horace (not to actual target of enemy)
            var path = map.NextStep(map[(VectorInt)(position / GameManager.tileSize)], map[(VectorInt)GameManager.horace.position / GameManager.tileSize]);

            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    g.DrawLine(Pens.Black, 
                        path[i].position.x * GameManager.tileSize + GameManager.tileSize / 2, 
                        path[i].position.y * GameManager.tileSize + GameManager.tileSize / 2, 
                        path[i + 1].position.x * GameManager.tileSize + GameManager.tileSize / 2, 
                        path[i + 1].position.y * GameManager.tileSize + GameManager.tileSize / 2);
                }
            }
            */
        }

        public override void Update()
        {
            FindPath();
        }
    }

    /// <summary>
    /// Search directly for Horace.
    /// </summary>
    class EnemyA : Enemy
    {
        // After reaching particular amount of points, enemy give Horace specialFood (leave on current position).
        public bool addedSpecialFood = false;

        public EnemyA() { }
        public EnemyA(Map map, int x, int y, Direction direction, int speed) : base(map, x, y, direction, speed) { }

        /// <summary>
        /// Enemy A search directly for Horace.
        /// </summary>
        public override void FindPath() 
        {
            VectorInt horacePositionInTiles = (VectorInt)((GameManager.horace.position + VectorFloat.One * GameManager.tileSize / 2) / GameManager.tileSize);
            MoveToTarget(horacePositionInTiles);
        }

        public override void Update()   
        {
            while (!GameManager.horaceInNextLevel && !GameManager.enemyWin)
            {
                // Enemy A leaves speacialFood for Horace after certain amount of reached score.
                base.Update();
                if (GameManager.scoreCurrentLevel > GameManager.pointsForSpecialFood && !addedSpecialFood)
                {
                    map[(VectorInt)(position / GameManager.tileSize)].type = TileType.SpecialFood;
                    addedSpecialFood = true;
                }

                // For the smooth flow of the game.
                System.Threading.Thread.Sleep(50);

                // If is the game paused, the thread should not do work and continue in playing the game.
                while (GameManager.pause)
                    System.Threading.Thread.Yield();
            }
        }
    }

    /// <summary>
    /// Search for "c" tiles in front of or behind Horace.
    /// </summary>
    class EnemyB : Enemy
    {
        // Enemies can not have the same target (they could merge). If EnemyB reaches its target, it can not stop moving.
        // When target in front of Horace is reached it changes to target behind Horace and vice versa (dir = +- 1).
        private int c = 4; 
        private int dir = 1;

        public EnemyB() { }

        public EnemyB(Map map, int x, int y, Direction direction, int speed) : base(map, x, y, direction, speed)
        {
            // EnemyB is coming after some amount of reached points.
            enabled = false;    
        }

        /*
        // this will draw path to the actual target of enemy
        public VectorInt vector = new VectorInt();
        */

        public override void FindPath()
        {
            VectorInt horacePositionInTiles = (VectorInt)((GameManager.horace.position + VectorFloat.One * GameManager.tileSize / 2) / GameManager.tileSize);
            VectorInt targetPositionInTiles = horacePositionInTiles + dir * c * VectorInt.FromDirection(GameManager.horace.direction);

            // If actual target is not in map or is a wall (i.e. Horace is at the border). Enemy is searching for less than c tiles around Horace. 
            while (!map.IsInMap(targetPositionInTiles) || map[targetPositionInTiles].type == TileType.Wall)
            {   
                targetPositionInTiles -= dir * VectorInt.FromDirection(GameManager.horace.direction); 
                if (targetPositionInTiles == horacePositionInTiles)
                    break;
            }

            // If enemy reach its target, target changes. In order to keep moving.
            if (targetPositionInTiles == PositionInTiles()) 
                dir = -dir;

            MoveToTarget(targetPositionInTiles);

            /*
            // this will draw path to the actual target of enemy
            vector = targetPositionInTiles * GameManager.tileSize;
            */
        }

        public override void Draw(Graphics g)
        {
            base.Draw(g);

            /*
            // this will draw path to the actual target of enemy
            g.DrawRectangle(Pens.Black, vector.x, vector.y, GameManager.tileSize, GameManager.tileSize);

            var path = map.NextStep(map[(VectorInt)(position / GameManager.tileSize)], map[vector / GameManager.tileSize]);

            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    g.DrawLine(Pens.Black, 
                        path[i].position.x * GameManager.tileSize + GameManager.tileSize / 2, 
                        path[i].position.y * GameManager.tileSize + GameManager.tileSize / 2, 
                        path[i + 1].position.x * GameManager.tileSize + GameManager.tileSize / 2, 
                        path[i + 1].position.y * GameManager.tileSize + GameManager.tileSize / 2);
                }
            }
            */
        }

        public override void Update()
        {
            while (!GameManager.horaceInNextLevel && !GameManager.enemyWin)  
            {
                base.Update();

                // For the smooth flow of the game.
                System.Threading.Thread.Sleep(50);

                // If is the game paused, the thread should not do work and continue in playing the game.
                while (GameManager.pause)
                    System.Threading.Thread.Yield();
            }
        }
    }

    class EnemyC : Enemy
    {
        // Enemies can not have the same target (they could merge). If EnemyC reaches its target, it can not stop moving.
        // When the enemy reaches the surroundings of Horace (its target) it changes its target to the entrance position.

        public EnemyC() { }

        public EnemyC(Map map, int x, int y, Direction direction, int speed) : base(map, x, y, direction, speed)
        {
            // EnemyC is coming after some amount of reached points.
            enabled = false;
        }

        /*// this will draw path to the actual target of enemy
        public VectorInt vector = new VectorInt();*/

        private VectorInt previousTarget = (VectorInt)((GameManager.entrancePosition + VectorFloat.One * GameManager.tileSize / 2) / GameManager.tileSize);

        public override void FindPath()
        {
            VectorInt horacePositionInTiles = (VectorInt)((GameManager.horace.position + VectorFloat.One * GameManager.tileSize / 2) / GameManager.tileSize);
            VectorInt entrancePositionInTiles = (VectorInt)((GameManager.entrancePosition + VectorFloat.One * GameManager.tileSize / 2) / GameManager.tileSize);
            VectorInt targetPositionInTiles = previousTarget;
            
            // If enemy is in the surroundings of the horace, the target changes.
            int difference = (int)Math.Round((horacePositionInTiles - PositionInTiles()).GetMagnitude());

            if (difference <= 8)
                targetPositionInTiles = entrancePositionInTiles;
            else if (difference > 12)
                targetPositionInTiles = horacePositionInTiles;
            
            previousTarget = targetPositionInTiles;

            MoveToTarget(targetPositionInTiles);

            /*// this will draw path to the actual target of enemy
            vector = targetPositionInTiles * GameManager.tileSize;*/
        }

        public override void Draw(Graphics g)
        {
            base.Draw(g);


            /*// this will draw path to the actual target of enemy
            g.DrawRectangle(Pens.Black, vector.x, vector.y, GameManager.tileSize, GameManager.tileSize);

            var path = map.NextStep(map[(VectorInt)(position / GameManager.tileSize)], map[vector / GameManager.tileSize]);

            if (path != null)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    g.DrawLine(Pens.Black,
                        path[i].position.x * GameManager.tileSize + GameManager.tileSize / 2,
                        path[i].position.y * GameManager.tileSize + GameManager.tileSize / 2,
                        path[i + 1].position.x * GameManager.tileSize + GameManager.tileSize / 2,
                        path[i + 1].position.y * GameManager.tileSize + GameManager.tileSize / 2);
                }
            }*/

        }

        public override void Update()
        {
            while (!GameManager.horaceInNextLevel && !GameManager.enemyWin)
            {
                base.Update();

                // For the smooth flow of the game.
                System.Threading.Thread.Sleep(50);

                // If is the game paused, the thread should not do work and continue in playing the game.
                while (GameManager.pause)
                    System.Threading.Thread.Yield();
            }
        }
    }
}
