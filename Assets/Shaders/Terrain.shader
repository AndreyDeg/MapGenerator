Shader "Terrain" {
	Properties {
		 _NoiseTex ("Noise", 2D) = "white" {}
		 _NoiseColorShift ("Noise color shift", Vector) = (0.0, 0.0, 0.0, 0.0)
		 _OutlineThickness ("Outline width", Range (0.0, 0.03)) = 0.0028
		 _OutlineBrightness ("Outline brightness", Range (0.0, 1.0)) = 0.41
		 _Brightness ("Brightness", Range (1.0, 5.0)) = 1.0
		 _SpotLightIntensity ("Spot light intensity", Range(0.0, 2.0)) = 0.641
		 _SpotLightRadius ("Spot light radius", Range(0.0, 5.0)) = 2.59
		 _SpotLightColor ("Spot Color", Color) = (0.97, 0.67, 0.13, 1.0)
		 _MagicScale ("Magic scale", Range(1, 2)) = 2
	}

	SubShader {
		LOD 200
		Tags { "RenderType"="Opaque" "Queue" = "Geometry" }
		Pass { 
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_fwdbase
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			
			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float3 wpos : TEXCOORD0;
				float4 color : COLOR;
				UNITY_FOG_COORDS(1)
			};

			float _Brightness;
			float _SpotLightIntensity;
			float _SpotLightRadius;
			float4 _SpotLightColor;
			sampler2D _NoiseTex;
			float4 _NoiseColorShift;
			float _MagicScale;
			
			uniform float4 _PlayerPosition;
		
			v2f vert (appdata_t v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float4 wpos = mul(unity_ObjectToWorld, v.vertex);
				o.wpos.x = wpos.x / 12.8 * _MagicScale-0.0001; // (Texture size = 64) * (Block size = 0.2) * (MAGIC NUMBER = 2)
				o.wpos.y = wpos.y / 12.8 * _MagicScale+0.0001;
				o.wpos.z = wpos.z / 12.8 * _MagicScale+0.0001;

				float3 vec = _PlayerPosition.xyz - wpos.xyz;
				float srad = _SpotLightRadius * _SpotLightRadius;
				float spot = 1.0 - min(dot(vec, vec), srad) / srad;

				o.color = v.color + spot * _SpotLightIntensity * _SpotLightColor;
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}
				
			fixed4 frag (v2f i) : SV_Target {
				float4 tex = tex2D(_NoiseTex, i.wpos.xz);
				
				float4 col = i.color * _Brightness;
				col.r = col.r + (tex.r + _NoiseColorShift.x) * _Brightness;
				col.g = col.g + (tex.g + _NoiseColorShift.y) * _Brightness;
				col.b = col.b + (tex.b + _NoiseColorShift.z) * _Brightness;
				col.a = 1;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
	SubShader {
		LOD 100
		Tags { "RenderType"="Opaque" "Queue" = "Geometry-1" }
		Pass { 
			Tags { "LightMode" = "ForwardBase" }
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#pragma multi_compile_fwdbase
			
			#include "UnityCG.cginc"
			#include "Lighting.cginc"
			
			struct appdata_t {
				float4 vertex : POSITION;
				float4 color : COLOR;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				float3 wpos : TEXCOORD0;
				float4 color : COLOR;
				UNITY_FOG_COORDS(1)
			};

			float _Brightness;
			float _SpotLightIntensity;
			float _SpotLightRadius;
			float4 _SpotLightColor;
			sampler2D _NoiseTex;
			float4 _NoiseColorShift;
			float _MagicScale;
			
			uniform float4 _PlayerPosition;
		
			v2f vert (appdata_t v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				float4 wpos = mul(unity_ObjectToWorld, v.vertex);
				o.wpos.x = wpos.x / 12.8 * _MagicScale-0.0001; // (Texture size = 64) * (Block size = 0.2) * (MAGIC NUMBER = 2)
				o.wpos.y = wpos.y / 12.8 * _MagicScale+0.0001;
				o.wpos.z = wpos.z / 12.8 * _MagicScale+0.0001;

				float3 vec = _PlayerPosition.xyz - wpos.xyz;
				float srad = _SpotLightRadius * _SpotLightRadius;
				float spot = 1.0 - min(dot(vec, vec), srad) / srad;

				o.color = v.color + spot * _SpotLightIntensity * _SpotLightColor;
				UNITY_TRANSFER_FOG(o, o.vertex);
				return o;
			}
				
			fixed4 frag (v2f i) : SV_Target {
				float4 tex = tex2D(_NoiseTex, i.wpos.xz);
				
				float4 col = i.color * _Brightness;
				col.r = col.r + (tex.r + _NoiseColorShift.x) * _Brightness;
				col.g = col.g + (tex.g + _NoiseColorShift.y) * _Brightness;
				col.b = col.b + (tex.b + _NoiseColorShift.z) * _Brightness;
				col.a = 1;
				UNITY_APPLY_FOG(i.fogCoord, col);
				return col;
			}
			ENDCG
		}
	}
    FallBack "Diffuse"
}