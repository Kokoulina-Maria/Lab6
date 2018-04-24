using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using static Lab6.Mob;

namespace Lab6
{
    class Game
    {
        enum Images { Background, Enemy, Player, Bomb, Rock, Explosion };
        const int gap = 50;

        static int playerOffsetX = 10;
        static int playerOffsetY = 10;

        public static int PlayerOffsetX { get { return playerOffsetX; } }
        public static int PlayerOffsetY { get { return playerOffsetY; } }

        public int[,] Map { get { return map; } }
        public Player Player { get { return player; } }

        static int[,] map ={{4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4},
                            {4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4},
                            {4, 0, 4, 0, 0, 0, 4, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4},
                            {4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4},
                            {4, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4},
                            {4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 4},
                            {4, 0, 0, 0, 4, 0, 0, 0, 4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4},
                            {4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4},
                            {4, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4},
                            {4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4} };

        Player player = new Player(new Point(250, 200));
        List<Enemy> enemies = new List<Enemy>();

        List<Task> tasks = new List<Task>();

        Thread gameThread;
        List<Thread> enemiesThreads = new List<Thread>();
        public List<Enemy> Enemies { get { return enemies; } }

        Random r = new Random();

        public Game()
        {
            for (int i = 0; i < 50; i++)
            {
                Enemy e = new Enemy(new Point(300, 300), r);
                enemies.Add(e);
                enemiesThreads.Add(new Thread(() => UpdateEnemy(e)));
                enemiesThreads.Last().Start();
            }

            //foreach (Enemy e in enemies)
                        
            gameThread = new Thread(GameLoop);
            gameThread.Priority = ThreadPriority.AboveNormal;
            gameThread.Start();
        }

        public void Exit()
        {
            gameThread.Abort();
            foreach (Thread t in enemiesThreads)
                t.Abort();
        }

        void UpdateEnemy(Enemy enemy)
        {//синхронизация
            while (enemy.IsAlive)//пока враг живой
            {
                lock (map) { }//проверяем, не занят ли массив
                Monitor.Enter(map);//занимаем массив
                enemy.Move();//двигаемся по нему
                Monitor.Exit(map);//освобождаем массив
                Thread.Sleep(200);
            }
        }

        public void Pause()
        {
            foreach (Thread t in enemiesThreads)
                t.Suspend();
            gameThread.Suspend();
        }

        public void Resume()
        {
            foreach (Thread t in enemiesThreads)
                t.Resume();
            gameThread.Resume();
        }

        void GameLoop()
        {
            while (true)
            {
                //UpdateEnemiesMoves();
                UpdateBombs();
                UpdateCollisions();
                Thread.Sleep(200);
            }
        }

        public static int PosToCoordinate(double pos)
        {
            return (int)(pos / gap);
        }

        public static bool MobRockCollision(Mob mob, int stepX, int stepY)
        {
            int x;
            int y;
            y = Game.PosToCoordinate(mob.Position.Y + stepY);
            x = Game.PosToCoordinate(mob.Position.X + stepX);
            if (stepX>0)
                x = Game.PosToCoordinate(mob.Position.X + gap - mob.step + stepX);
            else if (stepY > 0)
                y = Game.PosToCoordinate(mob.Position.Y + gap - mob.step + stepY);

            if (map[y, x] != 4 && map[y, x] != (int)Images.Bomb) // 4 is a rock
                return true;
            return false;
        }

        public static bool PlayerRockCollision(Mob mob, int stepX, int stepY, Directions direction)
        {
            int x = Game.PosToCoordinate(mob.Position.X + stepX);
            int y = Game.PosToCoordinate(mob.Position.Y + stepY);

            Point newPos = new Point(mob.Position.X + stepX, mob.Position.Y + stepY);

            switch (direction)
            {
                case Directions.Left:
                    if (map[y, x - 1] == (int)Images.Rock && PlayerBlockCollision(newPos, (x-1) * gap, (y) * gap, gap))
                        return true;
                    if (map[y +1, x - 1] == (int)Images.Rock && PlayerBlockCollision(newPos, (x - 1) * gap, (y+1) * gap, gap))
                        return true;
                    if (map[y-1, x - 1] == (int)Images.Rock && PlayerBlockCollision(newPos, (x - 1) * gap, (y-1) * gap, gap))
                        return true;
                    break;
                case Directions.Right:
                    if (map[y, x + 1] == (int)Images.Rock && PlayerBlockCollision(newPos, (x + 1) * gap, (y) * gap, gap))
                        return true;
                    if (map[y + 1, x + 1] == (int)Images.Rock && PlayerBlockCollision(newPos, (x + 1) * gap, (y + 1) * gap, gap))
                        return true;
                    if (map[y - 1, x + 1] == (int)Images.Rock && PlayerBlockCollision(newPos, (x + 1) * gap, (y - 1) * gap, gap))
                        return true;
                    break;
                case Directions.Up:
                    if (map[y - 1, x] == (int)Images.Rock && PlayerBlockCollision(newPos, (x) * gap, (y - 1) * gap, gap))
                        return true;
                    if (map[y - 1, x + 1] == (int)Images.Rock && PlayerBlockCollision(newPos, (x + 1) * gap, (y - 1) * gap, gap))
                        return true;
                    if (map[y - 1, x - 1] == (int)Images.Rock && PlayerBlockCollision(newPos, (x - 1) * gap, (y - 1) * gap, gap))
                        return true;
                    break;
                case Directions.Down:
                    if (map[y + 1, x] == (int)Images.Rock && PlayerBlockCollision(newPos, (x) * gap, (y + 1) * gap, gap))
                        return true;
                    if (map[y + 1, x + 1] == (int)Images.Rock && PlayerBlockCollision(newPos, (x + 1) * gap, (y + 1) * gap, gap))
                        return true;
                    if (map[y + 1, x - 1] == (int)Images.Rock && PlayerBlockCollision(newPos, (x - 1) * gap, (y + 1) * gap, gap))
                        return true;
                    break;
                default:
                    break;
            }
            return false;
        }

        static bool PlayerBlockCollision(Point player, double xObj, double yObj, double objOffset)
        {
            if ((player.X - Game.PlayerOffsetX  >= xObj && player.X - Game.PlayerOffsetX  <= xObj + objOffset ||
                 player.X + Game.PlayerOffsetX >= xObj && player.X + Game.PlayerOffsetX <= xObj + objOffset) &&
                 (player.Y - Game.PlayerOffsetY >= yObj && player.Y - Game.PlayerOffsetY <= yObj + objOffset ||
                 player.Y + Game.PlayerOffsetY >= yObj && player.Y + Game.PlayerOffsetY <= yObj + objOffset))
                return true;
            return false;
        }

        private void UpdateBombs()
        {
            foreach (Bomb bomb in player.Bombs)
                if (!bomb.IsUsed && bomb.Explosion)
                {
                    bomb.IsUsed = true;
                    new Thread(() => BombExplosion(bomb)).Start();
                }
        }

        void BombExplosion(Bomb bomb)
        {
            SetExplosion((int)Images.Explosion, bomb);
            Thread.Sleep(1000);
            SetExplosion((int)Images.Background, bomb);
        }

        private void SetExplosion(int image, Bomb bomb)
        {
            int x = (int)bomb.Position.X / gap;
            int y = (int)bomb.Position.Y / gap;
            for (int i = 0; i < bomb.Radius; i++)
                if (map[y, x + i] != (int)Images.Rock)
                    map[y, x + i] = image;
                else break;
            for (int i = 0; i < bomb.Radius; i++)
                if (map[y, x - i] != (int)Images.Rock)
                    map[y, x - i] = image;
                else break;
            for (int i = 0; i < bomb.Radius; i++)
                if (map[y + i, x] != (int)Images.Rock)
                    map[y + i, x] = image;
                else break;
            for (int i = 0; i < bomb.Radius; i++)
                if (map[y - i, x] != (int)Images.Rock)
                    map[y - i, x] = image;
                else break;
        }

        void UpdateCollisions()
        {
            foreach (Enemy enemy in enemies)
            {
                if (EnemyCollision(enemy))
                    player.Death();
                if (map[PosToCoordinate(enemy.Position.Y), PosToCoordinate(enemy.Position.X)] == (int)Images.Explosion)
                    enemy.Death();
            }
            if (map[PosToCoordinate(player.Position.Y), PosToCoordinate(player.Position.X)] == (int)Images.Explosion)
                player.Death();
        }

        bool EnemyCollision(Enemy enemy)
        {
            double xe = enemy.Position.X;
            double ye = enemy.Position.Y;
            double xp = player.Position.X ;
            double yp = player.Position.Y;
            if ((xe <= xp - Game.PlayerOffsetX && xe + gap >= xp - Game.PlayerOffsetX ||
                xe <= xp + Game.PlayerOffsetX && xe + gap >= xp + Game.PlayerOffsetX) &&
                (ye <= yp - Game.PlayerOffsetY && ye + gap >= yp - Game.PlayerOffsetY ||
                ye <= yp + Game.PlayerOffsetY && ye + gap >= yp + Game.PlayerOffsetY))
                return true;
            return false;
        }

        void UpdateEnemiesMoves()
        {
            foreach (Enemy enemy in enemies)
                if (enemy.IsAlive)
                    enemy.Move();
        }

    }
}
