using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TouchScript.Gestures;
using System;

public class UICard : MonoBehaviour {
	public Player myPlayer;
	public Card PhysicalCard;
	public Collider myCollider;

	public UITrigger myLastTrigger;

	public GameObject myTargetObject = null;

	public GameObject TempPanel;
	public GameObject RealPanel;

	public GameObject HighlightCard;

	public LayerMask myLayerMask;
	[SerializeField]
	private Image myImage;

	bool KeepThisCard = false;

	void OnEnable(){
		GetComponent<PressGesture> ().Pressed += PressHandler;
		GetComponent<ReleaseGesture> ().Released += ReleaseHandler;
		GetComponent<TransformGesture> ().Transformed += TransformHandler;
	}


	void OnDisable(){
		GetComponent<PressGesture> ().Pressed -= PressHandler;
		GetComponent<ReleaseGesture> ().Released -= ReleaseHandler;
		GetComponent<TransformGesture> ().Transformed -= TransformHandler;
	}

	void ReleaseHandler(object sender, EventArgs e){		
		if (myLastTrigger == null) {
			gameObject.transform.SetParent (RealPanel.transform);
			Debug.LogError ("GoHome");
		} else if (myLastTrigger.gameObject.tag == "PullDown") {
			DropToWorld ();
		} else if (myLastTrigger.gameObject.tag == "PullUp") {
			gameObject.transform.SetParent (RealPanel.transform);
			myLastTrigger = null;		
		}
		myCollider.enabled = false;
	}

	void PressHandler(object sender, EventArgs e){	
		switch (myPlayer.GetMyState ()) {
		case GameStateManager.GameStates.ChoosingVanguard:
			{
				gameObject.transform.SetParent (TempPanel.transform);
				myCollider.enabled = true;
				break;
			}
		case GameStateManager.GameStates.InitialHand:
			{				
				ToggleCardToKeep ();
				break;
			}		
		case GameStateManager.GameStates.MainPhase:
			{
				gameObject.transform.SetParent (TempPanel.transform);
				myCollider.enabled = true;
				break;
			}
		case GameStateManager.GameStates.BattlePhase:
			{
				gameObject.transform.SetParent (TempPanel.transform);
				myCollider.enabled = true;
				break;
			}
		case GameStateManager.GameStates.CallGuardian:
			{
				gameObject.transform.SetParent (TempPanel.transform);
				myCollider.enabled = true;
				break;
			}

		default:
			break;
		}
	}

	public void ToggleCardToKeep(){
		KeepThisCard = !KeepThisCard;
		HighlightEffect (KeepThisCard);
		if (KeepThisCard) {
			myPlayer.CardsToKeep.Add (gameObject);
		} else
			myPlayer.CardsToKeep.Remove (gameObject);
	}

	public void HighlightEffect(bool toggle){
		HighlightCard.SetActive (toggle);
	}

	void TransformHandler(object sender, EventArgs e){ //as the card is moving and being held...
		//LayerMask mask = ~(1<<27);
		if (myLastTrigger != null && myLastTrigger.gameObject.tag == "PullDown") {
			Vector2 ScreenPosition = GetComponent<TransformGesture> ().ScreenPosition;
			Ray ray = Camera.main.ScreenPointToRay (ScreenPosition);
			RaycastHit hit;
			if (Physics.Raycast (transform.position, Vector3.down, out hit, 100f, myLayerMask)) {
				
				string targetTag = hit.transform.tag;
				Debug.LogError (targetTag + " layer mask is " + hit.transform.gameObject.layer);
				if (targetTag == "Vanguard" || targetTag == "Rearguard" || targetTag == "Guardian") {
					myTargetObject = hit.transform.gameObject;
				} else {
					myTargetObject = null;
				}
			}
		}
	}

	public void SetCardProperties(Card myActualCard){
		PhysicalCard = myActualCard;
		SetMyCardImage ();
	}

	private void SetMyCardImage(){
		Rect spriteRect = new Rect (0, 0, PhysicalCard.FrontTexture.width, PhysicalCard.FrontTexture.height);
		Sprite cardSprite = Sprite.Create (PhysicalCard.FrontTexture, spriteRect, myImage.sprite.pivot);
		myImage.sprite = cardSprite;
	}

	void OnTriggerEnter(Collider other){
		if (other.tag == "PullDown") {
			myLastTrigger = other.GetComponent<UITrigger> ();
			other.GetComponent<UITrigger> ().SendToPanel (gameObject);
			other.GetComponent<UITrigger> ().RealPanel.GetComponent<Animator> ().SetBool ("PullDown", true);
			other.GetComponent<UITrigger> ().RealPanel.GetComponent<Animator> ().SetBool ("PullUp", false);
		} else if (other.tag == "PullUp") {
			myLastTrigger = other.GetComponent<UITrigger> ();
			other.GetComponent<UITrigger> ().RealPanel.GetComponent<Animator> ().SetBool ("PullUp", true);
			other.GetComponent<UITrigger> ().RealPanel.GetComponent<Animator> ().SetBool ("PullDown", false);
		}
	}

	public void DropToWorld(){
		switch (myPlayer.GetMyState ()) {
		case GameStateManager.GameStates.ChoosingVanguard:
			{
				if (myTargetObject != null && myTargetObject.tag == "Vanguard") {
					myPlayer.myUIHandObjects.Remove (gameObject);
					myPlayer.CmdChangeMyNextState (GameStateManager.GameStates.SelectedVanguard);
					Debug.LogError ("changing my state to: selected vanguard");
					Debug.LogError ("Dropping Initial Vanguard!");
					myPlayer.CmdMoveCardToLocation (PhysicalCard.gameObject, myTargetObject, false);
					myPlayer.MoveCardToNewDeckList (PhysicalCard.gameObject, myPlayer.GetMyHandObject(), myPlayer.GetMyVanguardObject());
					myPlayer.DisplayHandPanel.GetComponent<Animator> ().SetBool ("PullUp", true);
					myPlayer.DisplayHandPanel.GetComponent<Animator> ().SetBool ("PullDown", false);
					StartCoroutine (CheckVanguards ());
				}
				break;
			}
		case GameStateManager.GameStates.MainPhase:
			{
				if (myTargetObject != null && myTargetObject.tag == "Vanguard") {
					Deck TargetDeckScript = myTargetObject.GetComponent<Deck> (); 
					if (PhysicalCard.myAttributes.grade == (myPlayer.GetHighestVanguardGrade() +1) || 
						PhysicalCard.myAttributes.grade  ==  myPlayer.GetHighestVanguardGrade() ) {

						Card oldVanguard = TargetDeckScript.GetCard ().GetComponent<Card>();
						myPlayer.CmdChangeVanguardGrade (PhysicalCard.myAttributes.grade);
						myPlayer.myUIHandObjects.Remove (gameObject);
						myPlayer.CmdMoveCardToLocation (PhysicalCard.gameObject, myTargetObject, true);
						myPlayer.MoveCardToNewDeckList (PhysicalCard.gameObject, myPlayer.GetMyHandObject (), myPlayer.GetMyVanguardObject ());
						myPlayer.DisplayHandPanel.GetComponent<Animator> ().SetBool ("PullUp", true);
						myPlayer.DisplayHandPanel.GetComponent<Animator> ().SetBool ("PullDown", false);
						Destroy (gameObject);

						oldVanguard.raiseFlag("RIDEONTOP");
						PhysicalCard.raiseFlag ("RIDE");
						Debug.LogError ("Riding New Vanguard");
					}
				}
				break;
			}
		case GameStateManager.GameStates.BattlePhase:
			{
				if (myTargetObject != null && myTargetObject.tag == "Rearguard") {
					Deck TargetDeckScript = myTargetObject.GetComponent<Deck> (); 
					if (PhysicalCard.myAttributes.grade <= myPlayer.GetHighestVanguardGrade()) {
						myPlayer.myUIHandObjects.Remove (gameObject);

						if (myTargetObject.GetComponent<Deck> ().syncDeck.Count > 0) {
							myPlayer.CmdMoveCardToLocation (myTargetObject.GetComponent<Deck> ().syncDeck [0].card.gameObject, myPlayer.GetMyDropZoneObject (), true);
							myPlayer.MoveCardToNewDeckList (myTargetObject.GetComponent<Deck> ().syncDeck [0].card.gameObject, myTargetObject, myPlayer.GetMyDropZoneObject());
						}

						myPlayer.CmdMoveCardToLocation (PhysicalCard.gameObject, myTargetObject, true);
						myPlayer.MoveCardToNewDeckList (PhysicalCard.gameObject, myPlayer.GetMyHandObject (), myTargetObject);
						myPlayer.DisplayHandPanel.GetComponent<Animator> ().SetBool ("PullUp", true);
						myPlayer.DisplayHandPanel.GetComponent<Animator> ().SetBool ("PullDown", false);
						Debug.LogError ("Placing new RearGuard");
						Destroy (gameObject);
						PhysicalCard.raiseFlag ("CALL");
					}
				}
				break;
			}
		case GameStateManager.GameStates.CallGuardian:
			{
				if (myTargetObject != null && myTargetObject.tag == "Guardian") {
					Deck TargetDeckScript = myTargetObject.GetComponent<Deck> (); 
					myPlayer.CmdMoveCardToLocation (PhysicalCard.gameObject, myTargetObject, true);
					myPlayer.MoveCardToNewDeckList (PhysicalCard.gameObject, myPlayer.GetMyHandObject(), myPlayer.GetMyGuardianCircleObject());
					myPlayer.DisplayHandPanel.GetComponent<Animator> ().SetBool ("PullUp", true);
					myPlayer.DisplayHandPanel.GetComponent<Animator> ().SetBool ("PullDown", false);
					myPlayer.CmdDelayChangeState ();
					Debug.LogError ("Placing guardian");
					Destroy (gameObject);
				}
				break;
			}
		default:
			break;
		}
	}

	public IEnumerator CheckVanguards(){		
		yield return new WaitForSeconds (0.5f);
		myPlayer.CmdCheckVanguards ();
		myPlayer.ToggleStandByPanel ();
		myPlayer.ClearMyUIObjects ();
		Destroy (gameObject);
	}

}
