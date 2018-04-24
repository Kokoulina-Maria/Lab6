using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Lab6.Mob;

namespace Lab6
{
    public partial class Form1 : Form
    {
        static int xLim = 23, yLim = 10;
        const int gap = 50;
        enum Images { Background, Enemy, Player, Bomb, Rock, Explosion, Brick };
        Game game;
        static int playerOffsetX = 25;
        static int playerOffsetY = 25;

        CancellationTokenSource tokenSource = new CancellationTokenSource();

        static List<Image> mapBitmaps = new List<Image>();

        private object syncObj = new object(); // For locking...
        private bool paused = false;
        private static Mutex mut = new Mutex();

        Thread refreshThread;

        public Form1()
        {
            InitializeComponent();
            LoadBitmaps();
            this.Width = xLim * gap;
            this.Height = yLim * gap;
            game = new Game();
            CancellationToken token = tokenSource.Token;
            Monitor.Enter(syncObj);
            refreshThread = new Thread(RefreshLoop);
            refreshThread.Start();
        }


        private void LoadBitmaps()
        {
            mapBitmaps.Add(Image.FromFile("Images/Sprites/ground.jpg")); // Background
            mapBitmaps.Add(Image.FromFile("Images/Sprites/enemy.png")); // Enemy
            mapBitmaps.Add(Image.FromFile("Images/Sprites/enemy.png")); // Player
            mapBitmaps.Add(Image.FromFile("Images/Sprites/bomb.png")); // Bomb
            mapBitmaps.Add(Image.FromFile("Images/Sprites/border.jpg")); // Rock
            mapBitmaps.Add(Image.FromFile("Images/Sprites/fire.jpg")); // Explosion
        }

        private void Pause()
        {
            if (!paused)
            {
                refreshThread.Suspend();
                game.Pause();
                //Monitor.Enter(syncObj);
                paused = true;
            }
            else
                Resume();
        }

        private void Resume()
        {
            if (paused)
            {
                paused = false;
                refreshThread.Resume();
                game.Resume();
                //Monitor.Exit(syncObj);
            }
        }

        // Drawing
        void RefreshLoop()
        {
            while (true)
            {
                //lock (syncObj) { }
                canvas.Invalidate();
                Thread.Sleep(100);
            }
        }

        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            if (game.Map != null && mapBitmaps.Count > 0)
            {
                DrawMap(e.Graphics);
                DrawEnemies(e.Graphics);
                DrawPlayer(e.Graphics);
            }
        }

        public void DrawMap(Graphics drawing)
        {
            for (int row = 0; row < yLim; row++)
                for (int col = 0; col < xLim; col++)
                {
                    if (game.Map[row, col] == (int)Images.Bomb)
                        drawing.DrawImage(mapBitmaps[(int)Images.Background], new Rectangle(gap * col, gap * row, gap, gap));
                    drawing.DrawImage(mapBitmaps[game.Map[row, col]], new Rectangle(gap * col, gap * row, gap, gap));
                }
        }

        public void DrawEnemies(Graphics drawing)
        {
            foreach (Enemy enemy in game.Enemies)
                if (enemy.IsAlive)
                    drawing.DrawImage(mapBitmaps[(int)Images.Enemy], new Rectangle(enemy.Position.X, enemy.Position.Y, gap, gap));
        }

        public void DrawPlayer(Graphics drawing)
        {
            Player player = game.Player;
            if (player.IsAlive)
                drawing.DrawImage(mapBitmaps[(int)Images.Player], new Rectangle(player.Position.X - playerOffsetX, player.Position.Y - playerOffsetY, gap, gap));
            else
                drawing.DrawImage(mapBitmaps[(int)Images.Explosion], new Rectangle(player.Position.X - playerOffsetX, player.Position.Y - playerOffsetY, gap, gap));
        }

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            Player player = game.Player;
            switch (e.KeyCode)
            {
                case Keys.P:
                    Pause();
                    break;
                case Keys.Left:
                    player.MoveLeft();
                    break;
                case Keys.Right:
                    player.MoveRight();
                    break;
                case Keys.Up:
                    player.MoveUp();
                    break;
                case Keys.Down:
                    player.MoveDown();
                    break;
                case Keys.Space:
                    if (player.PlantBomb())
                        ShowBomb();
                    break;
            }
        }

        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            Player player = game.Player;
            switch (e.KeyCode)
            {
                case Keys.Left:
                    game.Player.Stop(Directions.Left);
                    break;
                case Keys.Right:
                    game.Player.Stop(Directions.Right);
                    break;
                case Keys.Up:
                    game.Player.Stop(Directions.Up);
                    break;
                case Keys.Down:
                    game.Player.Stop(Directions.Down);
                    break;
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            refreshThread.Abort();
            game.Exit();
        }

        /// <summary>
        /// Draws a bomb under the Player 
        /// when bomb is planted
        /// </summary>
        private void ShowBomb()
        {
            int x = (int)(game.Player.Position.X / gap);
            int y = (int)(game.Player.Position.Y / gap);
            game.Map[y, x] = (int)Images.Bomb;
        }
    }
}
