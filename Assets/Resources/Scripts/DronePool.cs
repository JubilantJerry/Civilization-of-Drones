using UnityEngine;
using System.Collections;

public class DronePool : MonoBehaviour
{
	#region Variables
	static DronePool instance;

	GameObject[] pool; //Filled with the root GameObjects of the drones

	static int maxDroneNumber = -1;
	int initialDroneNumber, currentDroneNumber;
	const int MinDroneNumber = 10;

	public GameObject drone;
	OrthographicCameraBoundary bounds;
	#endregion

	#region Properties
	public static int MaxDroneNumber {
		get {
			return maxDroneNumber;
		}
	}

	public int CurrentDroneNumber {
		get {
			return currentDroneNumber;
		}
	}

	public GameObject RandomDrone {
		get {
			int rand = Random.Range(0, instance.currentDroneNumber);
			return instance.pool[rand];
		}
	}
	#endregion

	#region MonoBehavior
	void Awake()
	{
		if (maxDroneNumber == -1f) {
			maxDroneNumber = PlayerPrefs.GetInt("DronePopulationMax", 50);
		}
		instance = this;
	}

	void Start()
	{
		bounds = new OrthographicCameraBoundary(Camera.main);
		float radius = drone.transform.Find("Simulation").GetComponent<CircleCollider2D>().radius;
		
		initialDroneNumber = (MinDroneNumber + maxDroneNumber) / 2;
		currentDroneNumber = 0;

		pool = new GameObject[maxDroneNumber];

		for (int i = 0; i < maxDroneNumber; i++) {
			pool[i] = ((GameObject)(Instantiate(drone, Vector3.zero, Quaternion.identity)));
			pool[i].transform.SetParent(transform);
		}

		for (int i = 0; i < initialDroneNumber; i++) {
			Vector3 randomLocation = new Vector3(
				Random.Range(bounds.Left + radius, bounds.Right - radius),
				Random.Range(bounds.Bottom + radius, bounds.Top - radius));
			SpawnDrone(randomLocation).transform.FindChild("Simulation").gameObject.SetActive(false);
		}
	}
	#endregion

	#region Public static methods
	public static void DestroyDrone(GameObject drone)
	{
		//Can be only be used on the simulation gameObject. Will not cause number of drones to decrease below minimum
		//Assumption: pool contains all drones onscreen, including this one
		if (instance.currentDroneNumber > MinDroneNumber) {
			Transform droneTransform = drone.transform.parent;
			droneTransform.gameObject.SetActive(false);
			droneTransform.SetParent(instance.transform);
			instance.currentDroneNumber -= 1;
		}
	}

	public static GameObject SpawnDrone(Vector3 position)
	{
		//Returns drone root GameObject. Will not spawn above the population maximum
		if (instance.currentDroneNumber >= maxDroneNumber) {
			return null; //Population limit reached
		}

		foreach (GameObject drone in instance.pool) {
			if (!drone.activeSelf) {
				drone.transform.SetParent(null);
				drone.SetActive(true);
				drone.transform.position = position;
				drone.GetComponent<DroneInitializationController>().InitializeDrone();
				instance.currentDroneNumber += 1;
				return drone;
			}
		}

		return null;
	}
	#endregion

	#region Private static methods
	static DronePool()
	{

	}
	#endregion
}
