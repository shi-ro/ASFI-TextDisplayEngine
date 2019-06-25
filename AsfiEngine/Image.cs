using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsfiEngine
{
    public class Image
    {
        public string[][,] Frames { get; private set; }
        public string[,] CurrentFrame { get; set; }
        public int AnimationDelay { get; private set; }
        public bool Advance { get; set; }
        private int _currentFrame = 0;
        private long _lastSwitch = 0;

        public Image(string[,] frame)
        {
            Frames = new string[1][,];
            Frames[0] = frame;
            CurrentFrame = frame;
            Advance = true;
        }

        public Image(string[][,] frames, int delay)
        {
            Frames = frames;
            AnimationDelay = delay;
            CurrentFrame = frames[0];
            Advance = true;
        }

        public void TryAdvance()
        {
            if(Frames.Count()<=0)
            {
                return;
            }

            long currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            if (_lastSwitch + AnimationDelay > currentTime)
            {
                _lastSwitch = currentTime;
                _currentFrame += 1;
                _currentFrame %= Frames.Count();
                CurrentFrame = Frames[_currentFrame];
                Advance = true;
            }
        }
    }
}
