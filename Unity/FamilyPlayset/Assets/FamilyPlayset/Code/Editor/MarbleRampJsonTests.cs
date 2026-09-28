using System;
using System.IO;
using LittleWeeps.Core;
using UnityEngine;

namespace LittleWeeps.EditorTools
{
    public static class MarbleRampJsonTests
    {
        // Exercise Unity's actual serializer, which differs from the portable
        // core suite's System.Text.Json writer/reader at double boundaries.
        public static void Run()
        {
            var failures=0;
            for(var surface=0;surface<3;surface++)for(var variant=0;variant<4;variant++){
                var course=new RampCourse{surface=surface};
                if(variant==1)MarbleRamps.Move(course,0,250,100);
                if(variant==2)MarbleRamps.Move(course,2,850,240);
                if(variant==3)MarbleRamps.Move(course,5,680,440);
                var duration=MarbleRamps.Path(course).Duration;
                foreach(var time in new[]{duration/2,duration}){
                    var tray=new RampTray{current=new RampState{course=course,released=true,elapsed=time}};
                    var json=JsonUtility.ToJson(tray);var copy=JsonUtility.FromJson<RampTray>(json);
                    try{MarbleRamps.Validate(copy);}
                    catch(Exception e){failures++;Debug.Log("Ramp JSON failure: "+e.Message+" source="+time.ToString("R")+" restored="+copy.current.elapsed.ToString("R")+" duration="+duration.ToString("R")+" json="+json);}
                }
            }
            Debug.Log("Ramp JSON round trips: 24; failures: "+failures);
            if(failures>0)throw new InvalidDataException("Ramp JSON round-trip failures: "+failures);
        }
    }
}
