using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

public class Deck : NetworkBehaviour {
	public enum Position{NONE = 0, front, back};

	public Player.ColorOfTeam myTeam;
	public Position myPosition;

	public struct CardStruct{
		public GameObject card;
	}

	public class SyncListDeck:SyncListStruct<CardStruct>{
		
	}

	[SerializeField]
	public SyncListDeck syncDeck = new SyncListDeck();

	[Command]
	public void CmdAddCard(GameObject card){
		CardStruct cardStruct = new CardStruct ();
		cardStruct.card = card;
		syncDeck.Add (cardStruct);
	}

	[Command]
	public void CmdRemoveCard(GameObject card){
		CardStruct cardStruct = new CardStruct ();
		cardStruct.card = card;
		syncDeck.Remove (cardStruct);
	}

	[Command]
	public void CmdTapAllCards(){
		Debug.LogError ("Tapping all cards");
		foreach(CardStruct cs in syncDeck){
			cs.card.GetComponent<Card> ().RpcTapCard ();
		}
	}

	[Command]
	public void CmdUnTapAllCards(){
		foreach(CardStruct cs in syncDeck){
			cs.card.GetComponent<Card> ().RpcUntapCard ();
		}
	}

	public float GetLatestYPosition(){
		if (syncDeck.Count > 1) {
			Debug.LogError (syncDeck [syncDeck.Count - 2].card.transform.position.y);
			return syncDeck [syncDeck.Count - 2].card.transform.position.y;
		} else {
			Debug.LogError (transform.position.y);
			return transform.position.y;
		}
	}

	public bool contains(Card card) {
		foreach (CardStruct cs in syncDeck) {
			if (cs.card.Equals (card)) {
				return true;
			}
		}
		return false;
	}

	[Command]
	public void CmdSwapWith(GameObject deckObject) {
		Debug.LogError ("Swapping deck ");
		Deck deck = deckObject.GetComponent<Deck> ();
		SyncListDeck deck1 = this.syncDeck;
		SyncListDeck deck2 = deck.syncDeck;
		this.syncDeck = deck2;
		deck.syncDeck = deck1;
	}

	public GameObject GetCard(){
		GameObject returnThisCard = null;
		if (syncDeck.Count > 0) {
			returnThisCard = syncDeck [syncDeck.Count - 1].card;
		} 
		return returnThisCard;
	}
}
