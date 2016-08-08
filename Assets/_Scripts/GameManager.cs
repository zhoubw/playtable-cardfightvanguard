using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : NetworkManager {
	private bool redSeat = true;
	private bool blueSeat = true;
	private bool spectateSeat = true;

	public bool RedSeat { get { return redSeat; } set { redSeat = value; } }
	public bool BlueSeat { get { return BlueSeat; } set { BlueSeat = value; } }
	public bool SpectateSeat { get { return SpectateSeat; } set { SpectateSeat = value; } }

	void Start () {
		maxConnections = 3;
	}
	PlayerController pc = new PlayerController();

	public override void OnServerAddPlayer(NetworkConnection conn, short playerControllerId){		

		Debug.LogError ("Spawnedme");
		var player = (GameObject)GameObject.Instantiate(playerPrefab, playerPrefab.transform.position, Quaternion.identity);
		NetworkServer.AddPlayerForConnection(conn, player, playerControllerId);
		if (conn.address == "localClient") {
			player.GetComponent<Player> ().PlayerHost ();
			Debug.LogError(conn.playerControllers [0].gameObject.name);
		} 
	}
		
	public override void OnServerDisconnect(NetworkConnection conn){
		Debug.LogError ("someone left");
		if (conn.playerControllers [0].gameObject.GetComponent<Player> ().myTeam == Player.ColorOfTeam.BLUE) {
			GameStateManager.instance.BlueSeat = true;
		} else if (conn.playerControllers [0].gameObject.GetComponent<Player> ().myTeam == Player.ColorOfTeam.RED) {
			GameStateManager.instance.RedSeat = true;
		}
		Destroy (conn.playerControllers [0].gameObject);
	}

	public string GetRoomData(){
		string data = "";
		data += ResourceManager.instance.AssignRandomRoomNumber ().ToString () + ",";
		if (redSeat) {
			data += "RED,";
		}
		if (blueSeat) {
			data += "BLUE,";
		}
		return data;
	}



}
