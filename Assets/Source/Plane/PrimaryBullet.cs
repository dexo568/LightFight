using UnityEngine;
using System.Collections;

public class PrimaryBullet : MonoBehaviour {
	public GameObject bulletExplosion;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnCollisionEnter (Collision c) {
		GameObject explosion = (GameObject) Instantiate(bulletExplosion,transform.position,transform.rotation);
		Destroy(this.gameObject);
		Destroy(explosion,1);
	}
}
