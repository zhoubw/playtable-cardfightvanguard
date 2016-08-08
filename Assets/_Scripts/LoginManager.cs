using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class LoginManager : MonoBehaviour {	
	public static LoginManager instance;
	public Dictionary<string, GameToJoin> AvailableGames = new Dictionary<string, GameToJoin>();
	public GameObject buttonPrefab;
	public GameObject DisplayLoginPanel;
	public GameObject DisplayAvailableGamesPanel;
	public GameObject DisplayAvailableGamesPanelSub;
	public GameObject DisplayRoomInfo;
	public GameObject DisplayConnections;
	public GameManager GameManagerReference;
	public GameObject GameStateManagerPrefab;

	void Awake(){
		if (instance == null) {
			instance = this;
		} else
			Destroy (this);
		DontDestroyOnLoad (this);
	}

	public void HostGame(){
		NetworkDiscovery.instance.Initialize ();
		NetworkDiscovery.instance.StartAsServer ();
		DisplayConnections.SetActive (false);
		//notify game to start as host
		GameManagerReference.StartHost();

	}

	public void JoinGame(){
		NetworkDiscovery.instance.Initialize ();
		NetworkDiscovery.instance.StartAsClient ();
		DisplayConnections.SetActive (false);
		DisplayAvailableGamesPanel.SetActive (true);
		DisplayGames ();
	}

	public void RemoveGame(string ip){
		AvailableGames.Remove (ip);
		DisplayGames ();
	}

	public void AddGame(GameToJoin game){
		AvailableGames.Add (game.LocalIp, game);
		DisplayGames ();		
	}

	public void DisplayGames(){
		foreach (Transform child in DisplayAvailableGamesPanelSub.transform) {
			Destroy (child.gameObject);
		}
		foreach(GameToJoin game in AvailableGames.Values){
			CreateGamesButton (game);
		}
	}

	public void RefreshGames(){
		AvailableGames.Clear ();
		DisplayGames ();
	}

	void CreateGamesButton(GameToJoin game){
		GameObject gamebtn = Instantiate (buttonPrefab) as GameObject;
		BtnScrpt_JoinGame gamebtnScript = gamebtn.GetComponent<BtnScrpt_JoinGame> ();
		gamebtn.transform.SetParent(DisplayAvailableGamesPanelSub.transform);
		gamebtnScript.MyData = game;
		gamebtnScript.DisplayRoomInfo ();

		gamebtnScript.SetButtonAction ();
		gamebtnScript.MyButtonScript.onClick.AddListener(() => 	DisplayAvailableGamesPanel.SetActive (false));
		gamebtnScript.MyButtonScript.onClick.AddListener (() => DisplayLoginPanel.SetActive (false));
		Debug.LogError ("Disabling");
	}

	public void DisplaySeats(){
		foreach (Transform child in DisplayAvailableGamesPanel.transform) {
			Destroy (child.gameObject);
		}
	}		

	/*

public void DisplayGames(){
	Debug.LogError ("All available games");
	GameObject PanelOfGames;
	foreach (GameToJoin content in AvailableGames.Values) {
		PanelOfGames = GameObject.Find ("Canvas_Login/Panel/Panel");
		GameObject newButton = Instantiate (buttonPrefab) as GameObject;
		newButton.transform.SetParent (PanelOfGames.transform);
		newButton.GetComponent<Button>().onClick.AddListener(() => AddRoomButton(content));
		newButton.tag = content.LocalIp;
		}
	}
*/


	private void AddRoomButton(GameToJoin gameAvailable){
		Debug.LogError (gameAvailable.LocalIp);
	}



}
