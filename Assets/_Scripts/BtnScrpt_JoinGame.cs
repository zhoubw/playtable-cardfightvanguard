using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Networking.Match;

public class BtnScrpt_JoinGame : MonoBehaviour {	
	public Button MyButtonScript;
	public Text MyButtonText;

	public GameToJoin MyData;



	public void DisplayRoomInfo(){
		MyButtonText.text = MyData.LocalIp + ", " + MyData.GameNumber.ToString();
	}


	public void SetButtonAction(){
		MyButtonScript.onClick.AddListener (() => SetAndConnect ());
	}

	public void SetAndConnect(){
		GameManager GameManagerRef = GameObject.Find ("GameManager").GetComponent<GameManager> ();
		GameManagerRef.networkAddress = MyData.LocalIp;

		GameManagerRef.StartClient ();

	}

}
