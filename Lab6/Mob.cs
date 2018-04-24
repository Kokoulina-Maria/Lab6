using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Lab6
{
    public abstract class Mob
    {
        public enum Directions { Left, Right, Up, Down, Stand};

        public int health;
        public int strength;
        public int speed = 10;
        public int step = 5;
        public double rotation;
        public Point position;
        public Point Position { get { return position; } set { position = value; } }
        public bool dead = false;
        public bool IsAlive { get { return !dead; } }

        virtual public void Death()
        {
            dead = true;
        }
    }
}
