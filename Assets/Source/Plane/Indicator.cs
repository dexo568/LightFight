using UnityEngine;
using System.Collections;

public class Indicator : MonoBehaviour {
	public Camera reference;
	public GameObject tracked;

	void Start () {
				
	}

	void Update () {
		RectTransform rt = (RectTransform)this.transform;
		float screenX = (reference.WorldToViewportPoint(tracked.transform.position).x*reference.pixelWidth)+(reference.rect.xMax*Screen.width)-Screen.width;
		if (screenX < Mathf.Abs(rt.rect.x) + (reference.rect.xMin*Screen.width)-(Screen.width*.5f)){
			screenX = Mathf.Abs(rt.rect.x) + (reference.rect.xMin*Screen.width)-(Screen.width*.5f);
		}else if(screenX > -Mathf.Abs(rt.rect.x) + (reference.rect.xMax*Screen.width)-(Screen.width*.5f)){
			screenX = -Mathf.Abs(rt.rect.x) + (reference.rect.xMax*Screen.width)-(Screen.width*.5f);
		}
		float screenY = (reference.WorldToViewportPoint(tracked.transform.position).y*reference.pixelHeight)-(reference.pixelHeight*.5f);
		if (screenY < -reference.pixelHeight*.5f + Mathf.Abs(rt.rect.y)){
			screenY = -reference.pixelHeight*.5f + Mathf.Abs(rt.rect.y);
		}else if(screenY > reference.pixelHeight*.5f - Mathf.Abs(rt.rect.y)){
			screenY = reference.pixelHeight*.5f - Mathf.Abs(rt.rect.y);
		}


		rt.anchoredPosition = new Vector2(screenX, screenY);
		Debug.Log (reference.rect.center);
	}
}
