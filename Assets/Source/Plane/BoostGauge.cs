using UnityEngine;
using System.Collections;

public class BoostGauge : MonoBehaviour {
	private RectTransform indicator;
	// Use this for initialization
	void Start () {
		indicator = (RectTransform) this.transform;
		indicator.anchoredPosition = new Vector2(.25f*Screen.width-(.5f*Screen.width),.05f*Screen.height-(.5f*Screen.height));
	}
	
	// Update is called once per frame
	void Update () {

	}
	public void updateBoostGauge(float curBoostPercent){
		indicator.offsetMax = new Vector2(indicator.offsetMin.x+(curBoostPercent*16f),indicator.offsetMax.y);
		Debug.Log ("["+indicator.offsetMin.x+", "+indicator.offsetMax.x+"]");

	}
}
