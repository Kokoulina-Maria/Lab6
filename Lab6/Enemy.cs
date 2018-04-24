using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Lab6
{
   public class Enemy : Mob
    {
        Random r;

        int direction = 1;

        Enemy(Random r)
        {
            this.r = r;
            Init();
        }

        private void Init()
        {
            step = 10;
            GenerateDirection();
        }

        public Enemy(Point position, Random r)
        {
            this.r = r;
            this.position = position;
            Init();
        }

        public void Move()
        {
            int x = 0;
            int y = 0;

            switch (direction)
            {
                case (int)Directions.Left:
                    x -= step;
                    break;
                case (int)Directions.Right:
                    x += step;
                    break;
                case (int)Directions.Up:
                    y -= step;
                    break;
                case (int)Directions.Down:
                    y += step;
                    break;
                default:
                    break;
            }

            if (Game.MobRockCollision(this, x, y))
            {
                position.X += x;
                position.Y += y;
            }
            else
                GenerateDirection();
        }

        public void GenerateDirection()
        {
            direction = r.Next(4);
        }
    }
}
