using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class BtnScript_JoinSeat :  NetworkBehaviour{

	public void JoinAsRed(){
		Debug.LogError ("changing states");
		CmdRedSeatTaken();
	}

	public void JoinAsBlue(){
		Debug.LogError ("changing states");
		CmdBlueSeatTaken();		
	}

	[Command]
	public void CmdRedSeatTaken(){
		if (isLocalPlayer) {
			GameStateManager.instance.RedSeat = false;
		}
	}

	[Command]
	public void CmdBlueSeatTaken(){
		if (isLocalPlayer) {
			GameStateManager.instance.BlueSeat = false;
		}
	}



}
