using UnityEngine;
using System.Collections;

public class OrcaSwimTrigger : Trigger {
	private bool isSwimming = false;
	private Vector3 initialPosition;
	// Use this for initialization
	void Start () {
		initialPosition = transform.position;
	}
	
	// Update is called once per frame
	void Update () {
		if(isSwimming){
			transform.position = transform.position - transform.right;
		}
	}

	override public void trigger(){
		if(!isSwimming){
			transform.position = initialPosition;
			transform.position = new Vector3(transform.position.x, transform.position.y+15f, transform.position.z);
			isSwimming = true;
		}
	}
}
