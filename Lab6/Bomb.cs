using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;

namespace Lab6
{
   public class Bomb
    {
        public bool Explosion { get; set; }
        public bool IsUsed { get; set; }
        public int Radius { get; set; }
        public Point Position { get; set; }

        int timer;
        CancellationTokenSource tokenSource = new CancellationTokenSource();

        public Bomb(int timer, Point position)
        {
            this.timer = timer;
            Explosion = false;
            Position = position;
            IsUsed = false;
            Radius = 3;
            
            CancellationToken token = tokenSource.Token;
            Tick(token);
        }

        async Task Tick(CancellationToken token)
        {
            while (true)
            {
                if (token.IsCancellationRequested)
                    break;
                timer -= 1;
                if (timer <= 0)
                {
                    Boom();
                    break;
                }
                await Task.Delay(200);
            }
        }

        void Boom()
        {
            Explosion = true;
            tokenSource.Cancel();
        }

    }
}
