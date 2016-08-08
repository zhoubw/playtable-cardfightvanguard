using UnityEngine;
using System.Collections;
using System;

public class CardAttributes : MonoBehaviour {
	[Serializable]
	public class Properties{
		public string name;
		public string type;
		public  int[] abilities;
		public  string[] abilityTypes;
		public string clan;
		public string race;
		public string nation;
		public int grade;
		public string skill;
		public string trigger;
		public int power;
		public int critical;
		public int shield;
	}


}
