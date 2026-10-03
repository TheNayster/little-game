Shader "LittleWeeps/ColoringInk"
{
 Properties { [PerRendererData] _MainTex("Line art",2D)="white"{} _Regions("Regions",2D)="black"{} _Palette("Colors",2D)="white"{} }
 SubShader {
 Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="False"}
 Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct Input {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
 struct Output {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;};
 sampler2D _MainTex,_Regions,_Palette;
 Output vert(Input v){Output o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=v.uv;return o;}
 fixed4 frag(Output i):SV_Target{fixed4 outline=tex2D(_MainTex,i.uv);float id=floor(tex2D(_Regions,i.uv).a*255+.5);fixed4 paint=tex2D(_Palette,float2((id+.5)/256,.5));return fixed4(outline.rgb*paint.rgb,outline.a)*i.color;}
 ENDCG
 }
 }
}
