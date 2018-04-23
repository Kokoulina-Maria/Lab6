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
        public List<Enemy> Enemies { get { return enemies; } }
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

        static List<Point> clsBox= new List<Point>();

        Player player = new Player(new Point(250, 200));
        List<Enemy> enemies = new List<Enemy>();
        static int x = 23, y = 10;

        List<Task> tasks = new List<Task>();
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        public Game()
        {
            clsBox.Add(new Point(0, 1));
            clsBox.Add(new Point(1, 0));
            clsBox.Add(new Point(1, 1));
            clsBox.Add(new Point(0, -1));
            clsBox.Add(new Point(-1, 0));
            clsBox.Add(new Point(-1, -1));
            clsBox.Add(new Point(1, -1));
            clsBox.Add(new Point(-1, 1));

            var t = Task.Run(() => ReadMapAsync(map));
            enemies.Add(new Enemy(new Point(300, 300)));
            enemies.Add(new Enemy(new Point(200, 300)));
            enemies.Add(new Enemy(new Point(500, 250)));
            CancellationToken token = tokenSource.Token;
            GameLoop(token);
        }

        public void Exit()
        {
            tokenSource.Cancel();
        }

        async Task GameLoop(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;
                UpdateEnemiesMoves();
                UpdateBombs();
                UpdateCollisions();
                await Task.Delay(200);
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

        public async Task ReadMapAsync(int[,] map)
        {
            try
            {
                StorageFolder storageFolder = ApplicationData.Current.LocalFolder;
                StorageFile sampleFile = await storageFolder.GetFileAsync("map.txt");
                string input = await FileIO.ReadTextAsync(sampleFile);
                input = input.Replace("\t", "");

                int i = 0, j = 0;
                int[,] result = new int[x, y];
                foreach (var row in input.Split('\n'))
                {
                    j = 0;
                    foreach (var col in row.Trim().Split(' '))
                    {
                        result[j, i] = int.Parse(col.Trim());
                        j++;
                    }
                    i++;
                }
                map = result;
            }
            catch (Exception e)
            {
                MessageDialog md = new MessageDialog(e.Message);
                await md.ShowAsync();
            }
        }

        private void UpdateBombs()
        {
            foreach (Bomb bomb in player.Bombs)
                if (!bomb.IsUsed && bomb.Explosion)
                {
                    bomb.IsUsed = true;
                    BombExplosion(bomb);
                }
        }

        async Task BombExplosion(Bomb bomb)
        {
            SetExplosion((int)Images.Explosion, bomb);
            await Task.Delay(1000);
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
