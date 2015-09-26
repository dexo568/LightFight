using UnityEngine;
using System.Collections;

public class CLodSphere
{
	GameObject[] m_Spheres;
	Material m_Material;
	CPlanet m_Planet;


	public CLodSphere(bool outer, CPlanet planet)
	{
		CGlobal global = CGlobal.GetInstance();

		m_Planet = planet;

		if (outer)
		{
			// outer atmosphere
			m_Material = (Material)Material.Instantiate(global.OuterAtmosphereMaterial);
		}
		else
		{
			// inner atmosphere
			m_Material = (Material)Material.Instantiate(global.InnerAtmosphereMaterial);
		}

		m_Material.SetFloat("_planetRadius", m_Planet.m_Radius);
		m_Material.SetFloat("_atmosRadius", m_Planet.AtmosRadius);
		m_Material.SetColor("_color1", m_Planet.m_AtmosColor1);
		m_Material.SetColor("_color2", m_Planet.m_AtmosColor2);
		m_Material.SetFloat("_sunIntensity", m_Planet.m_sunIntensity);
		m_Material.SetFloat("_horizonHeight", m_Planet.m_HorizonHeight);
		m_Material.SetFloat("_horizonIntensity", m_Planet.m_HorizonIntensity);
		m_Material.SetFloat("_horizonPower", m_Planet.m_HorizonPower);
		m_Material.SetFloat("_minAlpha", m_Planet.m_MinAtmosAlpha);

		m_Spheres = new GameObject[3];

		// lod is 0:lowest, 1:mid, 2:highest detail
		for (int i = 0; i < 3; i++)
		{
			// save original parent transformations
			Vector3 parentPos = m_Planet.gameObject.transform.position;
			Quaternion parentQua = m_Planet.gameObject.transform.rotation;

			// reset parent transformations before assigning mesh data (so our vertices will be centered on the parent transform)
			m_Planet.gameObject.transform.position = Vector3.zero;
			m_Planet.gameObject.transform.rotation = Quaternion.identity;

			m_Spheres[i] = new GameObject("atmos lod " + i);
			m_Spheres[i].AddComponent<MeshFilter>();
			m_Spheres[i].AddComponent<MeshRenderer>();

			// ### outer atmosphere tesselation should be using AtmosSlices and AtmosStacks configs from CPatchConfig...
			m_Spheres[i].GetComponent<MeshFilter>().sharedMesh = global.MakeSphere((outer ? m_Planet.AtmosRadius : m_Planet.AtmosRadius * 1.25f), 15 * ((i + 1) * 4), 11 * ((i + 1) * 4));
			m_Spheres[i].transform.parent = m_Planet.gameObject.transform;

			m_Spheres[i].GetComponent<Renderer>().sharedMaterial = m_Material;

			// restore parent transformations
			m_Planet.gameObject.transform.position = parentPos;
			m_Planet.gameObject.transform.rotation = parentQua;

			// inactivate this gameobject
			m_Spheres[i].hideFlags = HideFlags.HideInHierarchy | HideFlags.HideInInspector | HideFlags.NotEditable;
		}
	}


	public void Destroy()
	{
		// destroy atmospheres spheres
		if (m_Spheres != null && m_Spheres.Length > 0)
		{
			for (int i = 0; i < 3; i++)
			{
				#if UNITY_EDITOR
					GameObject.DestroyImmediate(m_Spheres[i]);
				#else
					GameObject.Destroy(m_Spheres[i]);
				#endif
			}
			m_Spheres = null;
		}
	}


	public void Select(bool hasAtmosphere, int lod, Vector3 invSunDir, Vector3 invCamPos)
	{
		m_Spheres[0].active = m_Spheres[1].active = m_Spheres[2].active = false;
		m_Spheres[lod].active = hasAtmosphere;

		m_Spheres[lod].GetComponent<Renderer>().sharedMaterial.SetVector("_sunDir", invSunDir);
		m_Spheres[lod].GetComponent<Renderer>().sharedMaterial.SetVector("_camPos", invCamPos);
	}
}
