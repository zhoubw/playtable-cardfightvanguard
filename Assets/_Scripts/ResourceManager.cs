using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;

public class ResourceManager : MonoBehaviour {	
	public enum AvailableDecks{NONE, Narukami, GoldPaladin};
	public static ResourceManager instance;

	List<string> RoomNames = new List<string>();
	List<Texture2D> RoomImages = new List<Texture2D>();

	public Texture2D BackTexture;

	[SerializeField]
	public Dictionary<string, Texture2D> AllCardsTextures = new Dictionary<string, Texture2D> ();

	private string StreamingAssetPath = "";
	private string DeckInfoPath="";
	private string CardPropertiesInfoPath = "";


	public JSONObject DeckCardListing;
	public JSONObject CardProperties;

	void Awake(){
		if (instance == null) {
			instance = this;
			GetAllRoomResources ();
			StartCoroutine(ReadJSONForDecks ());
			StartCoroutine (ReadJSONForCardProperties ());
		} else
			Destroy (this);
		DontDestroyOnLoad (this);
	}

	public void GetAllRoomResources(){
		string resourcePath = "RoomInfo";
		RoomImages = new List<Texture2D>(Resources.LoadAll (resourcePath, typeof(Texture2D)).Cast<Texture2D>().ToArray());
		for (int i = 0; i < RoomImages.Count; i++) {
			RoomNames.Add (RoomImages[i].name);
		}
	}

	public int AssignRandomRoomNumber(){
		return Random.Range (0, RoomNames.Count - 1);
	}


	private IEnumerator ReadJSONForDecks(){	
		StreamingAssetPath = Application.streamingAssetsPath;
		DeckInfoPath = StreamingAssetPath + "/" + "decks.json";

		if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
			StreamReader sr = new StreamReader (DeckInfoPath);
			yield return sr;
			DeckCardListing = new JSONObject (sr.ReadToEnd ());
			sr.Close ();
		}
		else if (Application.platform == RuntimePlatform.Android) {
			WWW www = new WWW (DeckInfoPath);
			yield return www;
			DeckCardListing = new JSONObject (www.text.ToString ());
		}
		LoadResources ();


	}

	private IEnumerator ReadJSONForCardProperties(){
		StreamingAssetPath = Application.streamingAssetsPath;
		CardPropertiesInfoPath = StreamingAssetPath + "/" + "cardProperties.json";

		if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
			StreamReader sr = new StreamReader (CardPropertiesInfoPath);
			yield return sr;
			CardProperties = new JSONObject (sr.ReadToEnd ());
			sr.Close ();
		}

		else if (Application.platform == RuntimePlatform.Android) {
			WWW www = new WWW (CardPropertiesInfoPath);
			yield return www;
			CardProperties = new JSONObject (www.text.ToString ());
		}


	}

	private void LoadResources(){	//gets the back and front images of all cards
		BackTexture = Resources.Load ("BackCards/back", typeof(Texture2D)) as Texture2D;
		for (int i = 0; i < DeckCardListing.Count; i++) {
			for (int j = 0; j < DeckCardListing [i].Count; j++) {
				List<Texture2D> newTexture = new List<Texture2D>(Resources.LoadAll ("FrontCards/" + DeckCardListing [i].keys [j], typeof(Texture2D)).Cast<Texture2D>().ToArray());
				AllCardsTextures.Add (DeckCardListing [i].keys [j], newTexture[0]);				
			}
		}
	}		



	 /*
	 string resourcePath = "MoveList/";
		if (myPokeData.selectedSingleAttack != "Struggle") {
			singleAttackPrefab = Resources.Load (resourcePath + myPokeData.selectedSingleAttack, typeof(GameObject)) as GameObject;

			singleAttackSounds = new List<AudioClip> (Resources.LoadAll (resourcePath + myPokeData.selectedSingleAttack, typeof(AudioClip)).Cast<AudioClip>().ToArray());
		} 
	  
	 */

}


//myPokeData = JsonUtility.FromJson<PokeData>(myPreData.Print());

