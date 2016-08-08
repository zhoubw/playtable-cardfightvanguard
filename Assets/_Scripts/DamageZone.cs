using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public class DamageZone : MonoBehaviour {
	public List<GameObject> damages = new List<GameObject>();
	public GameObject[] damagePositions = new GameObject[6];
	public Text buttonText;

	public GameObject damageZone;
	public Transform pos1;
	public Transform pos2;
	public bool moved = false;

	public void add (GameObject card) {
		damages.Add (card);
		reorganize ();
		updateDamageCount ();
	}

	public void reorganize() {
		for (int i = 0; i < damages.Count; i++) {
			damages [i].transform.SetParent (damagePositions [i].transform);
			//damages [i].transform.localScale = damages [i].GetComponent<Card> ().defaultTransform.localScale;
			damages[i].transform.position = Vector3.zero;
		}
	}

	public GameObject remove(GameObject card) {
		damages.Remove (card);
		reorganize ();
		updateDamageCount ();
		return card;
	}

	public void toggleMove() {
		if (moved) {
			damageZone.transform.position = pos1.position;
		} else {
			damageZone.transform.position = pos2.position;
		}
		moved = !moved;
		//Debug.LogError (moved);
	}

	public void updateDamageCount() {
		buttonText.text = damageCount() + ""; //cast int to string
	}

	public int damageCount() {
		return damages.Count;
	}
}
