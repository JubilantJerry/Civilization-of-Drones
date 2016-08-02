using UnityEngine;
using System.Collections;

public class SpeedController: MonoBehaviour
{
	#region Variables
	bool running;
	float simulationSpeed = 1;
	const float InitialTimeScale = 1f / 128;
	const float MinSpeed = 1f / 16;
	const float MaxSpeed = 256;
	const float MaxUnadjustedSpeed = 16;
	const int UpdateRate = 60;
	#endregion

	#region Properties
	public bool Running {
		get {
			return this.running;
		}
	}
	#endregion

	#region MonoBehavior
	void Start()
	{
		running = false;
		QualitySettings.vSyncCount = 0;
		Time.timeScale = InitialTimeScale;
		Application.targetFrameRate = UpdateRate;
	}
	#endregion

	#region Public methods
	public void SpeedUp()
	{
		if (simulationSpeed * 2 <= MaxSpeed) {
			simulationSpeed *= 2;
			Time.timeScale = simulationSpeed * InitialTimeScale;
			adjustDisplay();
		}
	}

	public void SlowDown()
	{
		if (simulationSpeed / 2 >= MinSpeed) {
			simulationSpeed /= 2;
			Time.timeScale = simulationSpeed * InitialTimeScale;
			adjustDisplay();
		}
	}

	public void StartStop()
	{
		running = !running;
		simulationSpeed = 1;
		Time.timeScale = InitialTimeScale;
		Application.targetFrameRate = UpdateRate;
		GameObject[] simulatedObjectContainers = GameObject.FindGameObjectsWithTag("Container");
		foreach (GameObject g in simulatedObjectContainers) {
			g.transform.FindChild("Simulation").gameObject.SetActive(running);
		}
		adjustDisplay();
	}
	
	public float AdjustedRealSeconds(float unadjustedRealSeconds)
	{
		return unadjustedRealSeconds * InitialTimeScale * simulationSpeed;
	}
	
	public float UnadjustedRealSeconds(float adjustedRealSeconds)
	{
		return adjustedRealSeconds / (InitialTimeScale * simulationSpeed);
	}

	public bool DisplayIsDeactivated()
	{
		return (running && simulationSpeed > MaxUnadjustedSpeed);
	}
	#endregion

	#region Private methods
	void adjustDisplay()
	{
		QualitySettings.vSyncCount = 0;
		GameObject[] displayedObjectContainers = GameObject.FindGameObjectsWithTag("Container");
		if (running && simulationSpeed > MaxUnadjustedSpeed) {
			foreach (GameObject g in displayedObjectContainers) {
				g.transform.FindChild("Display").gameObject.SetActive(false);
			}
			Application.targetFrameRate = UpdateRate * (int)(simulationSpeed / MaxUnadjustedSpeed);
		}
		else {
			foreach (GameObject g in displayedObjectContainers) {
				g.transform.FindChild("Display").gameObject.SetActive(true);
			}
			Application.targetFrameRate = UpdateRate;
		}
	}
	#endregion

	#region Public static methods
	public static float AdjustedGameSeconds(float unadjustedGameSeconds)
	{
		return unadjustedGameSeconds * InitialTimeScale;
	}

	public static float AdjustedGameRate(float unadjustedGameRate)
	{
		return unadjustedGameRate / InitialTimeScale;
	}

	public static float UnadjustedGameSeconds(float adjustedGameRate)
	{
		return adjustedGameRate / InitialTimeScale;
	}
	#endregion
}
