using UnityEngine;
// *******************************************
// Ability.cs
//
// Hardcoded abilities of cards
// *******************************************

public class Ability {

	// ALL ABILITIES EVER
	public static Effect[] abilities = {a0, a1, a2, a3, a4, a5, a6, a7, a8, a9, a10, a11, a12, a13, a14, a15, a16, a17, a18, a19, a20, a21, a22}; // 23 effects so far

	//	public Card host; // the card this ability belongs to and presumably acts from
	//public string type; // "AUTO", "ACT", "CONT"
	//public bool oncePerTurn;
	//public bool activated; // only relevant for those with oncePerTurn = true
	//public bool successful; // sometimes abilities are blocked

	public delegate void Effect(Card host, string flag); //flag = ACT, ATTACK, HIT, RIDEONTOP, RIDE, CALL, BOOST, INTERCEPT

	// public static Effect activate = new Effect(SampleEffect);
	// activate(this.host);

	//card.GetComponentInParent<Deck>().gameObject.layer

	public static void SampleEffect(Card host, string flag) {
		Debug.LogError ("effect activated with flag: " + flag);
	}

	public static void ability0(Card host, string flag) {

	}
	public static Effect a0 = new Effect(ability0);

	public static void ability1(Card host, string flag) {

	}
	public static Effect a1 = new Effect(ability1);

	public static void ability2(Card host, string flag) {

	}
	public static Effect a2 = new Effect(ability2);

	public static void ability3(Card host, string flag) {

	}
	public static Effect a3 = new Effect(ability3);

	public static void ability4(Card host, string flag) {

	}
	public static Effect a4 = new Effect(ability4);

	public static void ability5(Card host, string flag) {

	}
	public static Effect a5 = new Effect(ability5);

	public static void ability6(Card host, string flag) {

	}
	public static Effect a6 = new Effect(ability6);

	public static void ability7(Card host, string flag) {

	}
	public static Effect a7 = new Effect(ability7);

	public static void ability8(Card host, string flag) {

	}
	public static Effect a8 = new Effect(ability8);

	public static void ability9(Card host, string flag) {

	}
	public static Effect a9 = new Effect(ability9);

	public static void ability10(Card host, string flag) {

	}
	public static Effect a10 = new Effect(ability10);

	public static void ability11(Card host, string flag) {

	}
	public static Effect a11 = new Effect(ability11);

	public static void ability12(Card host, string flag) {
		
	}
	public static Effect a12 = new Effect(ability12);

	public static void ability13(Card host, string flag) {

	}
	public static Effect a13 = new Effect(ability13);

	public static void ability14(Card host, string flag) {

	}
	public static Effect a14 = new Effect(ability14);

	public static void ability15(Card host, string flag) {

	}
	public static Effect a15 = new Effect(ability15);

	public static void ability16(Card host, string flag) {

	}
	public static Effect a16 = new Effect(ability16);

	public static void ability17(Card host, string flag) {

	}
	public static Effect a17 = new Effect(ability17);

	public static void ability18(Card host, string flag) {

	}
	public static Effect a18 = new Effect(ability18);

	public static void ability19(Card host, string flag) {

	}
	public static Effect a19 = new Effect(ability19);

	public static void ability20(Card host, string flag) {

	}
	public static Effect a20 = new Effect(ability20);

	public static void ability21(Card host, string flag) {

	}
	public static Effect a21 = new Effect(ability21);

	public static void ability22(Card host, string flag) {

	}
	public static Effect a22 = new Effect(ability22);

}