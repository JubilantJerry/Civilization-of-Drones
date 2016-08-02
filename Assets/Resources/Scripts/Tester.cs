using UnityEngine;
using System.Collections;

public class Tester : MonoBehaviour
{

	#region MonoBehavior
	void Start()
	{
		Debug.Log("Response Data Structures");
		TestResponseDataStructures();
		Debug.Log("Genome");
		TestGenome();
		Debug.Log("Player Prefs");
		TestPlayerPrefs();
	}
	#endregion

	#region Private methods
	void TestResponseDataStructures()
	{
		float[] float4 = new float[4] {1f,1f,1f,1f};
		float[] float8 = new float[8] {1f,1f,1f,1f,1f,1f,1f,1f};
		float[] float11 = new float[11] {1f,1f,1f,1f,1f,1f,1f,1f,1f,1f,1f};
		ResponseCriteria RC = new ResponseCriteria(float8, float4);
		VisionWeigher W = new VisionWeigher(1f, float11, float11, float11, float11);
		ResponseCriteriaCalculator RCC = new VisionResponseCriteriaCalculator
			(new VisionWeigher[8] {W,W,W,W,W,W,W,W}, new VisionWeigher[4] {W,W,W,W});
		Debug.Log(RC);
		Debug.Log(W);
		Debug.Log(RCC);
	}

	void TestGenome()
	{
		DroneGenome.DisplaySizesAndBoundaries();
	}

	void TestPlayerPrefs()
	{
		Debug.Log("CameraOrthographicSize: " + PlayerPrefs.GetFloat("CameraOrthographicSize", 16f));
		Debug.Log("DroneMutationFraction: " + PlayerPrefs.GetFloat("DroneMutationFraction", 0.02f));
		Debug.Log("DroneMutationStrength: " + PlayerPrefs.GetFloat("DroneMutationStrength", 0.1f));
		Debug.Log("DroneMaxHealth: " + PlayerPrefs.GetFloat("DroneMaxHealth", 5f));
		Debug.Log("DroneHealthRegenerationDelay: " + PlayerPrefs.GetFloat("DroneHealthRegenerationDelay", 5f));
		Debug.Log("DronePopulationMax: " + PlayerPrefs.GetInt("DronePopulationMax", 50));
		Debug.Log("DronePregnancyDuration: " + PlayerPrefs.GetFloat("DronePregnancyDuration", 20f));
		Debug.Log("DroneTrackRadius: " + PlayerPrefs.GetFloat("DroneTrackRadius", 12f));
		Debug.Log("DroneVisionRadius: " + PlayerPrefs.GetFloat("DroneVisionRadius", 6f));
		Debug.Log("DroneCloseRangeRadius: " + PlayerPrefs.GetFloat("DroneCloseRangeRadius", 3f));
	}
	#endregion
}
