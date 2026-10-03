using System;
using System.Linq;

namespace LittleWeeps.Core
{
    public enum ZooFoodKind { Leaves, Hay, Meat, Pellets, Seaweed, Fish, Insects }
    public enum ZooHabitat { Land, Tank, LandWater, Climb }
    // Presentation sockets and authored habitat limits live with the species,
    // while the existing authority still owns all routes and food consumption.
    public sealed class ZooSpecies
    {
        public readonly string id,name,area,activity,sound;
        public readonly int panel;
        public readonly ZooFoodKind food;
        public readonly ZooHabitat habitat;
        public readonly float size,mouthX,mouthY,speed,approachSpeed,footOffset;
        public ZooSpecies(string id,string name,string area,int panel,ZooFoodKind food,ZooHabitat habitat,float size,float mouthX,float mouthY,float speed,float approachSpeed,string activity,string sound,float footOffset=-45)
        {this.id=id;this.name=name;this.area=area;this.panel=panel;this.food=food;this.habitat=habitat;this.size=size;this.mouthX=mouthX;this.mouthY=mouthY;this.speed=speed;this.approachSpeed=approachSpeed;this.activity=activity;this.sound=sound;this.footOffset=footOffset;}
        public float Center=>1200+panel*2400;
        public float FeedY=>habitat==ZooHabitat.Tank?330:270;
        public float FeedHeight=>(FeedY-100)*.45f+footOffset+mouthY;
        public string FoodName=>food==ZooFoodKind.Leaves?"leaves":food==ZooFoodKind.Hay?"hay":food==ZooFoodKind.Meat?"meat":food==ZooFoodKind.Pellets?"pellets":food==ZooFoodKind.Seaweed?"seaweed":food==ZooFoodKind.Fish?"fish":"insects";
        public float MinY=>250;
        public float MaxY=>habitat==ZooHabitat.Climb?440:420;
        public float Radius=>id=="tortoise"?260:habitat==ZooHabitat.Tank && id!="zebra-shark"?360:600;
        public float MinX=>Center-Radius;
        public float MaxX=>Center+Radius;
    }
    public static class ZooCatalog
    {
        public const int Schema=35;
        public const string Dinosaurs="zoo-dinosaurs",Aquarium="zoo-aquarium",Reptiles="zoo-reptiles";
        public static readonly string[] Trails={ZooLayout.Savanna,Dinosaurs,Reptiles,Aquarium};
        public static readonly ZooSpecies[] All={
            new ZooSpecies("elephant","Elephant",ZooLayout.Savanna,0,ZooFoodKind.Leaves,ZooHabitat.Land,540,140,294,48,42,"Explore with trunk","elephant",-55),
            new ZooSpecies("giraffe","Giraffe",ZooLayout.Savanna,1,ZooFoodKind.Leaves,ZooHabitat.Land,480,145,330,52,42,"Browse high leaves","giraffe",-10),
            new ZooSpecies("zebra","Zebra",ZooLayout.Savanna,2,ZooFoodKind.Hay,ZooHabitat.Land,470,215,112,58,42,"Graze and flick ears","zebra"),
            new ZooSpecies("lion","Lion",ZooLayout.Savanna,3,ZooFoodKind.Meat,ZooHabitat.Land,470,200,315,40,35,"Rest and groom","lion"),
            new ZooSpecies("brachiosaurus","Brachiosaurus",Dinosaurs,0,ZooFoodKind.Leaves,ZooHabitat.Land,560,217,497,38,35,"Browse the canopy","brachiosaurus"),
            new ZooSpecies("triceratops","Triceratops",Dinosaurs,1,ZooFoodKind.Leaves,ZooHabitat.Land,520,158,190,36,32,"Crop low foliage","triceratops"),
            new ZooSpecies("stegosaurus","Stegosaurus",Dinosaurs,2,ZooFoodKind.Leaves,ZooHabitat.Land,520,232,277,35,32,"Browse and look around","stegosaurus"),
            new ZooSpecies("tyrannosaurus","T. rex",Dinosaurs,3,ZooFoodKind.Meat,ZooHabitat.Land,550,141,322,46,38,"Look and sniff","tyrannosaurus"),
            new ZooSpecies("clownfish","Clownfish",Aquarium,0,ZooFoodKind.Pellets,ZooHabitat.Tank,290,120,195,46,40,"Hover by the anemone","water"),
            new ZooSpecies("blue-tang","Blue tang",Aquarium,1,ZooFoodKind.Seaweed,ZooHabitat.Tank,330,77,203,58,45,"Inspect the reef","water"),
            new ZooSpecies("zebra-shark","Zebra shark",Aquarium,2,ZooFoodKind.Fish,ZooHabitat.Tank,610,182,340,42,35,"Rest on sand","water"),
            new ZooSpecies("penguin","African penguin",Aquarium,3,ZooFoodKind.Fish,ZooHabitat.LandWater,400,119,312,45,35,"Preen and swim","penguin"),
            new ZooSpecies("tortoise","Galapagos tortoise",Reptiles,0,ZooFoodKind.Leaves,ZooHabitat.Land,370,164,219,20,22,"Graze and rest","tortoise"),
            new ZooSpecies("gecko","Leopard gecko",Reptiles,1,ZooFoodKind.Insects,ZooHabitat.Land,320,81,157,33,30,"Peek and inspect","gecko"),
            new ZooSpecies("iguana","Green iguana",Reptiles,2,ZooFoodKind.Leaves,ZooHabitat.Climb,450,150,286,34,30,"Warm rock and climb","iguana"),
            new ZooSpecies("crocodile","Nile crocodile",Reptiles,3,ZooFoodKind.Fish,ZooHabitat.LandWater,590,232,379,35,30,"Bask and float","crocodile")
        };
        public static ZooSpecies Get(string id)=>All.Single(s=>s.id==id);
        public static bool Trail(string area)=>Trails.Contains(area);
        public static string Name(string area)=>area==ZooLayout.Savanna?"Savanna":area==Dinosaurs?"Dinosaur Valley":area==Aquarium?"Aquarium":"Reptile Garden";
        public static string Next(string area)=>Trails[(Array.IndexOf(Trails,area)+1)%Trails.Length];
        public static string Previous(string area)=>Trails[(Array.IndexOf(Trails,area)+Trails.Length-1)%Trails.Length];
        public static float EntranceX(string target)=>Array.IndexOf(Trails,target)%2==0?650:1750;
        public static bool Gate(string from,string target,out float x,out float arrival)
        {
            x=200;arrival=420;
            if(from==ZooLayout.Entrance && Trail(target)){x=EntranceX(target);return true;}
            if(!Trail(from))return false;
            if(target==ZooLayout.Entrance){arrival=ZooLayout.EntranceArrivalX;return true;}
            if(target==Next(from)){x=9200;return true;}
            if(target==Previous(from)){arrival=9000;return true;}
            return false;
        }
    }
}
