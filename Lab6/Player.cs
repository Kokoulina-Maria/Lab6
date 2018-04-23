using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Lab6
{
    class Player: Mob
    {
        public List<Bomb> Bombs { get { return bombs; } }
        List<Bomb> bombs = new List<Bomb>();
        Directions direction = Directions.Stand;
        Player()
        {
        }

        public Player(Point position)
        {
            this.position = position;
            Init();
        }

        private void Init()
        {
            step = 10;
            Move();
        }

        public bool PlantBomb()
        {
            try
            {
                var q = (from b in bombs
                         where b.Explosion = false
                         select b).First();
            }
            catch
            {
                bombs.Add(new Bomb(10, position));
                return true;
            }

            return false;
        }

        async Task Move()
        {
            while (true)
            {
                switch (direction)
                {
                    case Directions.Left:
                        if (!Game.PlayerRockCollision(this, -step, 0, direction))
                            position.X -= step;
                        break;
                    case Directions.Right:
                        if (!Game.PlayerRockCollision(this, +step, 0, direction))
                            position.X += step;
                        break;
                    case Directions.Up:
                        if (!Game.PlayerRockCollision(this, 0, -step, direction))
                            position.Y -= step;
                        break;
                    case Directions.Down:
                        if (!Game.PlayerRockCollision(this, 0, step, direction))
                            position.Y += step;
                        break;
                    default:
                        break;
                }
                await Task.Delay(100);
            }
        }

        public void Stop(Directions stopDirection)
        {
            if (stopDirection == direction)
                direction = Directions.Stand;
        }

        public void MoveUp()
        {
            direction = Directions.Up;
        }
        public void MoveDown()
        {
            direction = Directions.Down;
        }
        public void MoveLeft()
        {
            direction = Directions.Left;
        }
        public void MoveRight()
        {
            direction = Directions.Right;
        }
    }
}
