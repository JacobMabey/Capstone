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
        public static double DeltaTime { get; private set; } = 0.0;
        public static double FPS => ((int)((1000.0 / DeltaTime) * 1000.0)) / 1000.0;


        private static long PrevTime { get; set; } = 0;

        public static void Initialize()
        {
        }

        public static void Update()
        {
            //Update Delta Time
            long now = DateTime.Now.Ticks;
            if (PrevTime == 0) PrevTime = now;
            DeltaTime = (now - PrevTime) / 10000.0f;
            PrevTime = now;
        }

        public static void Shutdown()
        {
        }
    }
}
