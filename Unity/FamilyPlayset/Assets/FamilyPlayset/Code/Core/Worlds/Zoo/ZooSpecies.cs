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
        public string FoodName=>food==ZooFoodKind.Leaves?"leaves":food==ZooFoodKind.Hay?"hay":food==ZooFoodKind.Meat?"meat":food==ZooFoodKind.Pellets?"pellets":food==ZooFoodKind.Seaweed?"seaweed":food==ZooFoodKind.Fish?"fish":"insects";
        // Configured pretend foods, shared by buckets and optional preparation.
        public ZooFoodKind[] SnackFoods=>food==ZooFoodKind.Leaves && habitat==ZooHabitat.Land && id!="tortoise"?new[]{food,ZooFoodKind.Hay}:new[]{food};
        public bool AcceptsSnack(int kind)=>SnackFoods.Contains((ZooFoodKind)kind);
        public float SnackX=>Center+(id=="elephant"?820:210);
        public float ToolX=>Center+(id=="elephant"?540:-210);
        public float PlayPropX=>Center-400;
        // Fitted play destinations stay inside the authored habitat. Browsers
        // face the rooted foliage; aquarium animals stay on their swim route.
        public float PlayX=>id=="elephant"?ZooLayout.WaterPlayX:Center-Math.Min(habitat==ZooHabitat.Tank?250:350,Radius-60);
        public float ActivityY=>habitat==ZooHabitat.Tank?370:320;
        public string CareTool=>id=="elephant" || id=="giraffe" || id=="zebra"?"brush":habitat==ZooHabitat.Tank?"tank cloth":id=="gecko" || id=="iguana" || id=="tyrannosaurus" || id=="lion"?"habitat cloth":"rinse";
        public bool HabitatCare=>CareTool.Contains("cloth");
        public string PlayKind=>id=="elephant" || id=="penguin"?"water":habitat==ZooHabitat.Tank?"current":id=="lion" || id=="tyrannosaurus"?"toy":id=="gecko"?"shade":id=="iguana" || id=="tortoise" || id=="crocodile"?"mist":"browse";
        public string PlayName=>id=="elephant"?"Water play":id=="giraffe"?"Swaying browse branch":id=="zebra"?"Scratch log":id=="lion"?"Rolling enrichment toy":id=="brachiosaurus"?"Canopy rustle":id=="triceratops"?"Low browse patch":id=="stegosaurus"?"Fern rustle":id=="tyrannosaurus"?"Scent toy":id=="clownfish"?"Anemone bubbles":id=="blue-tang"?"Reef current":id=="zebra-shark"?"Sand current":id=="penguin"?"Splashing pool":id=="tortoise"?"Gentle garden mist":id=="gecko"?"Warm-rock shade":id=="iguana"?"Leaf mist":"Basking-bank mist";
        public string Discovery(int slot)=>id=="elephant"?(slot==0?"Garden bird":"Butterflies"):id=="giraffe"?(slot==0?"Weaver nest":"Seed pods"):id=="zebra"?(slot==0?"Grasshopper":"Striped feathers"):id=="lion"?(slot==0?"Lizard peek":"Golden beetle"):id=="brachiosaurus"?(slot==0?"Fern unfurl":"Amber sparkle"):id=="triceratops"?(slot==0?"Seed cone":"Fern snail"):id=="stegosaurus"?(slot==0?"Dragonfly":"Leaf pattern"):id=="tyrannosaurus"?(slot==0?"Footprint pebble":"Amber beetle"):id=="clownfish"?(slot==0?"Anemone shrimp":"Pearl shell"):id=="blue-tang"?(slot==0?"Reef star":"Coral shrimp"):id=="zebra-shark"?(slot==0?"Sand shell":"Buried sea star"):id=="penguin"?(slot==0?"Rock crab":"Tide shell"):id=="tortoise"?(slot==0?"Garden snail":"Seedling"):id=="gecko"?(slot==0?"Pebble beetle":"Rock crystal"):id=="iguana"?(slot==0?"Leaf insect":"Seed pod"):(slot==0?"Reed frog":"Water snail");
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
