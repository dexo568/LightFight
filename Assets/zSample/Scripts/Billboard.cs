//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

using UnityEngine;
using System.Collections;


[ExecuteInEditMode]
public class Billboard : MonoBehaviour
{
	public Camera m_Camera;


	// Use this for initialization
	void Start()
	{
		if (m_Camera == null)
		{
			Debug.Log("[ETHEREA1] Billboard " + this.name + " had no associated Camera. Main Camera is being selected as the Camera for this Billboard.");
			m_Camera = Camera.main;
		}
	}


	// Update is called once per frame
	void LateUpdate()
	{
		transform.LookAt(transform.position + m_Camera.transform.rotation * Vector3.up, m_Camera.transform.rotation * Vector3.back);
	}
}
