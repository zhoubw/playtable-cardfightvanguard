using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;

public class CardManager : NetworkBehaviour {
	public GameObject CardPrefab;
	public static CardManager instance;

	public GameObject RedDeck;
	public Deck RedDeckScript;
	public GameObject RedHand;
	public Deck RedHandScript;
	public GameObject RedDamageZone;
	public GameObject RedGuardianCircle;
	public GameObject RedDropZone;
	public GameObject RedTriggerZone;
	public GameObject RedVanguard;
	public GameObject RedFrontLeftGuard;
	public GameObject RedBackLeftGuard;
	public GameObject RedFrontRightGuard;
	public GameObject RedBackRightGuard;
	public GameObject RedBackGuard;

	public GameObject[] RedZones = {
		RedVanguard,
		RedFrontLeftGuard,
		RedFrontRightGuard,
		RedBackGuard,
		RedBackLeftGuard,
		RedBackRightGuard
	};

	public GameObject BlueDeck;
	public Deck BlueDeckScript;
	public GameObject BlueHand;
	public Deck BlueHandScript;
	public GameObject BlueDamageZone;
	public GameObject BlueGuardianCircle;
	public GameObject BlueDropZone;
	public GameObject BlueTriggerZone;
	public GameObject BlueVanguard;
	public GameObject BlueFrontLeftGuard;
	public GameObject BlueBackLeftGuard;
	public GameObject BlueFrontRightGuard;
	public GameObject BlueBackRightGuard;
	public GameObject BlueBackGuard;

	public GameObject[] BlueZones = {
		BlueVanguard,
		BlueFrontLeftGuard,
		BlueFrontRightGuard,
		BlueBackGuard,
		BlueBackLeftGuard,
		BlueBackRightGuard
	};

	//reset on damage resolve
	public static List<ContinuousEffect> battleEffects = new List<ContinuousEffect> ();
	//reset on end phase
	public static List<ContinuousEffect> turnEffects = new List<ContinuousEffect> ();

	void Awake(){
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad (gameObject);
		} else
			Destroy (this);
	}
	/*
	void Start(){
		GameObject newCard = Instantiate (CardPrefab);
		Card newCardScript = newCard.GetComponent<Card> ();
		newCardScript.SetImage( ResourceManager.instance.BackTexture,  ResourceManager.instance.BackTexture);
	}*/

	public void GenerateMyDeck(Player.ColorOfTeam team){
		if (team == Player.ColorOfTeam.RED) {
			CreateDeckOfCards (GameStateManager.instance.RedSelectedDeck, CardManager.instance.RedDeck);
		} else if (team == Player.ColorOfTeam.BLUE) {
			CreateDeckOfCards (GameStateManager.instance.BlueSelectedDeck, CardManager.instance.BlueDeck);			
		}
	}

	public void CreateDeckOfCards(ResourceManager.AvailableDecks deckType, GameObject DeckLocation){
		string deckName = deckType.ToString ();
		Debug.LogError ("Creating deck : " + deckName);
		Texture2D backTexture = ResourceManager.instance.BackTexture;
		for (int i = 0; i < ResourceManager.instance.DeckCardListing [deckName].Count; i++) {
			string cardName = ResourceManager.instance.DeckCardListing [deckName].keys [i];
			for (int j = 0; j < (int)ResourceManager.instance.DeckCardListing [deckName] [i].f; j++) {
				GameObject newCard = Instantiate (CardPrefab, DeckLocation.transform.position, DeckLocation.transform.rotation) as GameObject;
				NetworkServer.Spawn (newCard);
				Card newCardScript = newCard.GetComponent<Card> ();
				if (isServer) {
					newCardScript.RpcSetCardProperties (cardName);
				} else {
					newCardScript.CmdSetCardProperties (cardName);
				}
				CmdAddCardToDeckList (newCard, DeckLocation);
			}
		}
	}

	[Command]
	public void CmdRemoveCardFromDeckList(GameObject card, GameObject deck){
		deck.GetComponent<Deck>().CmdRemoveCard (card);
	}

	[Command]
	public void CmdAddCardToDeckList(GameObject card, GameObject deck){
		card.GetComponent<Card> ().myZone = deck;
		deck.GetComponent<Deck> ().CmdAddCard (card);
	}

	[ClientRpc]
	public void RpcSetCardParent(GameObject myCard, GameObject myParent){
		myCard.transform.position = myParent.transform.position;
		myCard.transform.rotation = myParent.transform.rotation;
		myCard.transform.SetParent (myParent.transform);
	}

	[Command]
	public void CmdSetCardParent(GameObject myCard, GameObject myParent){
		myCard.transform.position = myParent.transform.position;
		myCard.transform.rotation = myParent.transform.rotation;
		myCard.transform.SetParent (myParent.transform);
		RpcSetCardParent(myCard, myParent);
	}

	[ClientRpc]
	public void RpcMoveCardToLocation(GameObject myCard, GameObject newLocation, bool faceUp){			
		Vector3 newPosition = new Vector3 (newLocation.transform.position.x, (newLocation.GetComponent<Deck>().GetLatestYPosition() + 0.014f), newLocation.transform.position.z);
		Debug.LogError ("New position is " + newPosition);
		myCard.transform.position = newPosition;
		myCard.transform.rotation = newLocation.transform.rotation;
		if (faceUp) {
			myCard.GetComponent<Card> ().flip ();
		}
	}

	[Command]
	public void CmdStandAllCards(Player.ColorOfTeam currentPlayer){
		if (currentPlayer == Player.ColorOfTeam.RED) {
			RedVanguard.GetComponent<Deck> ().CmdUnTapAllCards ();
			RedBackLeftGuard.GetComponent<Deck> ().CmdUnTapAllCards ();
			RedBackRightGuard.GetComponent<Deck> ().CmdUnTapAllCards ();
			RedFrontLeftGuard.GetComponent<Deck> ().CmdUnTapAllCards ();
			RedFrontRightGuard.GetComponent<Deck> ().CmdUnTapAllCards ();
			RedBackGuard.GetComponent<Deck> ().CmdUnTapAllCards ();
		} else {
			BlueVanguard.GetComponent<Deck> ().CmdUnTapAllCards ();
			BlueBackLeftGuard.GetComponent<Deck> ().CmdUnTapAllCards ();
			BlueBackRightGuard.GetComponent<Deck> ().CmdUnTapAllCards ();
			BlueFrontLeftGuard.GetComponent<Deck> ().CmdUnTapAllCards ();
			BlueFrontRightGuard.GetComponent<Deck> ().CmdUnTapAllCards ();
			BlueBackGuard.GetComponent<Deck> ().CmdUnTapAllCards ();
		}
	}


	[Command]
	public void CmdDrawCardToTriggerZone(Player.ColorOfTeam team){
		if (team == Player.ColorOfTeam.RED) {
			Deck deckScript = RedDeckScript;
			GameObject randomPhysicalCard = deckScript.syncDeck [Random.Range (0, deckScript.syncDeck.Count - 1)].card;
			CmdRemoveCardFromDeckList (randomPhysicalCard, RedDeck);
			CmdAddCardToDeckList (randomPhysicalCard, RedTriggerZone);
			RpcMoveCardToLocation (randomPhysicalCard, RedTriggerZone, true);
		}		
		else if (team == Player.ColorOfTeam.BLUE) {
			Deck deckScript = BlueDeckScript;
			GameObject randomPhysicalCard = deckScript.syncDeck [Random.Range (0, deckScript.syncDeck.Count - 1)].card;
			CmdRemoveCardFromDeckList (randomPhysicalCard, BlueDeck);
			CmdAddCardToDeckList (randomPhysicalCard, BlueTriggerZone);
			RpcMoveCardToLocation (randomPhysicalCard, BlueTriggerZone, true);
		}
	}

	[Command]
	public void CmdUpdateEffectiveValues() {
		//red
		foreach (GameObject zone in RedZones) {
			Card card = zone.GetComponent<Deck> ().GetCard ();
			card.effectivePower = card.myAttributes.power;
			card.effectiveCritical = card.myAttributes.critical;
		}
		//blue
		foreach (GameObject zone in BlueZones) {
			Card card = zone.GetComponent<Deck> ().GetCard ();
			card.effectivePower = card.myAttributes.power;
			card.effectiveCritical = card.myAttributes.critical;
		}

		foreach (ContinuousEffect effect in turnEffects) {
			effect.activate ();
		}
		foreach (ContinuousEffect effect in battleEffects) {
			effect.activate ();
		}
		//update text?
	}


}
