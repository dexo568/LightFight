using UnityEngine;
using System.Collections;

public class MenuButton : MonoBehaviour {
	void Start () {}
	void Update () {}

	public void BeginLocalGame() {
		Application.LoadLevel("canyonracescene");
	}

	public void BeginNetworkGame() {
		Application.LoadLevel("OnlineLobbyScene");
	}

}
