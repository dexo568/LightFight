using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BoostGauge : MonoBehaviour {
	private RectTransform indicator;
	public Image[] boostTriangles;
	// Use this for initialization
	void Start () {
		//indicator = (RectTransform) this.transform;
		//indicator.anchoredPosition = new Vector2(.25f*Screen.width-(.5f*Screen.width),.05f*Screen.height-(.5f*Screen.height));
		for(int i = 0; i < 11; i++){
			boostTriangles[i].canvasRenderer.SetAlpha(0);
		}
	}
	
	// Update is called once per frame
	void Update () {

	}
	public void updateBoostGauge(float curBoost){
		curBoost -= 30;
		//for(int i = 0; i < 11; i++){
		curBoost = Mathf.Max (curBoost, 0);
		curBoost = Mathf.Min (curBoost,109f);
		float curAlpha = ((curBoost - (((int)(curBoost/10))*10))/10) * 128;
		boostTriangles[(int)curBoost/10].canvasRenderer.SetAlpha((float)curAlpha);
		//Debug.Log ("A current boost of " + curBoost + "set triangle "  + (int)curBoost/10 + "'s alpha to " + curAlpha);
		//}
	}
}
