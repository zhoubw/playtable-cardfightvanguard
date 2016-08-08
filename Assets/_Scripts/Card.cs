using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Networking;
using System;
using TouchScript.Gestures;

public class Card : NetworkBehaviour {	
	public GameObject Front;
	public GameObject Back;
	public Texture2D FrontTexture;
	public Texture2D BackTexture;

	public LayerMask myTargetedLayer;

	public CardAttributes.Properties myAttributes;

	bool CardFlipped = false;
	public Animator animateCard;


	public GameObject RaycastingZone;

	[SyncVar]
	public GameObject myZone;

	public bool Standing = true;

	[SyncVar]
	public int effectivePower = 0;
	[SyncVar]
	public int effectiveCritical = 1;

	public void SetImage(){
		Front.GetComponent<Renderer> ().material.mainTexture = FrontTexture;
		Back.GetComponent<Renderer> ().material.mainTexture = BackTexture;
	}

	[ClientRpc]
	public void RpcSetCardProperties(string cardName){
		BackTexture = ResourceManager.instance.BackTexture;
		FrontTexture = ResourceManager.instance.AllCardsTextures [cardName];
		JSONObject data = ResourceManager.instance.CardProperties [cardName];
		myAttributes = JsonUtility.FromJson<CardAttributes.Properties> (data.Print ());
		SetImage ();

	}

	[Command]
	public void CmdSetCardProperties(string cardName){
		BackTexture = ResourceManager.instance.BackTexture;
		FrontTexture = ResourceManager.instance.AllCardsTextures [cardName];
		JSONObject data = ResourceManager.instance.CardProperties [cardName];
		myAttributes = JsonUtility.FromJson<CardAttributes.Properties> (data.Print ());
		SetImage ();
		RpcSetCardProperties(cardName);
	}		

	public void FlipCard(object sender, EventArgs eventArgs){
		flip ();
	}

	public void flip(){
		gameObject.transform.eulerAngles = new Vector3 (0, gameObject.transform.eulerAngles.y, 180f);
	}

	[ClientRpc]
	public void RpcTapCard(){
		if (Standing == true) {
			Vector3 myZoneEuler = myZone.transform.eulerAngles;
			gameObject.transform.eulerAngles = new Vector3 (myZoneEuler.x, myZoneEuler.y -90f, 180f);
			Standing = false;
		}
	}

	[ClientRpc]
	public void RpcUntapCard(){
		if (Standing == false) {
			Vector3 myZoneEuler = myZone.transform.eulerAngles;
			gameObject.transform.eulerAngles = new Vector3 (myZoneEuler.x, myZoneEuler.y, 180f);
			Standing = true;
		}
	}


	void OnEnable(){
		GetComponent<TapGesture> ().Tapped += TapHandler;
		GetComponent<ReleaseGesture> ().Released += ReleaseHandler;
		GetComponent<TransformGesture>().Transformed += TransformHandler;
	}

	void OnDisable(){
		GetComponent<TapGesture> ().Tapped -= TapHandler;	
		GetComponent<ReleaseGesture> ().Released -= ReleaseHandler;
		GetComponent<TransformGesture> ().Transformed -= TransformHandler;
	}

	void TapHandler(object sender, EventArgs e)	{	//conditions to initiate attack
		Debug.LogError("Tapped");
		Deck myZoneDeck = myZone.GetComponent<Deck>();
		Attacking(myZoneDeck);
		BeingAttacked (myZoneDeck);
		BoostingCard (myZoneDeck);

	}



	void TransformHandler (object sender, EventArgs e)	{	
		if (isServer) {
			if (GameStateManager.instance.CurrentState == GameStateManager.GameStates.MainPhase) {
				Vector2 ScreenPosition = GetComponent<TransformGesture> ().ScreenPosition;
				Ray ray = Camera.main.ScreenPointToRay (ScreenPosition);
				RaycastHit hit;
				Vector3 RaycastStartingPositionOffSet = new Vector3 (transform.position.x, transform.position.y + 1.1f, transform.position.z);
				if (Physics.Raycast (RaycastStartingPositionOffSet, Vector3.down, out hit, 100f, myTargetedLayer)) {
					Debug.LogError (hit.transform.name);
					RaycastingZone = hit.transform.gameObject;
				} else
					RaycastingZone = null;
			}

		}
	}

	void ReleaseHandler(object sender, EventArgs e){
		if (isServer) {
			if (RaycastingZone != null) {
				GameObject oppositeZoneOfTarget = RaycastingZone.GetComponent<Rearguard> ().opposite;
				if (oppositeZoneOfTarget == myZone && (oppositeZoneOfTarget.layer == 31)) {	
					GameObject otherCard = RaycastingZone.GetComponent<Deck> ().GetCard ();
					swapLocationWith (otherCard, RaycastingZone);
					RaycastingZone = null;
				} else {
					Vector3 backToLocation = myZone.gameObject.transform.position;
					backToLocation.y = myZone.GetComponent<Deck> ().GetLatestYPosition ()+0.014f;
					gameObject.transform.position = backToLocation;
				}
			} else {
				Vector3 backToLocation = myZone.gameObject.transform.position;
				backToLocation.y = myZone.GetComponent<Deck> ().GetLatestYPosition ()+0.014f;
				gameObject.transform.position = backToLocation;
			}
		}
	}

	public void swapLocationWith(GameObject targetCard, GameObject otherZone) {
		Debug.LogError ("Swapping card locadtion");
		CardManager.instance.RpcMoveCardToLocation (gameObject, RaycastingZone, true);

		if (targetCard != null) {
			CardManager.instance.RpcMoveCardToLocation (targetCard, myZone, true);
			CardManager.instance.CmdRemoveCardFromDeckList (targetCard, otherZone);
			CardManager.instance.CmdAddCardToDeckList (targetCard, myZone);
			targetCard.GetComponent<Card> ().myZone = this.myZone;
		}

		CardManager.instance.CmdRemoveCardFromDeckList (gameObject, myZone);
		CardManager.instance.CmdAddCardToDeckList (gameObject, otherZone);
		myZone = otherZone;
	}




	private void Attacking(Deck MyZoneDeck){
		if (isServer) {
			if ((GameStateManager.instance.CurrentState == GameStateManager.GameStates.BattlePhase) && //if battle phase,	
			   (MyZoneDeck.myPosition == Deck.Position.front) && // if your position is front
			   MyZoneDeck.myTeam == GameStateManager.instance.CurrentPlayerTurn) {//and it is your turn

				if ((GameStateManager.instance.CurrentAttackingCharacter == null) && //if there is an attacking character
				   (Standing == true)) { //if you're standing
					Debug.LogError ("Attacking");
					GameStateManager.instance.CurrentAttackingCharacter = gameObject;
					MyZoneDeck.CmdTapAllCards ();
					raiseFlag ("ATTACK");
				} 
				/*else if ((GameStateManager.instance.CurrentAttackingCharacter == gameObject) && //if you are the attacking character
				          (Standing == false)) { //if you're not standing
					Debug.LogError ("UnAttacking");
					GameStateManager.instance.CurrentAttackingCharacter = null;
					MyZoneDeck.CmdUnTapAllCards ();
				}
				*/
			} 
		}
	}

	private void BeingAttacked(Deck MyZoneDeck){
		if (isServer) {
			if ((GameStateManager.instance.CurrentState == GameStateManager.GameStates.BattlePhase) && //if battle phase,	
			   (MyZoneDeck.myPosition == Deck.Position.front) && // if your position is front
			   (MyZoneDeck.myTeam != GameStateManager.instance.CurrentPlayerTurn) && //// and it is not you're turn, you're being attacked
			   (GameStateManager.instance.CurrentAttackingCharacter != null)) { //if there is an attacking character
				Debug.LogError ("Being Attacked");
				GameStateManager.instance.CmdBeingAttacked (gameObject, MyZoneDeck.myTeam);

			}
		}
	}

	private void BoostingCard(Deck MyZoneDeck){
		if (isServer) {
			if ((GameStateManager.instance.CurrentState == GameStateManager.GameStates.BattlePhase) && //if battlephase,
			    (MyZoneDeck.myPosition == Deck.Position.back) && //if your position is back
			    (MyZoneDeck.myTeam == GameStateManager.instance.CurrentPlayerTurn) && //and it is your turn
			    (GameStateManager.instance.CurrentAttackingCharacter != null) &&
			    (myZone.GetComponent<Rearguard> ().opposite == GameStateManager.instance.CurrentAttackingCharacter.GetComponent<Card> ().myZone)) { //if character is a rearguard to attacking character
			
				Debug.LogError ("Boosting");
				RpcTapCard ();
				raiseFlag ("BOOST");
			}
		}
	}
	
	public void raiseFlag(string flag) {
		foreach (int index in myAttributes.abilities) {
			Ability.abilities [index] (this, flag);
		}
	}
}
