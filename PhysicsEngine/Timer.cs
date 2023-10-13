using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media;

namespace PhysicsEngine
{
    public static class Timer
    {
        private static double DeltaTimeRaw { get; set; } = 0.0;
        public static double DeltaTime { get; private set; } = 0.0;

        private static double timeScale = 1.0;
        public static double TimeScale
        {
            get => IsPaused ? 0.0 : timeScale;
            set
            {
                timeScale = value;
                if (timeScale < 0.0) timeScale = 0.0;
                if (timeScale > TIMESCALE_MAX) timeScale = TIMESCALE_MAX;
            }
        }
        private static readonly double TIMESCALE_MAX = 2.0;

        public static bool IsPaused { get; set; } = false;

        public static double FPS => ((int)((1000.0 / DeltaTimeRaw) * 1000.0)) / 1000.0;


        private static long PrevTime { get; set; } = 0;

        public static void Initialize()
        {
        }

        public static void Update()
        {
            //Update Delta Time
            long now = DateTime.Now.Ticks;
            if (PrevTime == 0) PrevTime = now;
            DeltaTimeRaw = ((now - PrevTime) / 10000.0f);
            DeltaTime = DeltaTimeRaw * TimeScale;
            PrevTime = now;
        }

        public static void Shutdown()
        {
        }
    }
}
