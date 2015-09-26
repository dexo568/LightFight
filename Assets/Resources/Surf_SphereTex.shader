//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

Shader "Surf_SphereTex"
{
	Properties
	{
		_tex ("Base Texture", 2D) = "white" {}
		_hmap ("Heightmap Texture", 2D) = "black" {}
		_planetRadius ("planetRadius", Float) = 16.0
		_heightScale ("heightScale", Float) = 1.0
	}

	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM

			#pragma target 3.0
			#pragma surface surf Lambert vertex:vert

			uniform sampler2D _tex;
			uniform sampler2D _hmap;
			uniform float _planetRadius;
			uniform float _heightScale;

			void vert(inout appdata_full v)
			{
				#if !defined(SHADER_API_OPENGL)
					float4 h = tex2Dlod(_hmap, float4(v.texcoord.xy,0,0));
					v.vertex = float4(normalize(v.vertex.xyz) * ((h.r + h.g + h.b)/3 * _heightScale + _planetRadius), 1);
				#endif
			}

			struct Input
			{
				float2 uv_MainTex;
			};

			void surf (Input IN, inout SurfaceOutput o)
			{
				half4 c = tex2D (_tex, IN.uv_MainTex);
				o.Albedo = c.rgb;
				o.Alpha = c.a;
			}

		ENDCG
	}

	FallBack "Diffuse"
}
