Shader "LittleWeeps/CakeColor"
{
 Properties { [PerRendererData] _MainTex("Cake art",2D)="white"{} }
 SubShader {
 Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "CanUseSpriteAtlas"="True"}
 Cull Off Lighting Off ZWrite Off ZTest [unity_GUIZTestMode] Blend SrcAlpha OneMinusSrcAlpha
 Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #include "UnityCG.cginc"
 struct Input {float4 vertex:POSITION;float4 color:COLOR;float2 uv:TEXCOORD0;};
 struct Output {float4 vertex:SV_POSITION;fixed4 color:COLOR;float2 uv:TEXCOORD0;};
 sampler2D _MainTex;
 Output vert(Input v){Output o;o.vertex=UnityObjectToClipPos(v.vertex);o.color=v.color;o.uv=v.uv;return o;}
 // Keep the source's shading and alpha, without mixing yellow sponge pigment
 // into a child's selected blue or purple batter color.
 fixed4 frag(Output i):SV_Target{fixed4 art=tex2D(_MainTex,i.uv);fixed light=dot(art.rgb,fixed3(.2126,.7152,.0722));return fixed4(light,light,light,art.a)*i.color;}
 ENDCG
 }
 }
}
