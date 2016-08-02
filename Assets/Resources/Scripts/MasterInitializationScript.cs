using UnityEngine;
using System.Collections;

public class MasterInitializationScript : MonoBehaviour
{

	public GameObject GUIGameObject, StatsLoggerGameObject, DronePoolGameObject, FXPoolGameObject;

	void Start()
	{
		//Ensure initialization order is correct
		GUIGameObject.SetActive(true);
		DronePoolGameObject.SetActive(true);
		StatsLoggerGameObject.SetActive(true);
		FXPoolGameObject.SetActive(true);
	}
}
