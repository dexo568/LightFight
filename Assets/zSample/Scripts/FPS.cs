//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

// this file is simplified from HUDFPS

using UnityEngine;
using System.Collections;

public class FPS : MonoBehaviour
{

	public float updateInterval = 0.5F;

	private float accum = 0;	// FPS accumulated over the interval
	private int frames = 0;		// Frames drawn over the interval
	private float timeleft;		// Left time for current interval

	string fpsStr;


	void Start()
	{
		timeleft = updateInterval;
	}


	void LateUpdate()
	{
		timeleft -= Time.deltaTime;
		accum += Time.timeScale / Time.deltaTime;
		++frames;

		if (timeleft <= 0.0)
		{
			float fps = accum / frames;
			fpsStr = System.String.Format("{0:F2} FPS", fps);

			timeleft = updateInterval;
			accum = 0.0F;
			frames = 0;
		}
	}


	void OnGUI()
	{
		GUILayout.Label(fpsStr);
	}

}
