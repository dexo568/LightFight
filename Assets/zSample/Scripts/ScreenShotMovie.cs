//
// Etherea1 for Unity3D
// Written by Vander 'imerso' Nunes -- imerso@imersiva.com
//

// this file is based on the Unity3D wiki
// improvements:
// - Insert key toggles capture on/off (and changes frameRate accordingly)

using UnityEngine;
using System.Collections;


public class ScreenShotMovie : MonoBehaviour
{
	int frameRate = 25;
	string folder = "D:\\temp\\capture";
	string realFolder = "";

	bool capture;


	void Start ()
	{
	}


	void Update ()
	{
		if (Input.GetKeyDown(KeyCode.Insert))
		{
			capture = !capture;

			if (capture)
			{
				// find a folder that doesn't exist yet by appending numbers
				realFolder = folder;
				int count = 1;
				while (System.IO.Directory.Exists(realFolder))
				{
					realFolder = folder + count;
					count++;
				}

				// create the folder
				System.IO.Directory.CreateDirectory(realFolder);

				// change the framerate
				Time.captureFramerate = frameRate;
			}
			else
			{
				// restore the framerate
				Time.captureFramerate = 0;
			}
		}

		if (capture)
		{
			// name is "realFolder/0005 shot.png"
			var name = string.Format("{0}/{1:D04} shot.png", realFolder, Time.frameCount);

			// capture the screenshot
			Application.CaptureScreenshot(name);
		}
	}
}
