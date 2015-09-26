using UnityEditor;
using UnityEngine;


[CustomEditor(typeof(CPlanet))]
class CPlanetEditor : Editor
{
    void OnSceneGUI()
	{
		CPlanet p = (CPlanet)target;

#if UNITY_EDITOR
		if (p.m_Wysiwyg)
		{
			p.LateUpdate();
		}
#endif
	}


	public override void OnInspectorGUI()
	{
		CPlanet p = (CPlanet)target;

		DrawDefaultInspector();

		if (GUI.changed)
		{
			if (!p.m_Wysiwyg)
			{
				p.Rebuild();
			}
		}
	}
}
