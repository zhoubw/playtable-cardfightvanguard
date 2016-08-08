using System;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class NetworkDiscovery : MonoBehaviour{
	public GameManager GMReference;
	public static NetworkDiscovery instance;

	void Awake(){
		if (instance == null) {
			instance = this;
		} else
			Destroy (this);
		DontDestroyOnLoad (this);
	}

	const int kMaxBroadcastMsgSize = 1024;
	// config data
	[SerializeField]
	public int m_BroadcastPort = 47777;

	[SerializeField]
	public int m_BroadcastKey = 1000;

	[SerializeField]
	public int m_BroadcastVersion = 1;

	[SerializeField]
	public int m_BroadcastSubVersion = 1;

	[SerializeField]
	public string m_BroadcastData = "HELLO";

	[SerializeField]
	public bool m_ShowGUI = true;

	[SerializeField]
	public int m_OffsetX;

	[SerializeField]
	public int m_OffsetY;

	// runtime data
	public int hostId = -1;
	public bool running = false;

	bool m_IsServer = false;
	bool m_IsClient = false;

	byte[] msgOutBuffer = null;
	byte[] msgInBuffer = null;
	HostTopology defaultTopology;

	public bool isServer { get { return m_IsServer; } set { m_IsServer = value; } }
	public bool isClient { get { return m_IsClient; } set { m_IsClient= value; } }

	static byte[] StringToBytes(string str)
	{
		byte[] bytes = new byte[str.Length * sizeof(char)];
		System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
		return bytes;
	}

	static string BytesToString(byte[] bytes)
	{
		char[] chars = new char[bytes.Length / sizeof(char)];
		System.Buffer.BlockCopy(bytes, 0, chars, 0, bytes.Length);
		return new string(chars);
	}

	public void Initialize()
	{
		if (m_BroadcastData.Length >= kMaxBroadcastMsgSize)
		{
			Debug.LogError("NetworkDiscovery Initialize - data too large. max is " + kMaxBroadcastMsgSize);
		}

		if (!NetworkTransport.IsStarted)
		{
			NetworkTransport.Init();
		}

		if (NetworkManager.singleton!= null)
		{
			m_BroadcastData = "NetworkManager:"+NetworkManager.singleton.networkAddress + ":" + NetworkManager.singleton.networkPort;
		}

		msgOutBuffer = StringToBytes(GMReference.GetRoomData());
		msgInBuffer = new byte[kMaxBroadcastMsgSize];

		ConnectionConfig cc = new ConnectionConfig();
		cc.AddChannel(QosType.Unreliable);
		defaultTopology = new HostTopology(cc, 1);

		if (m_IsServer)
			StartAsServer();

		if (m_IsClient)
			StartAsClient();

	}

	// listen for broadcasts
	public void StartAsClient()
	{
		if (hostId != -1 || running)
		{
			Debug.LogWarning("NetworkDiscovery StartAsClient already started");
		}

		hostId = NetworkTransport.AddHost(defaultTopology, m_BroadcastPort);
		if (hostId == -1)
		{
			Debug.LogError("NetworkDiscovery StartAsClient - addHost failed");
		}

		byte error;
		NetworkTransport.SetBroadcastCredentials(hostId, m_BroadcastKey, m_BroadcastVersion, m_BroadcastSubVersion, out error);

		running = true;
		m_IsClient = true;
		Debug.Log("StartAsClient Discovery listening");
	}

	// perform actual broadcasts
	public void StartAsServer()
	{
		if (hostId != -1 || running)
		{
			Debug.LogWarning("NetworkDiscovery StartAsServer already started");
		}

		hostId = NetworkTransport.AddHost(defaultTopology, 0);
		if (hostId == -1)
		{
			Debug.LogError("NetworkDiscovery StartAsServer - addHost failed");
		}

		byte err;
		if (!NetworkTransport.StartBroadcastDiscovery(hostId, m_BroadcastPort, m_BroadcastKey, m_BroadcastVersion, m_BroadcastSubVersion, msgOutBuffer, msgOutBuffer.Length, 1000, out err))
		{
			Debug.LogError("NetworkDiscovery StartBroadcast failed err: " + err);
		}

		running = true;
		m_IsServer = true;
		Debug.Log("StartAsServer Discovery broadcasting");
		DontDestroyOnLoad(gameObject);
	}

	public void StopBroadcast()
	{


		if (hostId == -1)
		{
			Debug.LogError("NetworkDiscovery StopBroadcast not initialized");
			return;
		}

		if (!running)
		{
			Debug.LogWarning("NetworkDiscovery StopBroadcast not started");
			return;
		}
		if (m_IsServer)
		{
			NetworkTransport.StopBroadcastDiscovery();
		}

		NetworkTransport.RemoveHost(hostId);
		hostId = -1;
		running = false;
		m_IsServer = false;
		m_IsClient = false;
		msgInBuffer = null;
		Debug.Log("Stopped Discovery broadcasting");
	}

	void Update()
	{
		if (hostId == -1)
			return;

		if (m_IsServer)
			return;

		int connectionId;
		int channelId;
		int receivedSize;
		byte error;
		NetworkEventType networkEvent = NetworkEventType.DataEvent;

		do
		{
			networkEvent = NetworkTransport.ReceiveFromHost(hostId, out connectionId, out channelId, msgInBuffer, kMaxBroadcastMsgSize, out receivedSize, out error);

			if (networkEvent == NetworkEventType.BroadcastEvent)
			{
				NetworkTransport.GetBroadcastConnectionMessage(hostId, msgInBuffer, kMaxBroadcastMsgSize, out receivedSize, out error);

				string senderAddr;
				int senderPort;
				NetworkTransport.GetBroadcastConnectionInfo(hostId, out senderAddr, out senderPort, out error);

				OnReceivedBroadcast(senderAddr, BytesToString(msgInBuffer));
			}
		} while (networkEvent != NetworkEventType.Nothing);

	}

	//every client devices will receive updated broadcasts of all hosts
	public virtual void OnReceivedBroadcast(string fromAddress, string data){

		Debug.Log ("Got broadcast from [" + fromAddress + "] " + data);
		string HostIp;
		var address = fromAddress.Split (':');
		HostIp = address [3];

		//if game is disconnected and is in our available list of games, remove it from that list
		if (data == "disconnected" && LoginManager.instance.AvailableGames.ContainsKey(HostIp)) {
			if (LoginManager.instance.AvailableGames.ContainsKey (HostIp)) {
				LoginManager.instance.RemoveGame (HostIp);
			}
		}

		//if game is not labeled disconnected and we don't have it on our available games list, add the game to the list
		else if (!LoginManager.instance.AvailableGames.ContainsKey (HostIp)) {
			List<SeatsAvailable.Seat> seatsAvailable = new List<SeatsAvailable.Seat> ();
			string[] availableSpot = data.Split (',');
			int roomNumber = Int32.Parse (availableSpot [0]);
			if (availableSpot.Length > 1) { //if there are reported available slots
				for (int i = 1; i < availableSpot.Length; i++) {
					if ((availableSpot [i].Contains("RED") || availableSpot[i].Contains("BLUE"))) {
						SeatsAvailable.Seat playSide = (SeatsAvailable.Seat)Enum.Parse (typeof(SeatsAvailable.Seat), availableSpot [i]);
						seatsAvailable.Add (playSide);
					}
				}
			}	
			GameToJoin newGame = new GameToJoin (HostIp, roomNumber, seatsAvailable);
			LoginManager.instance.AddGame(newGame);
		}

	}


	void OnGUI()
	{
		if (!m_ShowGUI)
			return;

		int xpos = 10 + m_OffsetX;
		int ypos = 40 + m_OffsetY;
		int spacing = 24;

		if (msgInBuffer == null) {
			if (GUI.Button (new Rect (xpos, ypos, 200, 20), "Initialize Broadcast")) {
				Initialize ();

				Debug.LogError ("passed initialized");
			}
			return;
		} else {
			string suffix = "";
			if (m_IsServer)
				suffix = " (server)";
			if (m_IsClient)
				suffix = " (client)";

			GUI.Label (new Rect (xpos, ypos, 200, 20), "initialized" + suffix);
		}
		ypos += spacing;
		if (running) {
			if (GUI.Button (new Rect (xpos, ypos, 200, 20), "Stop")) {

				StopBroadcast ();
			}
			ypos += spacing;
		} else {
			if (GUI.Button (new Rect (xpos, ypos, 200, 20), "Start Broadcasting")) {
				StartAsServer ();
			}
			ypos += spacing;

			if (GUI.Button (new Rect (xpos, ypos, 200, 20), "Listen for Broadcast")) {
				StartAsClient ();
			}
			ypos += spacing;
		}
	}

}
