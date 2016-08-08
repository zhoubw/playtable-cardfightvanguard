using UnityEngine;
using System.Collections;

public class GameNameImage : MonoBehaviour {
	public enum GameName{Daiearth, Lotus, Fianna, Lauris, StarVader};

	public Sprite GetImage(GameName PictureType){
		string resourcePath = "Image/GameRoom/" + PictureType.ToString();
		Sprite targetSprite = Resources.Load (resourcePath, typeof(Sprite))as Sprite;
		return targetSprite;
	}


}
