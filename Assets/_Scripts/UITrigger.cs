using UnityEngine;
using System.Collections;

public class UITrigger : MonoBehaviour {
	
	public GameObject UnBoundedPanel;
	public GameObject RealPanel;

	public void SendToPanel(GameObject obj){
		obj.transform.SetParent (UnBoundedPanel.transform);
	}

	public void SendToRealPanel(GameObject obj){
		obj.transform.SetParent (RealPanel.transform);
	}
	// Use this for initialization
}
