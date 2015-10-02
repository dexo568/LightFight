using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class LapCounter : MonoBehaviour {
	private Text counter;
	private int laps = 1;
	// Use this for initialization
	void Start () {
		counter = this.GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public bool updateLaps(){
		laps++;
		if(laps == 4) {
			return true;
		}else if(laps == 2){
			counter.fontSize = 40;
		}else{
			counter.fontSize = 60;
		}
		counter.text = "" + laps;
		return false;
	}

}
