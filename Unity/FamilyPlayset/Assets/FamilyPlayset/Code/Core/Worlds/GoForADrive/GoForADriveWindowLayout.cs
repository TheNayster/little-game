using System;
namespace LittleWeeps.Core
{
    public readonly struct DriveWindowBox
    {
        public readonly float x,y,width,height;
        public DriveWindowBox(float x,float y,float w,float h){this.x=x;this.y=y;width=w;height=h;}
        public bool Contains(float px,float py)=>px>=x && px<=x+width && py>=y && py<=y+height;
    }
    public sealed class DriveWindowPattern
    {
        public readonly float width,height,pivotX,pivotY,scale,span;
        public readonly DriveWindowBox obstacle,reward;
        public DriveWindowPattern(float w,float h,float px,float py,float scale,float span,DriveWindowBox obstacle,DriveWindowBox reward){width=w;height=h;pivotX=px;pivotY=py;this.scale=scale;this.span=span;this.obstacle=obstacle;this.reward=reward;}
    }
    // Original ordered prefabs and semantic collider projections. Excludes the
    // broad root input collider; coordinates are relative to the source root.
    public static class GoForADriveWindowLayout
    {
        public const float PixelsPerUnit=80,ArmX=-150,Floor=-80,HandY=-40;
        public static readonly DriveWindowPattern[] Patterns={
            new DriveWindowPattern(804.000000f,260.000000f,118.632812f,9.725037f,0.415925f,10.000000f,new DriveWindowBox(-28.000000f,-1.717850f,56.000001f,88.129910f),new DriveWindowBox(192.000020f,-5.593011f,95.999908f,100.674234f)),
            new DriveWindowPattern(788.000000f,183.000000f,239.457520f,1.441406f,0.519734f,9.240000f,new DriveWindowBox(-83.999994f,3.695381f,100.000001f,54.531870f),new DriveWindowBox(192.000032f,-2.596772f,95.999932f,100.674258f)),
            new DriveWindowPattern(806.000000f,266.000000f,157.865448f,7.425903f,0.439831f,10.000000f,new DriveWindowBox(-28.000004f,-1.597995f,56.000006f,86.531895f),new DriveWindowBox(191.999983f,-3.875147f,96.000014f,100.674235f)),
            new DriveWindowPattern(785.000000f,180.000000f,252.883636f,3.418518f,0.535788f,9.240000f,new DriveWindowBox(-83.999978f,-3.915126f,99.999961f,73.907667f),new DriveWindowBox(191.999996f,-2.596756f,96.000051f,100.674235f)),
            new DriveWindowPattern(747.000000f,263.000000f,197.507446f,9.325806f,0.518835f,10.000000f,new DriveWindowBox(-61.684701f,-4.333993f,38.370824f,120.247314f),new DriveWindowBox(192.000014f,-2.397008f,95.999954f,100.674243f)),
            new DriveWindowPattern(771.000000f,179.000000f,234.028259f,6.799103f,0.530940f,9.240000f,new DriveWindowBox(-82.800021f,0.699125f,97.600003f,54.531881f),new DriveWindowBox(192.000015f,-5.593003f,95.999966f,100.674230f)),
            new DriveWindowPattern(777.000000f,349.000000f,286.067963f,4.300903f,0.580766f,10.000000f,new DriveWindowBox(-139.063215f,10.388739f,146.926427f,153.325008f),new DriveWindowBox(192.000037f,-2.556797f,95.999949f,100.674221f)),
            new DriveWindowPattern(783.000000f,235.000000f,162.913757f,2.465942f,0.459736f,10.000000f,new DriveWindowBox(-49.889521f,-2.966875f,47.935433f,96.714262f),new DriveWindowBox(192.000018f,-2.396999f,95.999967f,100.674234f)),
            new DriveWindowPattern(781.000000f,171.000000f,260.925964f,-3.151978f,0.548202f,9.240000f,new DriveWindowBox(-121.458527f,-3.077234f,161.755986f,75.879387f),new DriveWindowBox(192.000008f,-1.558063f,95.999979f,100.674246f)),
            new DriveWindowPattern(752.000000f,231.000000f,265.879242f,3.819824f,0.586518f,10.000000f,new DriveWindowBox(-116.875302f,-3.074242f,121.224621f,120.694112f),new DriveWindowBox(192.000004f,-2.033466f,96.000000f,100.674259f)),
            new DriveWindowPattern(791.000000f,240.000000f,132.660889f,2.287903f,0.433010f,10.000000f,new DriveWindowBox(-27.999998f,0.727095f,56.000002f,86.531903f),new DriveWindowBox(192.000021f,-1.550049f,96.000003f,100.674234f)),
            new DriveWindowPattern(795.000000f,178.000000f,337.596252f,3.818665f,0.623368f,10.000000f,new DriveWindowBox(-187.575036f,-3.826306f,266.612401f,97.573723f),new DriveWindowBox(191.999994f,-2.396981f,95.999986f,100.674237f)),
            new DriveWindowPattern(800.000000f,269.000000f,240.444031f,12.981781f,0.509498f,9.240000f,new DriveWindowBox(-83.999999f,-7.290889f,99.999990f,114.457026f),new DriveWindowBox(192.000011f,-9.188521f,95.999967f,100.674238f)),
            new DriveWindowPattern(811.000000f,295.000000f,156.780518f,7.150970f,0.435738f,9.240000f,new DriveWindowBox(-63.761990f,-5.416387f,87.172845f,117.425575f),new DriveWindowBox(192.000031f,-5.912607f,96.000015f,100.674250f)),
            new DriveWindowPattern(739.000000f,258.000000f,185.000549f,5.842224f,0.514611f,10.000000f,new DriveWindowBox(-56.560007f,-4.505295f,40.000008f,118.565630f),new DriveWindowBox(192.000008f,-5.592998f,95.999960f,100.674227f)),
        };
    }
}
