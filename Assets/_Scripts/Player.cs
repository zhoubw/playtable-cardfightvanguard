using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityEngine.UI;
using TouchScript.Gestures;
using TouchScript.Behaviors;

public class Player : NetworkBehaviour {		
	public enum ColorOfTeam{NONE=0, RED, BLUE};

	public Button Red;
	public Button Blue;
	public GameObject PlayerLoginPanel;
	public GameObject PlayerSelectionPanel;
	public GameObject PlayerHandPanel;
	public GameObject DisplayHandPanel;
	public GameObject TempDisplayHandPanel;
	public GameObject StandByPanel;
	public Text StandByText;
	public GameObject KeepHandButton;
	public GameObject ConfirmInitialHandButton;
	public GameObject PlayerTriggerPanel;
	public GameObject MyCamera;

	public GameObject NextButton;

	public GameObject UICard;

	public Color myColor;

	public List<GameObject> myUIHandObjects = new List<GameObject> ();

	public List<GameObject> CardsToKeep = new List<GameObject>();

	[SyncVar]
	public ColorOfTeam myTeam = ColorOfTeam.NONE;

	[SyncVar]
	public ResourceManager.AvailableDecks mySelectedDeck = ResourceManager.AvailableDecks.NONE;

	void Start(){
		if (isLocalPlayer && !isServer) {
			Red.interactable = GameStateManager.instance.RedSeat;
			Blue.interactable = GameStateManager.instance.BlueSeat;
			Debug.LogError ("I am local player");
			Camera.main.gameObject.SetActive (false);
			GameStateManager.instance.EventUpdateSeating += PlayerSelectableButton;
			GameStateManager.instance.EventUpdateState += PlayerNextState;
			Debug.LogError ("added to event");
		}
	}

	public void PlayerSelectableButton(Player.ColorOfTeam team, bool value){
		if (team == Player.ColorOfTeam.RED) {
			Red.interactable = value;
		} else if (team == Player.ColorOfTeam.BLUE) {
			Blue.interactable = value;
		}
	}

	//	public enum GameStates{NONE = 0, Setup, StandBy, StartingVanguard, TurnOrder,  InitialHand, StandUp, TriggerPhase, DefendPhase, StandPhase, DrawPhase, RidePhase, MainPhase, BattlePhase, End};

	public void PlayerNextState(){
		if (isLocalPlayer) {
			Debug.LogError ("My state is: " + GetMyState ());
			switch (GetMyState ()) {
			case GameStateManager.GameStates.SelectedVanguard:
				{
					StandByText.text = "Determining turn order..";
					CmdChangeMyNextState (GameStateManager.GameStates.ChoosingTurnOrder);
					break;
				}
			case GameStateManager.GameStates.ChoosingTurnOrder:
				{
					CmdChangeMyNextState (GameStateManager.GameStates.InitialHand);
					StandByText.text = "Red goes first!";
					StartCoroutine (DisplayInitialHand ());
					Debug.LogError ("Showing initial hand");
					break;
				}
			case GameStateManager.GameStates.DrawPhase:
				{
					Debug.LogError ("Drawing my card!");
					DrawCard ();
					CmdChangeMyNextState (GameStateManager.GameStates.RidePhase);
					break;
				}
			case GameStateManager.GameStates.RidePhase:
				{
					Debug.LogError ("RIDE PHASE");
					CmdChangeMyNextState (GameStateManager.GameStates.MainPhase);
					CmdMoveToVanguardLocation ();
					PlayerTriggerPanel.SetActive (true);
					NextButton.SetActive (true);
					TransformerOnCards (true);
					break;
				}
			case GameStateManager.GameStates.MainPhase:
				{					
					Debug.LogError ("Main phase!");
					CmdChangeMyNextState (GameStateManager.GameStates.BattlePhase);
					CmdMoveToVanguardLocation ();
					PlayerTriggerPanel.SetActive (true);
					NextButton.SetActive (true);
					TransformerOnCards (true);
					break;
				}
			case GameStateManager.GameStates.BattlePhase:
				{
					CmdChangeMyNextState (GameStateManager.GameStates.NONE);
					CmdMoveToMyHomeLocation ();
					Debug.LogError ("End Phase");
					NextButton.SetActive (true);
					TransformerOnCards (false);
					PlayerTriggerPanel.SetActive (false);
					break;
				}
			case GameStateManager.GameStates.NONE:
				{
					NextButton.SetActive (false);
					TransformerOnCards (false);
					PlayerTriggerPanel.SetActive (false);
					CmdMoveToMyHomeLocation ();
					break;
				}			
			case GameStateManager.GameStates.CallGuardian:
				{
					CmdChangeMyNextState (GameStateManager.GameStates.DoneCallingGuardian);
					Debug.LogError ("Call Guardian! PHASE");
					PlayerTriggerPanel.SetActive (true);
					NextButton.SetActive (true);
					TransformerOnCards (true);
					CmdMoveToGuardianLocation ();
					break;
				}
			case GameStateManager.GameStates.DoneCallingGuardian:
				{
					CmdMoveToMyHomeLocation ();
					PlayerTriggerPanel.SetActive (false);
					NextButton.SetActive (false);
					TransformerOnCards (false);
					break;
				}
			case GameStateManager.GameStates.Standby:
				{
					CmdMoveToMyHomeLocation ();
					PlayerTriggerPanel.SetActive (false);
					NextButton.SetActive (false);
					TransformerOnCards (false);
					break;
				}
			case GameStateManager.GameStates.DriveCheck:
				{
					DrawCardFromTrigger ();
					CmdChangeMyNextState (GameStateManager.GameStates.Standby);
					PlayerTriggerPanel.SetActive (false);
					NextButton.SetActive (false);
					TransformerOnCards (false);
					break;
			}
			case GameStateManager.GameStates.DamageCheck:
				{
					DrawCardFromTrigger ();
					CmdChangeMyNextState (GameStateManager.GameStates.Standby);
					PlayerTriggerPanel.SetActive (false);
					NextButton.SetActive (false);
					TransformerOnCards (false);
					break;
				}
			default:
				break;
			}			
		}
	}

	public void PlayerHost(){
		if (isServer) {
			PlayerLoginPanel.SetActive (false);
			PlayerSelectionPanel.SetActive (false);
			MyCamera.SetActive (false);
		}
	}

	public void PlayerRedSelected(){
		if (isLocalPlayer) {
			CmdChangeMyNextState(GameStateManager.GameStates.Setup);
			PlayerSelectionPanel.gameObject.SetActive (true);
			gameObject.layer = 10;
			Camera MyCameraScript = MyCamera.GetComponent<Camera> ();
			MyCameraScript.cullingMask = ~(1 << 11); //Do not see blue player stuff
			ChangeMyColor(Color.red);
			PlayerLoginPanel.gameObject.SetActive (false);
		}
	}

	public void PlayerBlueSelected(){
		if (isLocalPlayer) {
			CmdChangeMyNextState(GameStateManager.GameStates.Setup);
			PlayerSelectionPanel.gameObject.SetActive (true);
			gameObject.layer = 11;
			Camera MyCameraScript = MyCamera.GetComponent<Camera> ();	
			MyCameraScript.cullingMask = ~(1 << 10); //do not see red player stuff
			ChangeMyColor(Color.blue);
			PlayerLoginPanel.gameObject.SetActive (false);
		}
	}

	private void ChangeMyColor(Color newColor){
		PlayerSelectionPanel.GetComponent<Image> ().color = newColor;
		DisplayHandPanel.GetComponent<Image> ().color = newColor;
		StandByPanel.GetComponent<Image> ().color = newColor;
	}

	public void ToggleStandByPanel(){
		StandByPanel.SetActive (true);
		StandByText.text = "Waiting on Opponent...";
	}		

	[Command]
	public void CmdChangeMyNextState(GameStateManager.GameStates state){
			if (myTeam == ColorOfTeam.RED) {
				GameStateManager.instance.RedState = state;
				Debug.LogError ("Changing red State to: " + state);
			} else if (myTeam == ColorOfTeam.BLUE) {
				GameStateManager.instance.BlueState = state;
				Debug.LogError ("Changing blue State to: " + state);
			}
	}

	[Command]
	public void CmdSelectedNarukamiDeck(){
		mySelectedDeck = ResourceManager.AvailableDecks.Narukami;
	}

	[Command]
	public void CmdSelectedGoldPaladinDeck(){
		mySelectedDeck = ResourceManager.AvailableDecks.GoldPaladin;
	}

	[Command]
	public void CmdConfirmDeckSelection(){
		Debug.LogError ("Calling confirmdeckselection");
		if (myTeam == ColorOfTeam.RED) {
			Debug.LogError ("RedSelected");
			GameStateManager.instance.RedSelectedDeck = mySelectedDeck;
			CmdGenerateDeck ();
		} else if (myTeam == ColorOfTeam.BLUE) {
			Debug.LogError ("BlueSelected");
			GameStateManager.instance.BlueSelectedDeck = mySelectedDeck;
			CmdGenerateDeck ();
		}
	}

	[Command]
	public void CmdChangeVanguardGrade(int grade){
		if(myTeam == ColorOfTeam.RED){
			GameStateManager.instance.RedVanguardRank = grade;
		}
		else if(myTeam == ColorOfTeam.BLUE){
			GameStateManager.instance.BlueVanguardRank = grade;
		}
	}

	public int GetHighestVanguardGrade(){
		if(myTeam == ColorOfTeam.RED){
			return GameStateManager.instance.RedVanguardRank;
		}
		else {
			return GameStateManager.instance.BlueVanguardRank;
		}
	}

	//gamestate command
	[Command]
	public void CmdChangeGameState(){
		if(isServer)
			GameStateManager.instance.ChangeGameState ();
	}

	//temp command
	[Command]
	public void CmdGenerateDeck(){
		CardManager.instance.GenerateMyDeck (myTeam);
	}

	[Command]
	public void CmdRedSeatTaken(){
		myTeam = ColorOfTeam.RED;
		GameStateManager.instance.CmdChangeSeat (myTeam, false);
		CmdMoveToMyHomeLocation ();
	}

	[Command]
	public void CmdBlueSeatTaken(){
		myTeam = ColorOfTeam.BLUE;
		GameStateManager.instance.CmdChangeSeat (myTeam, false);
		CmdMoveToMyHomeLocation ();
	}


	[Command]
	public void CmdMoveToMyHomeLocation(){
		if (myTeam == ColorOfTeam.RED) {
			RpcMoveToLocationRedHome ();
		} 
		else {
			RpcMoveToLocationBlueHome ();
		}
	}

	[Command]
	public void CmdMoveToGuardianLocation(){
		if (myTeam == ColorOfTeam.RED) {
			RpcMoveToLocationRedGuardianCircle ();
		} else {
			RpcMoveToLocationBlueGuardianCircle ();
		}
	}
		
	[Command]
	public void CmdMoveToVanguardLocation(){
		if (myTeam == ColorOfTeam.RED) {
			RpcMoveToLocationRedVanguard ();
		} 
		else {
			RpcMoveToLocationBlueVanguard ();

		}
	}

	[Command]
	public void CmdMoveCardToLocation(GameObject myCard, GameObject newLocation, bool faceUp){		
		CardManager.instance.RpcMoveCardToLocation (myCard, newLocation, faceUp);
	}

	[Command]
	public void CmdCheckVanguards(){
		if (myTeam == ColorOfTeam.RED) {
			GameStateManager.instance.RedVanguardSelected = true;
		} else if (myTeam == ColorOfTeam.BLUE) {
			GameStateManager.instance.BlueVanguardSelected = true;
		}
		GameStateManager.instance.CheckInitialVanguards ();
	}

	public void KeepSelectedCards(){
		if (isLocalPlayer) {
			int count = 0;
			while (count <= myUIHandObjects.Count-1) {
				GameObject uiCard = myUIHandObjects [count];
				if (!CardsToKeep.Contains (uiCard)) {
					MoveCardToNewDeckList(uiCard.GetComponent<UICard>().PhysicalCard.gameObject, GetMyHandObject(), GetMyDeckObject());
					CmdMoveCardToLocation (uiCard.GetComponent<UICard> ().PhysicalCard.gameObject, GetMyDeckObject (), false);
					myUIHandObjects.Remove (uiCard);
					Destroy (uiCard);
				} else {
					uiCard.GetComponent<UICard> ().HighlightEffect (false);
					count++;
				}
			}
			StartCoroutine (Mulligan (count));
		}
	}

	public IEnumerator Mulligan(int startingNum){		
		Deck deckScript = GetMyDeckScript ();
		for (int i = startingNum; i < 5; i++) {
			yield return new WaitForSeconds (0.3f);
			GameObject randomPhysicalCard = deckScript.syncDeck [Random.Range (0, deckScript.syncDeck.Count - 1)].card;
			MoveCardToNewDeckList(randomPhysicalCard, GetMyDeckObject(), GetMyHandObject());
			CmdMoveCardToLocation (randomPhysicalCard, GetMyHandObject (), false);
			CreateUICardToHand (randomPhysicalCard.GetComponent<Card>(), false);
		}
		CardsToKeep.Clear ();
		ConfirmInitialHandButton.SetActive (true);
	}

	public void DrawCard(){
		Deck deckScript = GetMyDeckScript ();
		GameObject randomPhysicalCard = deckScript.syncDeck [Random.Range (0, deckScript.syncDeck.Count - 1)].card;
		MoveCardToNewDeckList(randomPhysicalCard, GetMyDeckObject(), GetMyHandObject());
		CreateUICardToHand (randomPhysicalCard.GetComponent<Card>(), false);
	}

	public void DrawCardFromTrigger(){
		Debug.LogError ("getting card from trigger");
		if (myTeam == GameStateManager.instance.CurrentPlayerTurn) {
			Deck TriggerZoneDeck = GetMyTriggerZone().GetComponent<Deck> ();
			GameObject TriggerCard = TriggerZoneDeck.GetCard ();
			MoveCardToNewDeckList (TriggerCard, GetMyTriggerZone(), GetMyHandObject ());
			CmdMoveCardToLocation (TriggerCard, GetMyHandObject (), false);
			Debug.LogError ("Creating card to my hand");
			CreateUICardToHand (TriggerCard.GetComponent<Card> (), false);
		} else {
			Deck TriggerZoneDeck = GetMyTriggerZone().GetComponent<Deck> ();
			GameObject TriggerCard = TriggerZoneDeck.GetCard ();
			MoveCardToNewDeckList (TriggerCard, GetMyTriggerZone(), GetMyDamageZone ());
			//CmdMoveCardToLocation (TriggerCard, GetMyDamageZone (), false);
			GetMyDamageZone().GetComponent<DamageZone>().add(TriggerCard);
		}
	}


	[Command]
	public void CmdConfirmInitialHand(){
		if (myTeam == ColorOfTeam.RED) {
			GameStateManager.instance.RedInitialHandSelected = true;
		} else if (myTeam == ColorOfTeam.BLUE) {
			GameStateManager.instance.BlueInitialHandSelected = true;
		}
		GameStateManager.instance.CheckInitialHands ();
	}

	[ClientRpc]
	void RpcMoveToLocationRedHome(){		
		if (isLocalPlayer) {
			MyCamera.transform.position = CameraPositions.instance.RedHomePosition.position;
			MyCamera.transform.rotation = CameraPositions.instance.RedHomePosition.rotation;
		}
	}

	[ClientRpc]
	void RpcMoveToLocationBlueHome(){
		if (isLocalPlayer) {
			MyCamera.transform.position = CameraPositions.instance.BlueHomePosition.position;
			MyCamera.transform.rotation = CameraPositions.instance.BlueHomePosition.rotation;
		}
	}

	[ClientRpc]
	void RpcMoveToLocationRedVanguard(){		
		if (isLocalPlayer) {
			MyCamera.transform.position = CameraPositions.instance.RedVanguardCameraPosition.position;
			MyCamera.transform.rotation = CameraPositions.instance.RedVanguardCameraPosition.rotation;
		}
	}

	[ClientRpc]
	void RpcMoveToLocationBlueVanguard(){
		if (isLocalPlayer) {
			MyCamera.transform.position = CameraPositions.instance.BlueVanguardCameraPosition.position;
			MyCamera.transform.rotation = CameraPositions.instance.BlueVanguardCameraPosition.rotation;
		}
	}

	[ClientRpc]
	void RpcMoveToLocationRedGuardianCircle(){
		if (isLocalPlayer) {
			MyCamera.transform.position = CameraPositions.instance.RedGuardianCircleCameraPosition.position;
			MyCamera.transform.rotation = CameraPositions.instance.RedGuardianCircleCameraPosition.rotation;
		}
	}

	[ClientRpc]
	void RpcMoveToLocationBlueGuardianCircle(){
		if (isLocalPlayer) {
			MyCamera.transform.position = CameraPositions.instance.BlueGuardianCircleCameraPosition.position;
			MyCamera.transform.rotation = CameraPositions.instance.BlueGuardianCircleCameraPosition.rotation;
		}
	}

	public void DisplayInitialVanguards(){ //Move camera to position, then display cards
		CmdChangeMyNextState (GameStateManager.GameStates.ChoosingVanguard);
		if (myTeam == ColorOfTeam.RED) {
			MyCamera.transform.position = CameraPositions.instance.RedVanguardCameraPosition.position;
		} else if (myTeam == ColorOfTeam.BLUE) {
			MyCamera.transform.position = CameraPositions.instance.BlueVanguardCameraPosition.position;
		}
		StartCoroutine (DisplayVanguards ());
	}

	private IEnumerator DisplayVanguards(){
		yield return new WaitForSeconds (2.0f);
		Deck deckScript = GetMyDeckScript ();
		if (deckScript != null) {
			for (int i = 0; i < ResourceManager.instance.DeckCardListing [mySelectedDeck.ToString()].keys.Count; i++) {
				string SingleCardName = ResourceManager.instance.DeckCardListing [mySelectedDeck.ToString ()].keys [i].ToString();
				for (int j = deckScript.syncDeck.Count -1 ; j >= 0; j--) {
					Card cardInDeck = deckScript.syncDeck [j].card.GetComponent<Card> ();
					string PhysicalCardName = cardInDeck.myAttributes.name;
					if (cardInDeck.myAttributes.grade == 0 && SingleCardName == PhysicalCardName) {
						MoveCardToNewDeckList (cardInDeck.gameObject, GetMyDeckObject(), GetMyHandObject ());
						CmdMoveCardToLocation (cardInDeck.gameObject, GetMyHandObject (), false);
						CreateUICardToHand (cardInDeck, true);
						j = 0;			
					}
				}
			}
		}
	}

	private IEnumerator DisplayInitialHand(){
		CmdMoveToMyHomeLocation ();
		PlayerTriggerPanel.SetActive (false);
		KeepHandButton.SetActive (true);
		yield return new WaitForSeconds (2.0f);
		//turns off standy panel and turns on initial hand panel
		StandByPanel.SetActive (false);

		Deck deckScript = GetMyDeckScript ();
		if (deckScript != null) {
			for (int i = 0; i < 5; i++) {
				yield return new WaitForSeconds (0.1f);
				GameObject randomPhysicalCard = deckScript.syncDeck [Random.Range (0, deckScript.syncDeck.Count - 1)].card;
				MoveCardToNewDeckList (randomPhysicalCard, GetMyDeckObject(), GetMyHandObject ());
				CmdMoveCardToLocation (randomPhysicalCard, GetMyHandObject (), false);
				CreateUICardToHand (randomPhysicalCard.GetComponent<Card>(), false);
			}						
		}
	}

	public void MoveCardToNewDeckList(GameObject cardObject, GameObject oldDeck, GameObject newDeck){
		CmdRemoveCardFromDeck (cardObject, oldDeck);
		CmdAddCardToDeck (cardObject, newDeck);
	}


	[Command]
	public void CmdRemoveCardFromDeck(GameObject card, GameObject deck){
		CardManager.instance.CmdRemoveCardFromDeckList (card, deck);
	}

	[Command]
	public void CmdAddCardToDeck(GameObject card, GameObject deck){
		CardManager.instance.CmdAddCardToDeckList (card, deck);
	}

	public void TransformerOnCards(bool toggle){
		foreach(GameObject cardObject in myUIHandObjects){
			cardObject.GetComponent<Transformer> ().enabled = toggle;
		}
	}

	[Command]
	public void CmdDelayChangeState(){
		GameStateManager.instance.StartCoroutine (GameStateManager.instance.DelayChangeState ());
	}


	public void CreateUICardToHand(Card PhysicalCardProperties, bool TransformEnable){
		Debug.LogError ("Creating new card");
		GameObject newUICard = Instantiate (UICard) as GameObject;
		myUIHandObjects.Add (newUICard);
		newUICard.transform.SetParent (DisplayHandPanel.transform);
		UICard newUICardScript = newUICard.GetComponent<UICard> ();
		newUICardScript.SetCardProperties (PhysicalCardProperties);
		newUICardScript.myPlayer = this;
		newUICard.transform.localPosition = UICard.transform.localPosition;
		newUICard.transform.localRotation = UICard.transform.localRotation;
		newUICard.transform.localScale = UICard.transform.localScale;
		newUICardScript.TempPanel = TempDisplayHandPanel;
		newUICardScript.RealPanel = DisplayHandPanel;
		newUICard.GetComponent<Transformer> ().enabled = TransformEnable;
	}

	public void ClearMyUIObjects(){
		foreach (GameObject cardObject in myUIHandObjects) {
			MoveCardToNewDeckList (cardObject.GetComponent<UICard> ().PhysicalCard.gameObject, GetMyHandObject (), GetMyDeckObject ());
			Destroy (cardObject);
		}
		myUIHandObjects.Clear ();
	}

	public GameStateManager.GameStates GetMyState(){
		if (myTeam == ColorOfTeam.RED) {
			return GameStateManager.instance.RedState;
		} else{
			return GameStateManager.instance.BlueState;
		}
	}


	public GameObject GetMyDeckObject(){
		if (myTeam == ColorOfTeam.RED) {
			return CardManager.instance.RedDeck;
		} else
			return CardManager.instance.BlueDeck;
	}
	public Deck GetMyDeckScript(){
		if (myTeam == ColorOfTeam.RED) {
			return CardManager.instance.RedDeckScript;	
		} else {
			return CardManager.instance.BlueDeckScript;
		}
	}

	public GameObject GetMyHandObject(){
		if (myTeam == ColorOfTeam.RED) {
			return CardManager.instance.RedHand;
		} else
			return CardManager.instance.BlueHand;
	}
	public Deck GetMyHandScript(){
		if (myTeam == ColorOfTeam.RED) {
			return CardManager.instance.RedHandScript;	
		} else {
			return CardManager.instance.BlueHandScript;
		}
	}

	public GameObject GetMyVanguardObject(){
		if (myTeam == ColorOfTeam.RED) {
			return CardManager.instance.RedVanguard;
		} else
			return CardManager.instance.BlueVanguard;
	}
		
	public GameObject GetMyDropZoneObject(){
		if (myTeam == ColorOfTeam.RED) {
			return CardManager.instance.RedDropZone;
		} else
			return CardManager.instance.BlueDropZone;
	}

	public GameObject GetMyGuardianCircleObject(){
		if (myTeam == ColorOfTeam.RED) {
			return CardManager.instance.RedGuardianCircle;
		} else
			return CardManager.instance.BlueGuardianCircle;
		
	}

	public GameObject GetMyDamageZone(){
		if (myTeam == ColorOfTeam.RED) {
			return CardManager.instance.RedDamageZone;
		} else
			return CardManager.instance.BlueDamageZone;
	}

	public GameObject GetMyTriggerZone(){
		if (myTeam == ColorOfTeam.RED) {
			return CardManager.instance.RedTriggerZone;
		}
		return CardManager.instance.BlueTriggerZone;
	
	}


}
