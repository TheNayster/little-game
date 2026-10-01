using System;
namespace LittleWeeps.Core
{
    // Temporary exclusive equipment leases use existing fixture IDs. No new
    // inventory copies or durable vehicle positions: leaving parks that vehicle.
    public static class ParkWheels
    {
        public const float MinX=200, MaxX=4600;
        public static int Index(string id){for(var i=0;i<8;i++)if(id==Id(i))return i;return -1;}
        public static string Id(int i)=>"park-wheels-"+(i<4?"bike-":"scooter-")+(i%4);
        public static bool Usable(string id)=>Index(id)>=0;
        public static float Lane(string id)=>85+(Index(id)%4)*18;
        public static float ParkX(int i)=>420+i*240;
        public static bool Valid(SoloPlayer p)=>p.zone=="park" && p.x>=MinX && p.x<=MaxX && p.y==Lane(p.fixture);
        public static WalkPoint Floor(SoloPlayer p,float x,float y)=>Usable(p.fixture)?new WalkPoint(Math.Max(MinX,Math.Min(MaxX,x)),Lane(p.fixture)):new WalkPoint(x,y);
        public static WalkPoint Step(SoloPlayer p,float x,float y,WalkInput input,float dt,int schema)
        {
            if(!Usable(p.fixture))return Walking.Step(x,y,input,dt,WorldLayout.MinX(p.zone,schema)+40,WorldLayout.MaxX(p.zone,schema)-40);
            if(input==null || input.mode==WalkMode.Stop)return Floor(p,x,y);
            var projected=input.Copy();projected.y=input.mode==WalkMode.Direction?0:Lane(p.fixture);
            return Floor(p,Walking.Step(x,Lane(p.fixture),projected,dt,MinX,MaxX).X,Lane(p.fixture));
        }
    }
}
