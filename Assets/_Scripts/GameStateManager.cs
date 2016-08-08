using UnityEngine;
using System.Collections;
using UnityEngine.Networking;
using System.Collections.Generic;
using UnityEngine.UI;

public class GameStateManager :  NetworkBehaviour{
	public enum GameStates{NONE = 0, Setup, Standby, ChoosingVanguard, SelectedVanguard, ChoosingTurnOrder, TurnOrderSelected,  AttackAndBoost, CallGuardian, DoneCallingGuardian, DriveCheck, DamageCompare, DamageCheck, DamageResolve, InitialHand, StandUp, TriggerPhase, DefendPhase, StandPhase, DrawPhase, RidePhase, MainPhase, BattlePhase, End};

	#region Update Seating delegates
	public delegate void ChangeSeatDelegate(Player.ColorOfTeam team, bool value);
	[SyncEvent]
	public event ChangeSeatDelegate EventUpdateSeating;
	#endregion

	#region Update State delegates
	public delegate void ChangeStatetDelegate();
	[SyncEvent]
	public event ChangeStatetDelegate EventUpdateState;
	#endregion

	[SyncVar]
	public Player.ColorOfTeam CurrentPlayerTurn = Player.ColorOfTeam.NONE;

	[SyncVar]
	public GameStates RedState = GameStates.NONE;

	[SyncVar]
	public GameStates BlueState = GameStates.NONE;

	[SyncVar]
	public GameStates CurrentState = GameStates.NONE;

	[SyncVar]
	public bool RedVanguardSelected;

	[SyncVar]
	public bool BlueVanguardSelected;

	[SyncVar]
	public bool RedInitialHandSelected;

	[SyncVar]
	public bool BlueInitialHandSelected;

	public static GameStateManager instance;

	[SyncVar]
	public int RedVanguardRank = 0;

	[SyncVar]
	public int BlueVanguardRank = 0;

	[SyncVar]
	public ResourceManager.AvailableDecks RedSelectedDeck = ResourceManager.AvailableDecks.NONE;

	[SyncVar]
	public ResourceManager.AvailableDecks BlueSelectedDeck = ResourceManager.AvailableDecks.NONE;

	[SyncVar]
	public bool RedSeat = true;

	[SyncVar]
	public bool BlueSeat = true;

	[SyncVar]
	public int numberOfPlayers = 0;

	[SyncVar]
	public GameObject CurrentAttackingCharacter;

	[SyncVar]
	public GameObject CurrentDefendingCharacter;

	#region message/button/ on TT
	public GameObject SystemCanvas;
	public GameObject TTConfirmButton;
	public Text GlobalMessage;

	#endregion
	void Awake(){
		if (instance == null) {
			instance = this;
		} else
			Destroy (this);
	}

	void Start(){
		if (isServer) {
			EventUpdateSeating += ToggleSeat;
			EventUpdateState += ChangeState;
			CurrentState = GameStates.Setup;
		}
	}

	#region Update Seats
	[Command]
	public void CmdChangeSeat(Player.ColorOfTeam team, bool value){
		Debug.LogError ("Updating seats");
		EventUpdateSeating (team, value);
	}
	public void ToggleSeat(Player.ColorOfTeam team, bool value){
		if (team == Player.ColorOfTeam.RED) {
			RedSeat = value;
		} else if (team == Player.ColorOfTeam.BLUE) {
			BlueSeat = value;
		}
	}
	#endregion


	#region Update State
	public void ChangeGameState(){
		Debug.LogError ("Changing States");
		EventUpdateState ();

	}

	//	public enum GameStates{NONE = 0, Setup, ChoosingVanguard, SelectedVanguard, ChoosingTurnOrder, TurnOrderSelected,  InitialHand, StandUp, TriggerPhase, DefendPhase, StandPhase, DrawPhase, RidePhase, MainPhase, BattlePhase, End};

	public void ChangeState(){
		Debug.LogError ("Current state is : " + CurrentState);
		switch(CurrentState){
			case GameStates.Setup:{
					CurrentState = GameStates.ChoosingTurnOrder;
					Debug.LogError ("Determining turn order");
					if (isServer) {
						ShowConfirmButton ();
					}
					break;
				}
			case GameStates.ChoosingTurnOrder:  //players confirm coin flipp
				{
					Debug.LogError ("Red go first");
					CurrentPlayerTurn = Player.ColorOfTeam.RED;
					CurrentState = GameStates.TurnOrderSelected;					
					break;
				}
		case GameStates.TurnOrderSelected:
			{
				CurrentState = GameStates.StandUp;
				CardManager.instance.RedVanguard.GetComponent<Deck> ().syncDeck [0].card.GetComponent<Card> ().flip();
				CardManager.instance.BlueVanguard.GetComponent<Deck> ().syncDeck [0].card.GetComponent<Card> ().flip ();
				StartCoroutine(TransitionText("Vanguard Stand Up!", true));
				Debug.LogError ("Vanguards stand up!");
				break;
			}
		case GameStates.StandUp:
			{
				CurrentState = GameStates.StandPhase;
				StartCoroutine (TransitionText (CurrentPlayerTurn.ToString () + "'s StandPhase", true));
				CardManager.instance.CmdStandAllCards (CurrentPlayerTurn);
				ChangeStateOfPlayerTo (CurrentPlayerTurn, GameStates.DrawPhase);
				break;
			}
		case GameStates.StandPhase:			
			{
				CurrentState = GameStates.DrawPhase;
				StartCoroutine (TransitionText (CurrentPlayerTurn.ToString () + "'s Draw Phase!", true));				
				break;
			}
		case GameStates.DrawPhase:
			{
				CurrentState = GameStates.RidePhase;
				StartCoroutine (TransitionText (CurrentPlayerTurn.ToString () + "'s Ride Phase!", false));
				break;
			}
		case GameStates.RidePhase:
			{
				CurrentState = GameStates.MainPhase;
				StartCoroutine (TransitionText (CurrentPlayerTurn.ToString () + "'s Main Phase!", false));
				break;
			}
		case GameStates.MainPhase:
			{
				CurrentAttackingCharacter = null;
				CurrentDefendingCharacter = null;
				CurrentState = GameStates.BattlePhase;
				StartCoroutine (TransitionText (CurrentPlayerTurn.ToString () + "'s Battle Phase!", false));
				break;
			}
		case GameStates.BattlePhase:
			{
				CurrentState = GameStates.StandUp;
				StartCoroutine (TransitionText (CurrentPlayerTurn.ToString () + "'s End Phase!", true));
				NextPlayer ();
				break;
			}
		case GameStates.AttackAndBoost:
			{
				CurrentState = GameStates.CallGuardian;
				StartCoroutine (TransitionText ("Call Guardian!", false));
				break;
			}
		case GameStates.CallGuardian:
			{
				if (CurrentAttackingCharacter.GetComponent<Card> ().myZone.tag == "Vanguard") {
					CurrentState = GameStates.DriveCheck;
					StartCoroutine (TransitionText ("Drive Check!", true));
				} else {
					CurrentState = GameStates.DamageCompare;
					StartCoroutine (TransitionText ("Comparing Damage!", true));
				}
				break;
			}
		case GameStates.DriveCheck:
			{
				CardManager.instance.CmdDrawCardToTriggerZone (CurrentPlayerTurn);
				CurrentState = GameStates.DamageCompare;
				ChangeStateOfPlayerTo (CurrentPlayerTurn, GameStates.DriveCheck);
				StartCoroutine (TransitionText ("revealing card!", false));
				if (isServer) {
					ShowConfirmButton ();
				}
				break;
			}
		case GameStates.DamageCompare:
			{
				if (CurrentDefendingCharacter.GetComponent<Card> ().myZone.tag == "Vanguard") {
					CurrentState = GameStates.DamageCheck;
					StartCoroutine (TransitionText ("Damage Check!", true));
				} else {
					StartCoroutine (TransitionText ("Resolving Damage!", true));
					CurrentState = GameStates.DamageResolve;
				}					
				break;
			}
		case GameStates.DamageCheck:
			{
				CardManager.instance.CmdDrawCardToTriggerZone (OtherPlayerTeam());
				CurrentState = GameStates.DamageResolve;
				ChangeStateOfPlayerTo (OtherPlayerTeam (), GameStates.DamageCheck);
				StartCoroutine (TransitionText ("Damage Check!", false));
				if (isServer) {
					ShowConfirmButton ();
				}
				break;
			}
		case GameStates.DamageResolve:
			{
				ChangeStateOfPlayerTo (CurrentPlayerTurn, GameStates.BattlePhase);
				CurrentState = GameStates.MainPhase;
				StartCoroutine (TransitionText ("Damage Resolves!", true));
				break;
			}
			default:
				break;
		}			
	}
	#endregion

	public void ChangeStateOfPlayerTo (Player.ColorOfTeam teamColor, GameStates newState){
		if (teamColor == Player.ColorOfTeam.RED) {
			RedState = newState;
		} else
			BlueState = newState;
	}

	private IEnumerator TransitionText(string globalMessage, bool transition){
		Debug.LogError ("Transitioning");
		ShowTTMessage (globalMessage);
		yield return new WaitForSeconds (2.0f);
		if (transition) {
			Debug.LogError ("transition text with change state");
			ChangeGameState ();
		}
		DisableAllTTMessaging ();
	}

	private void NextPlayer(){
		if (CurrentPlayerTurn == Player.ColorOfTeam.RED) {
			CurrentPlayerTurn = Player.ColorOfTeam.BLUE;
		} else
			CurrentPlayerTurn = Player.ColorOfTeam.RED;
	}

	private void ShowConfirmButton(){
		TTConfirmButton.SetActive (true);
	}

	private void ShowTTMessage(string msg){
		GlobalMessage.gameObject.SetActive (true);
		GlobalMessage.text = msg;
	}

	private void DisableAllTTMessaging(){
		GlobalMessage.gameObject.SetActive (false);
	}

	public void CheckInitialVanguards(){
		if (RedVanguardSelected && BlueVanguardSelected) {
			ChangeGameState ();
			NetworkDiscovery.instance.StopBroadcast ();
			Debug.LogError ("begin next phase");
			//red goes first for now..
		}
	}

	public void CheckInitialHands(){
		if (RedInitialHandSelected && BlueInitialHandSelected) {
			ChangeGameState ();
			Debug.LogError ("begin next phase");
		}
	}
		
	[Command]
	public void CmdBeingAttacked(GameObject character, Player.ColorOfTeam team){
		CurrentState = GameStates.AttackAndBoost;
		CurrentDefendingCharacter = character;
		if (team == Player.ColorOfTeam.RED) {
			RedState = GameStates.CallGuardian;
			BlueState = GameStates.Standby;
		} else {
			BlueState = GameStates.CallGuardian;
			RedState = GameStates.Standby;
		}
		StartCoroutine (DelayChangeState ());
	}

	public IEnumerator DelayChangeState(){
		yield return new WaitForSeconds (0.1f);
		ChangeGameState ();
	}



	public Player.ColorOfTeam OtherPlayerTeam(){
		if (CurrentPlayerTurn == Player.ColorOfTeam.RED) {
			return Player.ColorOfTeam.BLUE;
		} else
			return Player.ColorOfTeam.RED;
	}

}
