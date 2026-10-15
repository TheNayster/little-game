// Research replacement: uses recovered colour/decal data, not original shader code.
Shader "Research/RecoveredBrick" {
 Properties {
  _Color("Recovered colour", Color)=(1,1,1,1)
  _MainTex("Recovered decal", 2D)="black"{}
  _UseDecal("Use decal", Float)=0
 }
 SubShader {
  Tags {"RenderType"="Opaque"} Cull Back
  Pass {
   CGPROGRAM
   #pragma vertex vert
   #pragma fragment frag
   #include "UnityCG.cginc"
   sampler2D _MainTex; float4 _MainTex_ST; fixed4 _Color; float _UseDecal;
   struct appdata {float4 vertex:POSITION; float3 normal:NORMAL; float2 uv:TEXCOORD0;};
   struct v2f {float4 pos:SV_POSITION; float3 normal:TEXCOORD0; float2 uv:TEXCOORD1;};
   v2f vert(appdata v) {v2f o; o.pos=UnityObjectToClipPos(v.vertex); o.normal=UnityObjectToWorldNormal(v.normal); o.uv=TRANSFORM_TEX(v.uv,_MainTex); return o;}
   fixed4 frag(v2f i):SV_Target {
    fixed4 decal=tex2D(_MainTex,i.uv);
    fixed3 baseColor=lerp(_Color.rgb,decal.rgb,decal.a*_UseDecal);
    float diffuse=saturate(dot(normalize(i.normal),normalize(float3(-0.3,0.8,-0.6))));
    return fixed4(baseColor*(0.70+0.30*diffuse),1);
   }
   ENDCG
  }
 }
}
