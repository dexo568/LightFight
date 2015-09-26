//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "GenTexture"
{
	Properties
	{
		_planetRadius ("planetRadius", Float) = 16.0
		_temperature ("temperature", Float) = 0.0
		_blendLut ("blendLut", 2D) = "black" {}

		_waterLevel ("waterLevel", Float) = 0.0

		_waterTex ("waterTex", 2D) = "black" {}
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

		_nrmMap ("nrmMap", 2D) = "black" {}
	}

	SubShader
	{
		Pass
		{
			CGPROGRAM

				#pragma target 3.0
				//#pragma exclude_renderers opengl
				//#pragma exclude_renderers d3d9
				#pragma vertex vert
				#pragma fragment frag
				#include "UnityCG.cginc"

				uniform float _planetRadius;
				uniform float _temperature;

				// 256x256 lut with 4 channels, one diffuse blending weight per channel
				uniform sampler2D _blendLut;

				//
				// textures blend
				//

				uniform float _waterLevel;
				uniform sampler2D _waterTex;		// every planet has a water texture, that may be modulated or not according to the height and temperature
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

				// normalmap is:
				// r,g : tangent-space normal perturbation
				// b   : slope
				// a   : height
				uniform sampler2D _nrmMap;

				//
				// vertex
				//

				struct vtx_out
				{
					float4 position : POSITION;
					float2 uv : TEXCOORD0;
					float3 vol : TEXCOORD1;
					float2 uvvol : TEXCOORD2;
				};

				vtx_out vert(float4 position : POSITION, float2 uv : TEXCOORD0, float3 vol : TEXCOORD1, float2 uvvol : TEXCOORD2)
				{
					vtx_out OUT;
					OUT.position = position;
					OUT.uv = uv;
					OUT.vol = float3(vol.x, vol.y, vol.z);
					OUT.uvvol = uvvol;
					return OUT;
				}

				//
				// fragment
				//

				half4 frag(vtx_out i) : COLOR
				{
					float4 texelNormal = tex2D(_nrmMap, i.uv);

					float3 nTexCoord = normalize(i.vol);
					float3 exploded = nTexCoord * _planetRadius;
					float snowWeight = max(0.0, abs(exploded.y)/_planetRadius - 0.5) * 4.0 - _temperature*4.0 + texelNormal.a;
					snowWeight = clamp(snowWeight, 0.0, 1.0);

					// every height below the water level will force water texture to dominate, except when its already frozen
					float waterWeight = clamp((_waterLevel - texelNormal.a) * 10, 0.0, 1.0);

					// put height in _waterLevel..1 range so that we can sample the lut correctly.
					texelNormal.a = clamp(texelNormal.a, _waterLevel, 1.0);

					// weights of the four blending textures
					float4 weights = tex2D(_blendLut, float2(texelNormal.b, texelNormal.a)) * (1.0 - snowWeight - waterWeight);

					// weights for the base texture is the remaining non-used weight until 1.0
					float baseWeight = max(0.0, 1.0 - (snowWeight + waterWeight + weights.r + weights.g + weights.b + weights.a));

#if 1
					//
					// use a global spherical uv mapping
					//

					float4 diffuse =
									tex2D(_diffuse1, i.uvvol * _tiling1) * weights.x +
									tex2D(_diffuse2, i.uvvol * _tiling2) * weights.y +
									tex2D(_diffuse3, i.uvvol * _tiling3) * weights.z +
									tex2D(_diffuse4, i.uvvol * _tiling4) * weights.w +
									tex2D(_waterTex, i.uvvol * _tilingWater) * waterWeight +
									tex2D(_snowTex, i.uvvol * _tilingSnow) * snowWeight +
									tex2D(_diffuseBase, i.uvvol * _tilingBase) * baseWeight;


#else

					//
					// use a local, patch uv mapping
					//

					float4 diffuse =
									tex2D(_diffuse1, i.uv * _tiling1) * weights.r +
									tex2D(_diffuse2, i.uv * _tiling2) * weights.g +
									tex2D(_diffuse3, i.uv * _tiling3) * weights.b +
									tex2D(_diffuse4, i.uv * _tiling4) * weights.a +
									tex2D(_waterTex, i.uv * _tilingWater) * waterWeight +
									tex2D(_snowTex, i.uv * _tilingSnow) * snowWeight +
									tex2D(_diffuseBase, i.uv * _tilingBase) * baseWeight;

#endif

					return diffuse;
				}

			ENDCG
		}
	}
	FallBack "Diffuse"
}
