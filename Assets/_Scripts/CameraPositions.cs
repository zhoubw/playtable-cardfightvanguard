using UnityEngine;
using System.Collections;

public class CameraPositions : MonoBehaviour {
	public static CameraPositions instance;

	public Transform RedHomePosition;
	public Transform BlueHomePosition;
	public Transform RedVanguardCameraPosition;
	public Transform RedRearguardCameraPosition;
	public Transform RedGuardianCircleCameraPosition;

	public Transform BlueVanguardCameraPosition;
	public Transform BlueRearguardCameraPosition;
	public Transform BlueGuardianCircleCameraPosition;

	void Awake(){
		if (instance == null) {
			instance = this;
		} 
		else {
			DontDestroyOnLoad (gameObject);
		}
	}



}
