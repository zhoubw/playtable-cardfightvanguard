using UnityEngine;
using System.Collections;
using TouchScript;
using TouchScript.Gestures;
using System;

public class TestCardScript : MonoBehaviour {

	/*
	 List of card properties

*/
	
	public bool Snappable = false;

	void OnEnable(){
		GetComponent<ReleaseGesture> ().Released += ReleasedHandler;
		GetComponent<PressGesture> ().Pressed += PressHandler;
	}

	void OnDisable(){
		GetComponent<ReleaseGesture> ().Released -= ReleasedHandler;
		GetComponent<PressGesture> ().Pressed -= PressHandler;
	}

	public void ReleasedHandler(object sender, EventArgs e){
		Snappable = true;
		StartCoroutine (forafew ());
	}

	public void PressHandler(object sender, EventArgs e){
		Snappable = false;
	}

	public IEnumerator forafew(){
		yield return new WaitForSeconds (0.1f);
		Snappable = false;

	}

	//layer 28 = guardiancircle
	//layer 29 = card
	//layer 30 = vanguard
	//layer 31 = rearguard
	void OnTriggerStay(Collider other){
		Debug.LogError ("Collided with something");
		if(Snappable){
			if (other.gameObject.layer == 30) {
				Debug.LogError ("Collided with some vanguard");
				transform.position = other.transform.position;
				//something else happens if collided with vanguard
			}
			if (other.gameObject.layer == 31) {
				Debug.LogError ("Collided with some rearguard");
				transform.position = other.transform.position;
				//something else happens if collided with rearguard
			}


			if (other.gameObject.layer == 28) {
				Debug.LogError ("Collided with some vanguard");
				transform.position = other.transform.position;
				//something else happens if collided with guardian circle
			}


			// if(other objects
		}
	}



	/*
	 List of card functions here
	 */


}
