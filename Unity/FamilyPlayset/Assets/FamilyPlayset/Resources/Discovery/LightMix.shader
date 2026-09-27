Shader "LittleWeeps/LightMix"
{
 Properties { _MainTex("Texture",2D)="white"{} _Lights("Lamps",Float)=0 }
 SubShader {
 Tags {"Queue"="Transparent" "RenderType"="Transparent"}
 Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct Input {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
 struct Output {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;};
 float _Lights;
 Output vert(Input v){Output o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=v.uv;return o;}
 float spot(float2 p,float2 center){return 1-smoothstep(.275,.31,length(p-center));}
 fixed4 frag(Output i):SV_Target{
 float2 p=float2(i.uv.x*1.8,i.uv.y);
 float r=fmod(floor(_Lights),2)*spot(p,float2(.69,.6));
 float g=fmod(floor(_Lights/2),2)*spot(p,float2(1.11,.6));
 float b=fmod(floor(_Lights/4),2)*spot(p,float2(.9,.36));
 float3 light=saturate(float3(r,g,b));
 float2 q=abs(i.uv-.5)-float2(.47,.45);float edge=length(max(q,0))+min(max(q.x,q.y),0)-.03;
 float alpha=1-smoothstep(-.002,.002,edge);
 float rim=smoothstep(-.025,-.018,edge);
 float3 bg=lerp(float3(.035,.075,.13),float3(.09,.2,.28),i.uv.y);
 float3 result=lerp(max(bg,light),float3(.31,.58,.59),rim);
 return fixed4(result,alpha)*i.color;
 }
 ENDCG
 }
 }
}
