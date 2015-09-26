//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "TexNormalSplat"
{
	Properties
	{
		_hmap ("hmap", 2D) = "bump" {}
		_rndmap ("rndmap", 2D) = "black" {}
		_waterLevel ("waterLevel", Float) = 0
		_frame ("frame", Float) = 0

		// ---

		_planetRadius ("planetRadius", Float) = 16.0
		_atmosRadius ("atmosRadius", Float) = 17.0
		_temperature ("temperature", Float) = 0.0
		_blendLut ("blendLut", 2D) = "black" {}

		_waterTex ("waterTex", 2D) = "black" {}
		_waterNormal ("waterNormal", 2D) = "black" {}
		_tilingWater ("tilingWater", Float) = 1.0

		_snowTex ("snowTex", 2D) = "black" {}
		_tilingSnow ("tilingSnow", Float) = 1.0

		_diffuseBase ("diffuseBase", 2D) = "black" {}
		_tilingBase ("tilingBase", Float) = 1.0

		_diffuse1 ("diffuse1", 2D) = "black" {}
		_tiling1 ("tiling1", Float) = 1.0

		_diffuse2 ("diffuse2", 2D) = "black" {}
		_tiling2 ("tiling2", Float) = 1.0

		_diffuse3 ("diffuse3", 2D) = "black" {}
		_tiling3 ("tiling3", Float) = 1.0

		_diffuse4 ("diffuse4", 2D) = "black" {}
		_tiling4 ("tiling4", Float) = 1.0

		// ---

		_sunDir ("sunDir", Vector) = (0, 1, 0)
		_sunIntensity ("sunIntensity", Float) = 1.0
		_camPos ("camPos", Vector) = (0, 0, 0)				// The camera's current position
		_color1 ("color1", Color) = (0, 0.1, 0.5, 1.0)
		_color2 ("color2", Color) = (0.5, 0.3, 0, 1.0)
		_horizonHeight ("horzHeight", Float) = 0.55
		_horizonIntensity ("horzInt", Float) = 2.0
		_horizonPower ("horzPower", Float) = 5.0
		_fogIntensity ("fogIntensity", Float) = 2.0
		_fogMaxAlpha ("fogMaxAlpha", Range(0, 1)) = 0.90
		_fogHeight ("fogHeight", Range(0, 1)) = 0.3
		_fogNear ("fogNear", Float) = 10.0
		_fogFar ("fogFar", Float) = 100.0
	}

	SubShader
	{
		Tags { "Queue"="Geometry" "RenderType"="Opaque" }
		LOD 200

		Pass
		{

			CGPROGRAM

			#pragma target 3.0
			#pragma vertex vert
			#pragma fragment frag
			#pragma exclude_renderers xbox360

			#include "UnityCG.cginc"

			uniform sampler2D _hmap;
			uniform sampler2D _rndmap;
			uniform float _waterLevel;
			uniform float _frame;

			uniform float _planetRadius;
			uniform float _atmosRadius;
			uniform float _temperature;

			// 256x256 lut with 4 channels, one diffuse blending weight per channel
			uniform sampler2D _blendLut;

			//
			// textures blend
			//

			uniform sampler2D _waterTex;		// every planet has a water texture, that may be modulated or not according to the height and temperature
			uniform sampler2D _waterNormal;
			uniform float _tilingWater;

			uniform sampler2D _snowTex;			// every planet has a snow texture, that may be modulated or not according to the height and temperature
			uniform float _tilingSnow;

			uniform sampler2D _diffuseBase;	// the complementar base texture (when no other texture fits, will complete with this one)
			uniform float _tilingBase;

			uniform sampler2D _diffuse1;		// lut-based texture
			uniform float _tiling1;

			uniform sampler2D _diffuse2;		// lut-based texture
			uniform float _tiling2;

			uniform sampler2D _diffuse3;		// lut-based texture
			uniform float _tiling3;

			uniform sampler2D _diffuse4;		// lut-based texture
			uniform float _tiling4;

			uniform float3 _sunDir;
			uniform float _sunIntensity;
			uniform float3 _camPos;
			uniform float3 _color1;
			uniform float3 _color2;
			uniform float _horizonHeight;
			uniform float _horizonIntensity;
			uniform float _horizonPower;
			uniform float _fogIntensity;
			uniform float _fogMaxAlpha;
			uniform float _fogHeight;
			uniform float _fogNear;
			uniform float _fogFar;

			struct appdata
			{
				float4 vertex : SV_POSITION;
				float3 normal : NORMAL;
				float4 texcoord : TEXCOORD0;
				float4 texcoord1 : TEXCOORD1;
				float4 tangent : TANGENT;
			};

			struct vtx_out
			{
				float4 position : SV_POSITION;
				float3 normal;								// calculated in vertex shader
				float2 uv : TEXCOORD0;						// heightmap uv coords
				float2 uvvol : TEXCOORD1;					// source splat textures flat uv coords
				float3 vol : TEXCOORD2;						// patch volume flat 3d coords will come as normal
				float3 pixelPos;
				float3 tangent;
				float3 binormal;
			};

			vtx_out vert(appdata v)
			{
				vtx_out OUT;

				OUT.position = mul(UNITY_MATRIX_MVP, v.vertex);
				OUT.normal = normalize(v.vertex.xyz);								// normal is calculated by vertex displacement from the object's origin
				OUT.uv = v.texcoord.st;
				OUT.vol = v.normal;													// not really the normal, but the patch volume 3d coords is passed using this parm slot
				OUT.uvvol = v.texcoord1.st;
				OUT.tangent = v.tangent.xyz;
				OUT.binormal =  -cross(OUT.normal, OUT.tangent) * v.tangent.w;

				OUT.pixelPos = v.vertex.xyz;

				return OUT;
			}

			// ---

			half4 frag(vtx_out IN) : COLOR
			{
				float3 eyeDir = normalize(_camPos - IN.pixelPos);
				float4 map = tex2D(_hmap, IN.uv);

				// pseudo-random height variation
				float var = (tex2D(_rndmap, IN.uvvol * 0.1).a - 0.5) * map.b;

				float3 nPixCoord = normalize(IN.vol);
				float3 exploded = nPixCoord * _planetRadius;
				float snowWeight = max(0.0, abs(exploded.y) / _planetRadius - 0.5) * 4.0 - _temperature*4.0 + map.a;
				snowWeight = clamp(snowWeight, 0.0, 1.0);

				// every height below the water level will force water texture to dominate, except when its already frozen
				float waterWeight = clamp((_waterLevel - map.a) * 50, 0.0, 1.0);

				// put height in _waterLevel..1 range so that we can sample the lut correctly.
				map.a = clamp(map.a, _waterLevel, 1.0);

				// height variation is now summed to the height
				var = clamp(map.a + var, 0.0, 1.0);

				// weights of the four blending textures
				// use the random height variation to sample weights
				float4 weights = tex2D(_blendLut, float2(map.b, var)) * (1.0 - snowWeight - waterWeight);

				// weights for the base texture is the remaining non-used weight until 1.0
				float baseWeight = max(0.0, 1.0 - (snowWeight + waterWeight + weights.r + weights.g + weights.b + weights.a));

				float3 diffuse = (
								tex2D(_diffuse1, IN.uvvol * _tiling1) * weights.x +
								tex2D(_diffuse2, IN.uvvol * _tiling2) * weights.y +
								tex2D(_diffuse3, IN.uvvol * _tiling3) * weights.z +
								tex2D(_diffuse4, IN.uvvol * _tiling4) * weights.w +
								tex2D(_waterTex, IN.uvvol * _tilingWater) * waterWeight +
								tex2D(_snowTex, IN.uvvol * _tilingSnow) * snowWeight +
								tex2D(_diffuseBase, IN.uvvol * _tilingBase) * baseWeight ).rgb;

				// ---

				// calculate atmosphere influence
				// first we will determine the color of the sphere at the eye direction,
				// then interpolate that in relation to pixel height * distance (the more far and lower height the more fog).
				// the calculation here is similar to the AtmosIn.shader in regard to determining the correct eye direction atmosphere color

				//const float waterLevelAnim = 0.0003;

				float camHeight = max(0.1, length(_camPos) - _planetRadius);
				float pixelHeight = max(0.1, length(IN.pixelPos) - _planetRadius);
				float atmosHeight = _atmosRadius - _planetRadius;

				float dotCam = dot(IN.normal, _sunDir);
				float dotSunSquared = pow(1 - dotCam, 2);

				float horzHeight = _horizonHeight - max(0, -dotCam) * 0.25;
				float dotNormal2Normal = pow( clamp( horzHeight + dot(normalize(_camPos), eyeDir), 0, 1 ), _horizonPower);
				//dotNormal2Normal *= _horizonIntensity;

				float tint = clamp(dotSunSquared, 0.0, 1.0);
//				float3 atmosColor = lerp(_color1, _color2, tint) * dotNormal2Normal * _fogIntensity;
				float3 atmosColor = lerp(_color1, _color2, tint) * _fogIntensity;

				float dist = length(_camPos - IN.pixelPos);
				float fogHeightLevel = 1 - clamp(pixelHeight / (atmosHeight * _fogHeight), 0, 1);
//				float fogLevel = clamp((dist - _fogNear) / _fogFar, 0, _fogMaxAlpha) * max(0, fogHeightLevel * -dot(eyeDir, _sunDir));
//				float fogLevel = clamp((dist - _fogNear) / _fogFar, 0, _fogMaxAlpha) * fogHeightLevel * max(0, 1 - dot(normalize(_camPos), _sunDir));
				float fogLevel = clamp((dist - _fogNear) / _fogFar, 0, _fogMaxAlpha) * fogHeightLevel;

				// apply the fog
				diffuse = lerp(diffuse, atmosColor, fogLevel);

				// ---

				float3 up = IN.normal;
				float3 dx = map.r * IN.tangent;
				float3 dy = map.g * IN.binormal;

				up += dx;
				up += dy;
				up = normalize(up);

				// first water normal perturbation
				float3 upWater = IN.normal;
				float3 wn = UnpackNormal(tex2D(_waterNormal, IN.uvvol * _tilingWater));
				float3 wdx = wn.r * IN.tangent;
				float3 wdy = wn.g * IN.binormal;
				upWater += wdx;
				upWater += wdy;
				upWater = normalize(upWater);

				// second water normal perturbation
				float3 upWater2 = IN.normal;
				float3 wn2 = UnpackNormal(tex2D(_waterNormal, IN.uvvol * _tilingWater + _frame));
				float3 wdx2 = wn2.r * IN.tangent;
				float3 wdy2 = wn2.g * IN.binormal;
				upWater2 += wdx2;
				upWater2 += wdy2;
				upWater2 = normalize(upWater2);

				// add both perturbations
				upWater = normalize(upWater + upWater2);

				float smoothDot = dot(upWater, _sunDir);
				//float smoothLight = clamp(pow(smoothDot, 3), 0.02, 1.0);
				//float smoothLight = clamp(smoothDot * abs(smoothDot), 0.02, 1.0);
				float smoothLight = clamp(smoothDot, 0.02, 1.0);

				float disturbedDot = dot(up, _sunDir);
				//float disturbedLight = clamp(pow(disturbedDot, 3), 0.02, 1.0);
				//float disturbedLight = clamp(disturbedDot * abs(disturbedDot), 0.02, 1.0);
				float disturbedLight = clamp(disturbedDot, 0.02, 1.0);

				float diff = _waterLevel; // + waterLevelAnim + max(-waterLevelAnim, waterLevelAnim * sin(_frame));
				float waterDepth = diff - map.a;
				if (waterDepth >= 0.0)
				{
					// water
					float3 refvec = reflect(-eyeDir, upWater);
					float refSun = clamp(dot(refvec, _sunDir), 0.0, 1.0);
					float spec1 = clamp(pow(refSun, 12), 0.0, 1.0) * 1.5;		// ### specular power and intensity could be a parameter
					float spec2 = clamp(pow(refSun, 128), 0.0, 1.0) * 4.0;		// ### second specular power and intensity could be a parameter
					float shoreAmount = 0.10 - waterDepth * 10.0;				// ### coast incidence could be a parameter
					shoreAmount = clamp(shoreAmount, 0.0, 1.0);
					diffuse = lerp(diffuse, float3(1.0, 1.0, 1.0), shoreAmount);
					diffuse = diffuse * (smoothLight + spec1 + spec2);
				}
				else
				{
					// landscape
					diffuse = diffuse * disturbedLight;
				}

				return half4(diffuse * _sunIntensity, 1);
			}

			ENDCG

		}
	}

	FallBack "Diffuse"
}
