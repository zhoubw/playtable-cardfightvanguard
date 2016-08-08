using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DestroyIfNotMine : NetworkBehaviour {
	public Canvas MyUI;
	public GameObject MyCamera;

	void Start(){
		if (!isLocalPlayer) {
			MyUI.gameObject.SetActive (false);
			MyCamera.SetActive (false);
		}
	}
}

