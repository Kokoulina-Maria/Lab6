using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace UWPGame
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        static int xLim = 23, yLim = 10;
        const int gap = 50;
        enum Images {Background, Enemy, Player, Bomb, Rock, Explosion, Brick};
        Game game;
        static List<CanvasBitmap> mapBitmaps = new List<CanvasBitmap>();

        static int playerOffsetX = 25;
        static int playerOffsetY = 25;
        CancellationTokenSource tokenSource = new CancellationTokenSource();
        Thread thread = null;

        private object syncObj = new object(); // For locking...
        private bool paused = false;
        private static Mutex mut = new Mutex();

        public MainPage()
        {
            this.InitializeComponent();
            this.Width = xLim * gap;
            this.Height = yLim * gap;
            CoreWindow.GetForCurrentThread().KeyDown += MainPage_KeyDown;
            CoreWindow.GetForCurrentThread().KeyUp += MainPage_KeyUp;
            game = new Game();
            CancellationToken token = tokenSource.Token;
            Monitor.Enter(syncObj);
            RefreshLoop(token);
           //thread = new Thread(ThreadRefreshLoop);
            //thread.Priority = ThreadPriority.Highest;
        }

        // Drawing
        async Task RefreshLoop(CancellationToken token)
        {
            thread = Thread.CurrentThread;
            thread.Name = "Refresh";
            while (true)
            {
                lock (syncObj) {}
                if (token.IsCancellationRequested)
                    break;
                canvas.Invalidate();
                await Task.Delay(100);
            }
        }

        private void Pause()
        {
            if (paused == false)
            {
                Monitor.Enter(syncObj);
                paused = true;
            }
        }

        private void Resume()
        {
            if (paused)
            {
                paused = false;
                Monitor.Exit(syncObj);
            }
        }

        private void Canvas_Draw(CanvasControl sender, CanvasDrawEventArgs args)
        {
            //args.DrawingSession.DrawText("wehfuwe", 100, 100, Colors.Black);
            if (game.Map != null && mapBitmaps.Count > 0)
            {
                DrawMap(args.DrawingSession);
                DrawEnemies(args.DrawingSession);
                DrawPlayer(args.DrawingSession);
            }
        }

        public void DrawMap(CanvasDrawingSession drawing)
        {
            for (int row = 0; row < yLim; row++)
                for (int col = 0; col < xLim; col++)
                {
                    if (game.Map[row, col] == (int)Images.Bomb)
                        drawing.DrawImage(mapBitmaps[(int)Images.Background], new Rect(gap * col, gap * row, gap, gap));
                    drawing.DrawImage(mapBitmaps[game.Map[row, col]], new Rect(gap * col, gap * row, gap, gap));
                }
        }

        public void DrawEnemies(CanvasDrawingSession drawing)
        {
            foreach (Enemy enemy in game.Enemies)
                if (enemy.IsAlive)
                    drawing.DrawImage(mapBitmaps[(int)Images.Enemy], new Rect(enemy.Position.X, enemy.Position.Y, gap, gap));
        }

        public void DrawPlayer(CanvasDrawingSession drawing)
        {
            Player player = game.Player;
            if (player.IsAlive)
                drawing.DrawImage(mapBitmaps[(int)Images.Player], new Rect(player.Position.X - playerOffsetX, player.Position.Y - playerOffsetY, gap, gap));
            else
                drawing.DrawImage(mapBitmaps[(int)Images.Explosion], new Rect(player.Position.X - playerOffsetX, player.Position.Y - playerOffsetY, gap, gap));
        }

        /// <summary>
        /// Draws a bomb under the Player 
        /// when bomb is planted
        /// </summary>
        private void ShowBomb()
        {
            int x = (int)(game.Player.Position.X/gap);
            int y = (int)(game.Player.Position.Y/gap);
            game.Map[y, x] = (int)Images.Bomb;
        }

        // Upload resources
        private void canvas_CreateResources(CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            args.TrackAsyncAction(CreateResources(sender).AsAsyncAction());
        }

        async Task CreateResources(CanvasControl sender)
        {
            //enum Images {Background, Enemy, Player, Bomb, Rock, Explosion};
            try
            {
                mapBitmaps.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Images/Sprites/ground.jpg")
                )); // Background
                mapBitmaps.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Images/Sprites/enemy.png")
                )); // Enemy
                mapBitmaps.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Assets/StoreLogo.png")
                )); // Player
                mapBitmaps.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Images/Sprites/bomb.png")
                )); // Bomb
                mapBitmaps.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Images/Sprites/border.jpg")
                )); // Rock
                mapBitmaps.Add(await CanvasBitmap.LoadAsync(sender, new Uri("ms-appx:///Images/Sprites/fire.jpg")
                )); // Explosion
            }
            catch (Exception e)
            {
                var mb = new MessageDialog(e.Message);
                await mb.ShowAsync();
            }
        }
       
        // Handle movements
        private void MainPage_KeyDown(CoreWindow sender, KeyEventArgs args)
        {
            Player player = game.Player;
            VirtualKey virtualKey = args.VirtualKey;
            switch (virtualKey)
            {
                case VirtualKey.P:
                    //if (paused)
                    //    Resume();
                    //else
                    Pause();
                    break;
                case VirtualKey.Left:
                case VirtualKey.GamepadDPadLeft:
                    player.MoveLeft();
                    break;
                case VirtualKey.Right:
                case VirtualKey.GamepadDPadRight:
                    player.MoveRight();
                    break;
                case VirtualKey.Up:
                case VirtualKey.GamepadDPadUp:
                    player.MoveUp();
                    break;
                case VirtualKey.Down:
                case VirtualKey.GamepadDPadDown:
                    player.MoveDown();
                    break;
                case VirtualKey.Space:
                case VirtualKey.GamepadB:
                    if(player.PlantBomb())
                        ShowBomb();
                    break;
            }
        }


        private void MainPage_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
             Player player = game.Player;
            VirtualKey virtualKey = args.VirtualKey;
            switch (virtualKey)
            {
                case VirtualKey.Left:
                case VirtualKey.GamepadDPadLeft:
                    game.Player.Stop(Directions.Left);
                    break;
                case VirtualKey.Right:
                case VirtualKey.GamepadDPadRight:
                    game.Player.Stop(Directions.Right);
                    break;
                case VirtualKey.Up:
                case VirtualKey.GamepadDPadUp:
                    game.Player.Stop(Directions.Up);
                    break;
                case VirtualKey.Down:
                case VirtualKey.GamepadDPadDown:
                    game.Player.Stop(Directions.Down);
                    break;
            }
        }

        private void canvas_Unloaded(object sender, RoutedEventArgs e)
        {
            tokenSource.Cancel();
            game.Exit();
        }
    }
}
