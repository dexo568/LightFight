//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "Surf_TexNormalSplat"
{
	Properties
	{
		_hmap ("hmap", 2D) = "black" {}
		_rndmap ("rndmap", 2D) = "black" {}
		_waterLevel ("waterLevel", Float) = 0
		_frame ("frame", Float) = 0

		// ---

		_planetRadius ("planetRadius", Float) = 16.0
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
		_camPos ("camPos", Vector) = (0, 0, 0)				// The camera's current position
	}

	SubShader
	{
			CGPROGRAM

			#pragma target 3.0
			#pragma surface surf Lambert vertex:vert
			//#pragma exclude_renderers xbox360

			uniform sampler2D _hmap;
			uniform sampler2D _rndmap;
			uniform float _waterLevel;
			uniform float _frame;

			uniform float _planetRadius;
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
			uniform float3 _camPos;

			struct Input
			{
				float4 tangent;							// vertex tangent (x,y,z = tangent) (w = eyevector x)
				float4 binormal;						// vertex binormal (x,y,z = binormal) (w = eyevector y)
				float4 frontColor;						// atmospheric color (r,g,b = frontColor) (a = eyevector z)
				float4 texcoords;						// (x,y = heightmap uv coords) (z,w = source splat textures flat uv coords)
				float3 texcoord2;						// patch volume flat 3d coords will come as normal
			};

			void vert(inout appdata_full v, out Input o)
			{
				float3 normal = normalize(v.vertex.xyz);								// normal is calculated by vertex displacement from the object's origin
				o.tangent.xyz = v.tangent.xyz;
				o.binormal.xyz = cross(normal, o.tangent.xyz);
				o.texcoords.xy = v.texcoord.st;
				o.texcoords.zw = v.texcoord1.st;
				o.texcoord2 = v.normal;													// not really the normal, but the patch volume 3d coords is passed using this parm slot

				// Get the ray from the camera to the vertex, and its length (which is the far point of the ray passing through the atmosphere)
				float3 eyevec = normalize(_camPos - v.vertex.xyz);

				o.tangent.w = eyevec.x;
				o.binormal.w = eyevec.y;
				o.frontColor.a = eyevec.z;
				o.frontColor.rgb = float3(0,0,0);
			}

			// ---

			void surf(Input IN, inout SurfaceOutput o)
			{
#if 1
				float4 map = tex2D(_hmap, IN.texcoords.xy);

				// pseudo-random height variation
				float var = (tex2D(_rndmap, IN.texcoords.zw * 0.1).a - 0.5) * map.b;

				float3 nPixCoord = normalize(IN.texcoord2);
				float3 exploded = nPixCoord * _planetRadius;
				float snowWeight = max(0.0, abs(exploded.y)/_planetRadius - 0.5) * 4.0 - _temperature*4.0 + map.a;
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

				float4 diffuse =
								tex2D(_diffuse1, IN.texcoords.zw * _tiling1) * weights.x +
								tex2D(_diffuse2, IN.texcoords.zw * _tiling2) * weights.y +
								tex2D(_diffuse3, IN.texcoords.zw * _tiling3) * weights.z +
								tex2D(_diffuse4, IN.texcoords.zw * _tiling4) * weights.w +
								tex2D(_waterTex, IN.texcoords.zw * _tilingWater) * waterWeight +
								tex2D(_snowTex, IN.texcoords.zw * _tilingSnow) * snowWeight +
								tex2D(_diffuseBase, IN.texcoords.zw * _tilingBase) * baseWeight;

				// ---

				float3 up = float3(0,0,2);
				float3 dx = map.r * IN.tangent.xyz;
				float3 dy = map.g * IN.binormal.xyz;
				up += dx;
				up += dy;
				up = normalize(up);

				float3 upWater = float3(0,0,1);
				float3 wn = UnpackNormal(tex2D(_waterNormal, IN.texcoords.xy * _tilingWater));
				float3 wdx = wn.r * IN.tangent;
				float3 wdy = wn.g * IN.binormal;
				upWater += wdx;
				upWater += wdy;
				upWater = normalize(upWater);

				float diff = _waterLevel; // + 0.0001 + 0.0001 * sin(_frame);
				float waterDepth = diff - map.a;
				if (waterDepth >= 0.0)
				{
					// water
					float3 eyevec = float3(IN.tangent.w, IN.binormal.w, IN.frontColor.a);
					float3 refvec = reflect(eyevec, upWater);
					float refSun = clamp(-dot(refvec, _sunDir), 0.0, 1.0);
					float spec1 = clamp(pow(refSun, 8), 0.0, 1.0) * 1.5;			// ### potência do specular podia ser um parâmetro...
					float spec2 = clamp(pow(refSun, 128), 0.0, 1.0) * 4.0;		// ### segunda potência do specular podia ser um parâmetro...
					float shoreAmount = 0.10 - waterDepth * 64.0;								// ### incidência da costa podia ser um parâmetro...
					shoreAmount = clamp(shoreAmount, 0.0, 1.0);
					diffuse = lerp(diffuse, float4(1.0, 1.0, 1.0, 1.0), shoreAmount);
					diffuse = diffuse * (spec1 + spec2);
					o.Normal = upWater;
				}
				else
				{
					// landscape
					o.Normal = up;
				}

				o.Albedo = IN.frontColor.rgb + diffuse.rgb;

#else

				float4 map = tex2D(_hmap, IN.texcoords.xy);
				o.Albedo = float3(map.a, map.a, map.a);

#endif
			}

			ENDCG

	}

	FallBack "Diffuse"
}
