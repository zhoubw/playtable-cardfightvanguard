using UnityEngine;
// *******************************************
// ContinuousEffect.cs
//
// Until end of battle, until end of turn, etc.
// These effects never expire on their own right now.
// *******************************************

public class ContinuousEffect {

	public Card host;

	public Card target;
	public int power;
	public int critical;

	public ContinuousEffect (Card host, Card target, int bonusPower, int bonusCritical) {
		this.host = host;
		this.target = target;
		this.power = bonusPower;
		this.critical = bonusCritical;
	}

	public void activate() {
		target.effectivePower += power;
		target.effectiveCritical += critical;
	}
}
