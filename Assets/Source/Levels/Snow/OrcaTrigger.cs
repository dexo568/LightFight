using UnityEngine;
using System.Collections;

public class OrcaTrigger : Trigger {
	private bool isRotating = false;
	private bool hasBreached = false;
	private Vector3 initialRotation;
	private int breachCounter = 0;
	public ParticleSystem startSplash;
	public ParticleSystem startSplash2;
	public ParticleSystem endSplash;
	public ParticleSystem endSplash2;
	// Use this for initialization
	void Start () {
		initialRotation = transform.rotation.eulerAngles;
	}
	
	// Update is called once per frame

	void Update() 
	{
		if(isRotating){
			breachCounter++;
			//angle = Mathf.LerpAngle(transform.rotation.z, degree, Time.deltaTime);
			Vector3 myAngles = transform.rotation.eulerAngles;
			transform.rotation = Quaternion.Euler(myAngles.x, myAngles.y, myAngles.z+100*Time.deltaTime);			if(myAngles.z<5){
				hasBreached = true;
			}else if(hasBreached && myAngles.z>200){
				isRotating = false;
				transform.rotation = Quaternion.Euler(initialRotation.x, initialRotation.y, initialRotation.z);
			}
			if(breachCounter >= 125){
				endSplash.Play();
				endSplash2.Play();
				breachCounter = -9999;
			}
		}
	}

	override public void trigger(){
		if(!isRotating){
			Debug.Log("An orca was triggered");
			isRotating = true;
			hasBreached = false;
			startSplash.Play();
			startSplash2.Play();
			breachCounter = 0;
		}
	}
}
