using System;
namespace LittleWeeps.Core
{
    // Projected from saf_Park serialized pegs and slide path; scene SHA256
    // 3b428b542b5d160808c5dba39c2a3301bd08925c89fe6fd00553c68a324ce7e3.
    public static class SpideyHQLayout
    {
        public const double ServeDuration=17d/12, SlideDuration=1.25;
        public static readonly float[] SeatX={3058.475f,1415.028f,1904.629f,1339.705f,3130.475f,1831.644f};
        public static readonly float[] SeatY={265.806f,204.586f,279.410f,204.586f,265.873f,279.410f};
        public static readonly float[] SlideX={2575.740f,2590.184f,2606.421f,2616.151f,2607.089f,2596.493f,2583.116f};
        public static readonly float[] SlideY={367.188f,361.264f,323.953f,266.226f,189.565f,218.050f,135.727f};
        public static readonly float[] PizzaX={1652.936f,1555.152f,1481.859f};
        public static readonly float[] PizzaY={153.481f,151.761f,168.565f};
        public static readonly float[] BalloonX={2872.615f,2798.154f,2723.692f};
        public static readonly float[] BalloonY={162.932f,167.636f,171.667f};
        public static readonly float[] SlideQueueX={2571.536f,2526.689f,2508.985f};
        public static readonly float[] SlideQueueY={218.555f,238.750f,248.548f};
        // The fourth family slot extends the final authored queue segment.
        public static float Queue(string station,int index,bool y)
        {var a=station=="pizza"?(y?PizzaY:PizzaX):station=="balloon"?(y?BalloonY:BalloonX):(y?SlideQueueY:SlideQueueX);return index<3?a[index]:a[2]+(a[2]-a[1]);}
        public static float SlidePoint(double progress,bool y)
        {var a=y?SlideY:SlideX;double t=Math.Clamp(progress,0,1)*(a.Length-1);int i=Math.Min(a.Length-2,(int)t);return a[i]+(a[i+1]-a[i])*(float)(t-i);}
    }
}
