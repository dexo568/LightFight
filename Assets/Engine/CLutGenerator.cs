//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

using UnityEngine;
using System.Collections;


[System.Serializable]
public class CLutLayer
{
	public float minh;			// minimum height (0..1 = 255)
	public float maxh;			// maximum height (0..1 = 255)
	public float slope;			// the slope (0..1 = 90º)
	public float aperture;		// the slope angle aperture (0..1 = 180º)

	public CLutLayer(float minh, float maxh, float slope, float aperture)
	{
		this.minh = minh;
		this.maxh = maxh;
		this.slope = slope;
		this.aperture = aperture;
	}
}


// look up table of slopes blending texture for GenTexture
public class CLutGenerator
{
	public Texture2D m_LutTex = null;


	public CLutGenerator()
	{
		m_LutTex = new Texture2D(256, 256, TextureFormat.ARGB32, false);
	}


	//~CLutGenerator()
	//{
	//    #if UNITY_EDITOR
	//        Texture2D.DestroyImmediate(m_LutTex);
	//    #else
	//        Texture2D.Destroy(m_LutTex);
	//    #endif
	//}


	// maximum of 4 layers supported
	// layer 0 goes into color.r
	// layer 1 goes into color.g
	// layer 2 goes into color.b
	// layer 3 goes into color.a
	public void UpdateLutTex(CLutLayer[] lutLayers)
	{
		float step = 1.0f / 256;
		Color[] pixels = new Color[256 * 256];

		int layerCount = (lutLayers.Length >= 4 ? 4 : lutLayers.Length);

		for (int i = 0; i < layerCount; i++)
		{
			float fy = 0.0f; // the height in 0..1 range

			for (int height = 0; height < 256; height++)
			{
				if (fy >= lutLayers[i].minh && fy <= lutLayers[i].maxh)
				{
					float fx = 0.0f; // the slope in 0..1 range
					for (int slope = 0; slope < 256; slope++)
					{
						float slopeDiff = Mathf.Abs(lutLayers[i].slope - fx);
						float value;

						if (slopeDiff <= lutLayers[i].aperture)
						{
							float slopeFade = 1.0f - (slopeDiff / lutLayers[i].aperture);
							float heightFade = 1.0f -(fy - lutLayers[i].minh) / (lutLayers[i].maxh - lutLayers[i].minh);
							value = Mathf.Min(1.0f, Mathf.Max(0.0f, slopeFade * heightFade));
						}
						else
						{
							value = 0;
						}

						switch (i)
						{
							case 0:
								pixels[height * 256 + slope].r = value;
								break;

							case 1:
								pixels[height * 256 + slope].g = value;
								break;

							case 2:
								pixels[height * 256 + slope].b = value;
								break;

							case 3:
								pixels[height * 256 + slope].a = value;
								break;
						}

						fx += step;
					}
				}

				fy += step;
			}
		}

		m_LutTex.anisoLevel = 0;
		m_LutTex.filterMode = FilterMode.Bilinear;
		m_LutTex.wrapMode = TextureWrapMode.Clamp;

		// upload pixels
		m_LutTex.SetPixels(pixels);
		m_LutTex.Apply(false);

		//// ### SAVE TO DISK
		//byte[] bytes = m_LutTex.EncodeToPNG();
		//Debug.Log(bytes + " bytes in PNG");
		//string str = "d:/temp/lutTex.png";
		//if (System.IO.File.Exists(str)) System.IO.File.Delete(str);
		//System.IO.FileStream fs = new System.IO.FileStream(str, System.IO.FileMode.CreateNew);
		//System.IO.BinaryWriter w = new System.IO.BinaryWriter(fs);
		//w.Write(bytes);
		//w.Close();
		//fs.Close();
	}
}
