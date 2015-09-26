//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

using UnityEngine;
using System.Collections;

public class CGlobal
{
	private static CGlobal m_GlobalInstance = null;

	// for the GenNoise shader
	public Texture2D PermTex = null;		// permutation
	public Texture2D SimplexTex = null;		// simplex

	string[] NoiseMaterialNames = { "GenNoiseRidged", "GenNoiseTurbulence" };
	public Material[] GenNoiseMaterial = null;

	public Material ShapeNoiseMaterial = null;
	public Material GenNormalMaterial = null;
	public Material PatchMaterial = null;
	public Material PackNoiseMaterial = null;
	public Material SimpleTexMaterial = null;
	public Material OuterAtmosphereMaterial = null;
	public Material InnerAtmosphereMaterial = null;

	public SimplexNoiseCPU SNoiseCPU = null;

	public const RenderTextureFormat RENDER_TEXTURE_FORMAT = RenderTextureFormat.ARGBHalf;
	public const TextureFormat TEXTURE_FORMAT = TextureFormat.ARGB32;
	public const TextureWrapMode WRAP_MODE = TextureWrapMode.Clamp;


	public static CGlobal GetInstance()
	{
		if (m_GlobalInstance == null)
		{
			m_GlobalInstance = new CGlobal();
		}

		return m_GlobalInstance;
	}


	~CGlobal()
	{
		//Destroy();
		//m_GlobalInstance = null;
	}


	public void Setup()
	{
		Destroy();

		// init the two textures needed by the GenNoise shader
		if (PermTex == null)
		{
			InitGenNoise();
		}

		// init GenNoise materials
		if (GenNoiseMaterial == null)
		{
			GenNoiseMaterial = new Material[NoiseMaterialNames.Length];
			for (int i = 0; i < NoiseMaterialNames.Length; i++)
			{
				if (GenNoiseMaterial[i] == null)
				{
					GenNoiseMaterial[i] = new Material(Shader.Find(NoiseMaterialNames[i]));
				}
			}
		}

		// init GenNormal material
		if (GenNormalMaterial == null)
		{
			GenNormalMaterial = new Material(Shader.Find("GenNormal"));
		}

		// init ShapeNoise material
		if (ShapeNoiseMaterial == null)
		{
			ShapeNoiseMaterial = new Material(Shader.Find("ShapeNoise"));
		}

		// init patch material
		if (PatchMaterial == null)
		{
			PatchMaterial = new Material(Shader.Find("TexNormalSplat"));
		}

		// init packnoise material
		if (PackNoiseMaterial == null)
		{
			PackNoiseMaterial = new Material(Shader.Find("PackNoise"));
		}

		// init simple tex material
		if (SimpleTexMaterial == null)
		{
			SimpleTexMaterial = new Material(Shader.Find("SimpleTex"));
		}

		// init outer atmosphere tex material
		if (OuterAtmosphereMaterial == null)
		{
			OuterAtmosphereMaterial = new Material(Shader.Find("Surf_AtmosOut"));
		}

		// init inner atmosphere tex material
		if (InnerAtmosphereMaterial == null)
		{
			InnerAtmosphereMaterial = new Material(Shader.Find("Surf_AtmosIn"));
		}

		// init CPU simplex noise
		if (SNoiseCPU == null)
		{
			SNoiseCPU = new SimplexNoiseCPU();
		}
	}


	public void Destroy()
	{
		#if UNITY_EDITOR
			if (PatchMaterial != null)
			{
				Material.DestroyImmediate(PatchMaterial);
				PatchMaterial = null;
			}

			if (PackNoiseMaterial != null)
			{
				Material.DestroyImmediate(PackNoiseMaterial);
				PackNoiseMaterial = null;
			}

			if (SimpleTexMaterial != null)
			{
				Material.DestroyImmediate(SimpleTexMaterial);
				SimpleTexMaterial = null;
			}

			if (OuterAtmosphereMaterial != null)
			{
				Material.DestroyImmediate(OuterAtmosphereMaterial);
				OuterAtmosphereMaterial = null;
			}

			if (InnerAtmosphereMaterial != null)
			{
				Material.DestroyImmediate(InnerAtmosphereMaterial);
				InnerAtmosphereMaterial = null;
			}

			if (GenNoiseMaterial != null)
			{
				for (int i = 0; i < NoiseMaterialNames.Length; i++)
				{
					if (GenNoiseMaterial[i] != null)
					{
						Material.DestroyImmediate(GenNoiseMaterial[i]);
						GenNoiseMaterial[i] = null;
					}
				}
				GenNoiseMaterial = null;
			}

			if (GenNormalMaterial != null)
			{
				Material.DestroyImmediate(GenNormalMaterial);
				GenNormalMaterial = null;
			}

			if (ShapeNoiseMaterial != null)
			{
				Material.DestroyImmediate(ShapeNoiseMaterial);
				ShapeNoiseMaterial = null;
			}

			if (PermTex != null)
			{
				Texture2D.DestroyImmediate(PermTex);
				PermTex = null;
			}

			if (SimplexTex != null)
			{
				Texture2D.DestroyImmediate(SimplexTex);
				SimplexTex = null;
			}

#else

		if (PatchMaterial != null)
			{
				Material.Destroy(PatchMaterial);
				PatchMaterial = null;
			}

			if (PackNoiseMaterial != null)
			{
				Material.Destroy(PackNoiseMaterial);
				PackNoiseMaterial = null;
			}

			if (SimpleTexMaterial != null)
			{
				Material.Destroy(SimpleTexMaterial);
				SimpleTexMaterial = null;
			}

			if (OuterAtmosphereMaterial != null)
			{
				Material.Destroy(OuterAtmosphereMaterial);
				OuterAtmosphereMaterial = null;
			}

			if (InnerAtmosphereMaterial != null)
			{
				Material.Destroy(InnerAtmosphereMaterial);
				InnerAtmosphereMaterial = null;
			}

			if (GenNoiseMaterial != null)
			{
				for (int i = 0; i < NoiseMaterialNames.Length; i++)
				{
					if (GenNoiseMaterial[i] != null)
					{
						Material.Destroy(GenNoiseMaterial[i]);
						GenNoiseMaterial[i] = null;
					}
				}
				GenNoiseMaterial = null;
			}

			if (GenNormalMaterial != null)
			{
				Material.Destroy(GenNormalMaterial);
				GenNormalMaterial = null;
			}

			if (ShapeNoiseMaterial != null)
			{
				Material.Destroy(ShapeNoiseMaterial);
				ShapeNoiseMaterial = null;
			}

			if (PermTex != null)
			{
				Texture2D.Destroy(PermTex);
				PermTex = null;
			}

			if (SimplexTex != null)
			{
				Texture2D.Destroy(SimplexTex);
				SimplexTex = null;
			}
		#endif

		SNoiseCPU = null;
	}


	// initialize the three pre-computed GenNoise shader textures
	public void InitGenNoise()
	{
		InitPermTexture();
		InitSimplexTexture();
	}


	// private
	private void InitPermTexture()
	{
		int[] perm =
			{
				151,160,137,91,90,15, 
				131,13,201,95,96,53,194,233,7,225,140,36,103,30,69,142,8,99,37,240,21,10,23,
				190, 6,148,247,120,234,75,0,26,197,62,94,252,219,203,117,35,11,32,57,177,33,
				88,237,149,56,87,174,20,125,136,171,168, 68,175,74,165,71,134,139,48,27,166,
				77,146,158,231,83,111,229,122,60,211,133,230,220,105,92,41,55,46,245,40,244,
				102,143,54, 65,25,63,161, 1,216,80,73,209,76,132,187,208, 89,18,169,200,196,
				135,130,116,188,159,86,164,100,109,198,173,186, 3,64,52,217,226,250,124,123,
				5,202,38,147,118,126,255,82,85,212,207,206,59,227,47,16,58,17,182,189,28,42,
				223,183,170,213,119,248,152, 2,44,154,163, 70,221,153,101,155,167, 43,172,9,
				129,22,39,253, 19,98,108,110,79,113,224,232,178,185, 112,104,218,246,97,228,
				251,34,242,193,238,210,144,12,191,179,162,241, 81,51,145,235,249,14,239,107,
				49,192,214, 31,181,199,106,157,184, 84,204,176,115,121,50,45,127, 4,150,254,
				138,236,205,93,222,114,67,29,24,72,243,141,128,195,78,66,215,61,156,180
			};

		int[,] grad3 = new int[,]
			{
				{0,1,1},{0,1,-1},{0,-1,1},{0,-1,-1},
				{1,0,1},{1,0,-1},{-1,0,1},{-1,0,-1},
				{1,1,0},{1,-1,0},{-1,1,0},{-1,-1,0},
				{1,0,-1},{-1,0,-1},{0,-1,1},{0,1,1}
			};

		Color[] pixels = new Color[256 * 256];
		int i, j;
		float inv256 = 1.0f / 255.0f;

		for (i = 0; i < 256; i++)
		{
			for (j = 0; j < 256; j++)
			{
				int offset = (i * 256 + j);
				char value = (char)perm[(j + perm[i]) & 0xFF];
				pixels[offset].r = (grad3[value & 0x0F, 0] * 64 + 64) * inv256;		// Gradient x
				pixels[offset].g = (grad3[value & 0x0F, 1] * 64 + 64) * inv256;		// Gradient y
				pixels[offset].b = (grad3[value & 0x0F, 2] * 64 + 64) * inv256;		// Gradient z
				pixels[offset].a = value * inv256;									// Permuted index
			}
		}

		// allocate the unity3d texture
		PermTex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
		PermTex.anisoLevel = 0;
		PermTex.filterMode = FilterMode.Bilinear;
		PermTex.wrapMode = TextureWrapMode.Repeat;

		// upload pixels
		PermTex.SetPixels(pixels);
		PermTex.Apply(false);

		// ### SAVE TO DISK
		//byte[] bytes = PermTex.EncodeToPNG();
		//Debug.Log(bytes + " bytes in PNG");
		//string str = "d:/temp/PermTex.png";
		//if (System.IO.File.Exists(str)) System.IO.File.Delete(str);
		//System.IO.FileStream fs = new System.IO.FileStream(str, System.IO.FileMode.CreateNew);
		//System.IO.BinaryWriter w = new System.IO.BinaryWriter(fs);
		//w.Write(bytes);
		//w.Close();
		//fs.Close();
	}


	// private
	private void InitSimplexTexture()
	{
		byte[,] simplex4 = new byte[,]
			{
				{0,64,128,192},{0,64,192,128},{0,0,0,0},
				{0,128,192,64},{0,0,0,0},{0,0,0,0},{0,0,0,0},{64,128,192,0},
				{0,128,64,192},{0,0,0,0},{0,192,64,128},{0,192,128,64},
				{0,0,0,0},{0,0,0,0},{0,0,0,0},{64,192,128,0},
				{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
				{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
				{64,128,0,192},{0,0,0,0},{64,192,0,128},{0,0,0,0},
				{0,0,0,0},{0,0,0,0},{128,192,0,64},{128,192,64,0},
				{64,0,128,192},{64,0,192,128},{0,0,0,0},{0,0,0,0},
				{0,0,0,0},{128,0,192,64},{0,0,0,0},{128,64,192,0},
				{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
				{0,0,0,0},{0,0,0,0},{0,0,0,0},{0,0,0,0},
				{128,0,64,192},{0,0,0,0},{0,0,0,0},{0,0,0,0},
				{192,0,64,128},{192,0,128,64},{0,0,0,0},{192,64,128,0},
				{128,64,0,192},{0,0,0,0},{0,0,0,0},{0,0,0,0},
				{192,64,0,128},{0,0,0,0},{192,128,0,64},{192,128,64,0}
			};

		float inv256 = 1.0f / 255.0f;

		Color[] pixels = new Color[64];
		for (int i = 0; i < 64; i++)
		{
			pixels[i] = new Color(simplex4[i, 0] * inv256, simplex4[i, 1] * inv256, simplex4[i, 2] * inv256, simplex4[i, 3] * inv256);
		}

		SimplexTex = new Texture2D(64, 1, TextureFormat.ARGB32, false);
		SimplexTex.anisoLevel = 0;
		SimplexTex.filterMode = FilterMode.Bilinear;
		SimplexTex.wrapMode = TextureWrapMode.Repeat;

		// upload pixels
		SimplexTex.SetPixels(pixels);
		SimplexTex.Apply(false);

		// ### SAVE TO DISK
		//byte[] bytes = SimplexTex.EncodeToPNG();
		//Debug.Log(bytes + " bytes in PNG");
		//string str = "d:/temp/SimplexTex.png";
		//if (System.IO.File.Exists(str)) System.IO.File.Delete(str);
		//System.IO.FileStream fs = new System.IO.FileStream(str, System.IO.FileMode.CreateNew);
		//System.IO.BinaryWriter w = new System.IO.BinaryWriter(fs);
		//w.Write(bytes);
		//w.Close();
		//fs.Close();
	}


	//private void GenLutTex()
	//{
	//    Color[] pixels = new Color[256*256];

	//    int tex1, tex2, tex3, tex4;
	//    int idx = 0;
	//    for (int height=0; height<256; height++)
	//    {
	//        for (int slope=0; slope<256; slope++)
	//        {
	//            int h = height;
	//            int s = Mathf.Min(255, Mathf.Max(0, (slope - 96) * 3));
	//            tex1 = Mathf.Min(255, Mathf.Max(0, 255-s-h*64));
	//            tex2 = (int)Mathf.Min(255-tex1, Mathf.Max(0, 255-s-h*4));
	//            tex3 = (int)Mathf.Min(255-tex1-tex2, Mathf.Max(0, 255-s-(h-100)));
	//            tex4 = (int)Mathf.Min(255-tex1-tex2-tex3, Mathf.Max(0, 255-s));

	//            pixels[idx] = new Color(tex1 / 255.0f, tex2 / 255.0f, tex3 / 255.0f, tex4 / 255.0f);
	//            idx++;
	//        }
	//    }

	//    LutTex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
	//    LutTex.anisoLevel = 0;
	//    LutTex.filterMode = FilterMode.Bilinear;
	//    LutTex.wrapMode = TextureWrapMode.Clamp;

	//    // upload pixels
	//    LutTex.SetPixels(pixels);
	//    LutTex.Apply(false);

	//    // ### SAVE TO DISK
	//    //byte[] bytes = LutTex.EncodeToPNG();
	//    //Debug.Log(bytes + " bytes in PNG");
	//    //string str = "d:/temp/LutTex.png";
	//    //if (System.IO.File.Exists(str)) System.IO.File.Delete(str);
	//    //System.IO.FileStream fs = new System.IO.FileStream(str, System.IO.FileMode.CreateNew);
	//    //System.IO.BinaryWriter w = new System.IO.BinaryWriter(fs);
	//    //w.Write(bytes);
	//    //w.Close();
	//    //fs.Close();
	//}


	public Mesh MakeSphere(float radius, int divr, int divh)
	{
		float a,da,yp,ya,yda,yf;
		float U,V,dU,dV;

		if (divr < 3) divr = 3;
		if (divh < 3) divh = 3;

		int numVerts = (divh-2) * divr + 2 + (divh-2);
		int numInds = (divr * 2 + divr * 2 * (divh - 3)) * 3;

		Mesh mesh = new Mesh();
		Vector3[] vertices = new Vector3[numVerts];
		Vector2[] uvs = new Vector2[numVerts];
		int[] inds = new int[numInds];

		int vidx = 0;		// current vertex
		int fidx = 0;		// current index

		// top and bottom vertices
		vertices[vidx++] = new Vector3(0, radius, 0);
		vertices[vidx++] = new Vector3(0, -radius, 0);

		ya = 0;
		yda = Mathf.PI / (divh-1);
		da = (2 * Mathf.PI) / divr;

		// all other vertices
		for (int y = 0; y < divh-2; y++)
		{
			ya += yda;
			yp = Mathf.Cos(ya) * radius;
			yf = Mathf.Sin(ya) * radius;
			a = 0;

			for (int x = 0; x < divr; x++)
			{
				vertices[vidx++] = new Vector3( Mathf.Cos(a) * yf, yp, Mathf.Sin(a) * yf);

				if (x == divr - 1)
				{
					// add an extra vertex in the end of each longitudinal circunference
					Vector3 tmp = vertices[y * (divr + 1) + 2];
					vertices[vidx++] = new Vector3(tmp.x, tmp.y, tmp.z);
				}

				a += da;
			}
		}

		a = 0;
		U = 0;
		dU = 1.0f / divr;
		dV = V = 1.0f / divh;

		// top indices
		for (int x = 0; x < divr; x++)
		{
			int[] v = { 0, 2+x+1, 2+x };

			inds[fidx++] = v[0];
			inds[fidx++] = v[1];
			inds[fidx++] = v[2];

			uvs[v[0]].x = U;
			uvs[v[0]].y = 0;

			uvs[v[1]].x = U + dU;
			uvs[v[1]].y = V;

			uvs[v[2]].x = U;
			uvs[v[2]].y = V;

			U += dU;
		}

		da = 1.0f / (divr + 1);
		int offv = 2;

		// create main body faces
		for (int x = 0; x < divh-3; x++)
		{
			U = 0;
			for (int y = 0; y < divr; y++)
			{
				int[] v = { offv + y, offv + (divr + 1) + y + 1, offv + (divr + 1) + y };

				inds[fidx++] = v[0];
				inds[fidx++] = v[1];
				inds[fidx++] = v[2];

				uvs[v[0]].x = U;
				uvs[v[0]].y = V;

				uvs[v[1]].x = U + dU;
				uvs[v[1]].y = V + dV;

				uvs[v[2]].x = U;
				uvs[v[2]].y = V + dV;

				int[] vv = { offv+y, offv+y+1, offv+y+1+(divr+1) };

				inds[fidx++] = vv[0];
				inds[fidx++] = vv[1];
				inds[fidx++] = vv[2];

				uvs[vv[0]].x = U;
				uvs[vv[0]].y = V;

				uvs[vv[1]].x = U + dU;
				uvs[vv[1]].y = V;

				uvs[vv[2]].x = U + dU;
				uvs[vv[2]].y = V + dV;

				U += dU;
			}

			V += dV;
			offv += divr + 1;
		}

		int s = numVerts - divr - 1;
		U = 0;

		// bottom faces
		for (int x = 0; x < divr; x++)
		{
			int[] v = { 1, s+x, s+x+1 };

			inds[fidx++] = v[0];
			inds[fidx++] = v[1];
			inds[fidx++] = v[2];

			uvs[v[0]].x = U;
			uvs[v[0]].y = 1.0f;

			uvs[v[1]].x = U;
			uvs[v[1]].y = V;

			uvs[v[2]].x = U + dU;
			uvs[v[2]].y = V;

			U += dU;
		}

		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.triangles = inds;

		mesh.Optimize();
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();

		return mesh;
	}


	public void RenderQuadVolume(int width, int height, Material material, CBoundingVolume volume, bool flip)
	{
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.Viewport(new Rect(0, 0, width, height));

		material.SetPass(0);

		Vector3 v1 = volume.vertices[0];
		Vector3 uv1 = volume.uvs[0];

		Vector3 v2 = volume.vertices[3];
		Vector3 uv2 = volume.uvs[3];

		Vector3 v3 = volume.vertices[2];
		Vector3 uv3 = volume.uvs[2];

		Vector3 v4 = volume.vertices[1];
		Vector3 uv4 = volume.uvs[1];

		GL.Begin(GL.QUADS);

		if (flip)
		{
			GL.MultiTexCoord(0, new Vector3(0, 1, 0));
			GL.MultiTexCoord(1, v1);
			GL.MultiTexCoord(2, uv1);
			GL.Vertex3(-1, 1, 0);

			GL.MultiTexCoord(0, new Vector3(0, 0, 0));
			GL.MultiTexCoord(1, v2);
			GL.MultiTexCoord(2, uv2);
			GL.Vertex3(-1, -1, 0);

			GL.MultiTexCoord(0, new Vector3(1, 0, 0));
			GL.MultiTexCoord(1, v3);
			GL.MultiTexCoord(2, uv3);
			GL.Vertex3(1, -1, 0);

			GL.MultiTexCoord(0, new Vector3(1, 1, 0));
			GL.MultiTexCoord(1, v4);
			GL.MultiTexCoord(2, uv4);
			GL.Vertex3(1, 1, 0);
		}
		else
		{
			GL.MultiTexCoord(0, new Vector3(0, 0, 0));
			GL.MultiTexCoord(1, v1);
			GL.MultiTexCoord(2, uv1);
			GL.Vertex3(-1, 1, 0);

			GL.MultiTexCoord(0, new Vector3(0, 1, 0));
			GL.MultiTexCoord(1, v2);
			GL.MultiTexCoord(2, uv2);
			GL.Vertex3(-1, -1, 0);

			GL.MultiTexCoord(0, new Vector3(1, 1, 0));
			GL.MultiTexCoord(1, v3);
			GL.MultiTexCoord(2, uv3);
			GL.Vertex3(1, -1, 0);

			GL.MultiTexCoord(0, new Vector3(1, 0, 0));
			GL.MultiTexCoord(1, v4);
			GL.MultiTexCoord(2, uv4);
			GL.Vertex3(1, 1, 0);
		}

		GL.End();
		GL.PopMatrix();
	}


	public void RenderQuadVolumeExCoord(int width, int height, Material material, CBoundingVolume volume, bool flip)
	{
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.Viewport(new Rect(0, 0, width, height));

		material.SetPass(0);

		Vector3 v1 = volume.exvertices[0];
		Vector3 uv1 = volume.uvs[0];

		Vector3 v2 = volume.exvertices[3];
		Vector3 uv2 = volume.uvs[3];

		Vector3 v3 = volume.exvertices[2];
		Vector3 uv3 = volume.uvs[2];

		Vector3 v4 = volume.exvertices[1];
		Vector3 uv4 = volume.uvs[1];

		GL.Begin(GL.QUADS);

		if (flip)
		{
			GL.MultiTexCoord(0, new Vector3(0, 1, 0));
			GL.MultiTexCoord(1, v1);
			GL.MultiTexCoord(2, uv1);
			GL.Vertex3(-1, 1, 0);

			GL.MultiTexCoord(0, new Vector3(0, 0, 0));
			GL.MultiTexCoord(1, v2);
			GL.MultiTexCoord(2, uv2);
			GL.Vertex3(-1, -1, 0);

			GL.MultiTexCoord(0, new Vector3(1, 0, 0));
			GL.MultiTexCoord(1, v3);
			GL.MultiTexCoord(2, uv3);
			GL.Vertex3(1, -1, 0);

			GL.MultiTexCoord(0, new Vector3(1, 1, 0));
			GL.MultiTexCoord(1, v4);
			GL.MultiTexCoord(2, uv4);
			GL.Vertex3(1, 1, 0);
		}
		else
		{
			GL.MultiTexCoord(0, new Vector3(0, 0, 0));
			GL.MultiTexCoord(1, v1);
			GL.MultiTexCoord(2, uv1);
			GL.Vertex3(-1, 1, 0);

			GL.MultiTexCoord(0, new Vector3(0, 1, 0));
			GL.MultiTexCoord(1, v2);
			GL.MultiTexCoord(2, uv2);
			GL.Vertex3(-1, -1, 0);

			GL.MultiTexCoord(0, new Vector3(1, 1, 0));
			GL.MultiTexCoord(1, v3);
			GL.MultiTexCoord(2, uv3);
			GL.Vertex3(1, -1, 0);

			GL.MultiTexCoord(0, new Vector3(1, 0, 0));
			GL.MultiTexCoord(1, v4);
			GL.MultiTexCoord(2, uv4);
			GL.Vertex3(1, 1, 0);
		}

		GL.End();
		GL.PopMatrix();
	}


	public void RenderQuadVolumeExUv(int width, int height, int sourceWidth, Material material, CBoundingVolume volume, bool flip)
	{
		GL.PushMatrix();
		GL.LoadOrtho();
		GL.Viewport(new Rect(0, 0, width, height));

		material.SetPass(0);

		float zero = 1.0f / sourceWidth;
		float one = 1 - zero;

		Vector3 v1 = volume.vertices[0];
		Vector3 uv1 = new Vector3(zero, zero, 0);

		Vector3 v2 = volume.vertices[3];
		Vector3 uv2 = new Vector3(zero, one, 0);

		Vector3 v3 = volume.vertices[2];
		Vector3 uv3 = new Vector3(one, one, 0);

		Vector3 v4 = volume.vertices[1];
		Vector3 uv4 = new Vector3(one, zero, 0);

		GL.Begin(GL.QUADS);

		if (flip)
		{
			GL.MultiTexCoord(0, new Vector3(0, 1, 0));
			GL.MultiTexCoord(1, v1);
			GL.MultiTexCoord(2, uv1);
			GL.Vertex3(-1, 1, 0);

			GL.MultiTexCoord(0, new Vector3(0, 0, 0));
			GL.MultiTexCoord(1, v2);
			GL.MultiTexCoord(2, uv2);
			GL.Vertex3(-1, -1, 0);

			GL.MultiTexCoord(0, new Vector3(1, 0, 0));
			GL.MultiTexCoord(1, v3);
			GL.MultiTexCoord(2, uv3);
			GL.Vertex3(1, -1, 0);

			GL.MultiTexCoord(0, new Vector3(1, 1, 0));
			GL.MultiTexCoord(1, v4);
			GL.MultiTexCoord(2, uv4);
			GL.Vertex3(1, 1, 0);
		}
		else
		{
			GL.MultiTexCoord(0, new Vector3(0, 0, 0));
			GL.MultiTexCoord(1, v1);
			GL.MultiTexCoord(2, uv1);
			GL.Vertex3(-1, 1, 0);

			GL.MultiTexCoord(0, new Vector3(0, 1, 0));
			GL.MultiTexCoord(1, v2);
			GL.MultiTexCoord(2, uv2);
			GL.Vertex3(-1, -1, 0);

			GL.MultiTexCoord(0, new Vector3(1, 1, 0));
			GL.MultiTexCoord(1, v3);
			GL.MultiTexCoord(2, uv3);
			GL.Vertex3(1, -1, 0);

			GL.MultiTexCoord(0, new Vector3(1, 0, 0));
			GL.MultiTexCoord(1, v4);
			GL.MultiTexCoord(2, uv4);
			GL.Vertex3(1, 1, 0);
		}

		GL.End();
		GL.PopMatrix();
	}
}
