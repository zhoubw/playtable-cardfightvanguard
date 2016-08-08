using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class Pnl_RoomInfo : MonoBehaviour {
	public Button JoinAsRed;
	public Button JoinAsBlue;
	public Button JoinAsSpectator;

	public void EnableAvailableSeats(){
		DisableAllButtons ();
		if (GameStateManager.instance.RedSeat) {
			JoinAsRed.interactable = true;
		}
		if (GameStateManager.instance.BlueSeat) {
			JoinAsBlue.interactable = true;
		}
	}

	public void DisableAllButtons(){
		JoinAsRed.interactable = false;
		JoinAsBlue.interactable = false;
		JoinAsSpectator.interactable = false;
	}



}
