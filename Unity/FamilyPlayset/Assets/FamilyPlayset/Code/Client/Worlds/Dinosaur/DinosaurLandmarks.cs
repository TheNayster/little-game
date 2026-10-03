using UnityEngine;
namespace LittleWeeps.Client
{
    // Normalized, measured cushion/foot landmarks on the preserved generated sheets.
    internal static class DinosaurLandmarks
    {
        internal static Vector3 Get(string id,int frame)
        {
            if(id=="tyrannosaurus")return new[]{new Vector3(0.47773f,0.48253f,0.89064f),new Vector3(0.47926f,0.4938f,0.88613f),new Vector3(0.44797f,0.48703f,0.88613f),new Vector3(0.44173f,0.48478f,0.88839f),new Vector3(0.47535f,0.37204f,0.77114f),new Vector3(0.48301f,0.38106f,0.77565f),new Vector3(0.43639f,0.37655f,0.77339f),new Vector3(0.47245f,0.41488f,0.77339f)}[frame];
            else if(id=="triceratops")return new[]{new Vector3(0.43477f,0.48478f,0.86359f),new Vector3(0.43674f,0.48478f,0.86809f),new Vector3(0.43424f,0.48478f,0.86809f),new Vector3(0.43901f,0.48478f,0.87035f),new Vector3(0.43705f,0.40812f,0.79143f),new Vector3(0.44314f,0.40812f,0.79369f),new Vector3(0.43504f,0.40812f,0.79369f),new Vector3(0.44294f,0.47802f,0.79143f)}[frame];
            else if(id=="brachiosaurus")return new[]{new Vector3(0.58164f,0.48478f,0.92897f),new Vector3(0.58253f,0.48703f,0.92897f),new Vector3(0.55896f,0.48478f,0.92897f),new Vector3(0.55379f,0.48253f,0.92446f),new Vector3(0.59019f,0.45998f,0.90417f),new Vector3(0.58978f,0.46223f,0.90417f),new Vector3(0.57536f,0.45998f,0.90643f),new Vector3(0.5485f,0.46223f,0.90643f)}[frame];
            else if(id=="parasaurolophus")return new[]{new Vector3(0.4731f,0.47576f,0.89966f),new Vector3(0.52117f,0.48478f,0.8929f),new Vector3(0.52031f,0.48478f,0.89966f),new Vector3(0.53681f,0.48478f,0.90192f),new Vector3(0.48907f,0.39008f,0.80496f),new Vector3(0.53079f,0.39233f,0.80496f),new Vector3(0.49293f,0.39008f,0.80271f),new Vector3(0.51655f,0.48253f,0.77339f)}[frame];
            throw new System.ArgumentException("Unknown dinosaur landmark");
        }
    }
}
