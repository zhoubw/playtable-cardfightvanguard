using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameToJoin {
	public string LocalIp;
	public int GameNumber;
	public List<SeatsAvailable.Seat> AvailableSeats = new List<SeatsAvailable.Seat>();

	public GameToJoin(string ip, int gameNum,List<SeatsAvailable.Seat> availableSeats){
		LocalIp = ip;
		GameNumber = gameNum;
		AvailableSeats = availableSeats;
	}
}
