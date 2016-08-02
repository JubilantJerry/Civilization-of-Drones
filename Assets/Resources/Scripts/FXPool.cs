using UnityEngine;
using System.Collections;

public class FXPool : MonoBehaviour
{

	#region Variables
	public static FXPool instance;

	GameObject[] beamPool;
	GameObject[] pulsePool;
	GameObject[][] talkPool;

	public GameObject beam;
	public GameObject pulse;
	public GameObject talk;

	static ResponseCriteria.Response[] responseArray;

	static SpeedController speedController;
	#endregion

	#region MonoBehavior
	void Awake()
	{
		instance = this;
		speedController = GameObject.Find("GUI").GetComponent<SpeedController>();

		if (responseArray == null) {
			responseArray = new ResponseCriteria.Response[(int)(ResponseCriteria.Response.NumCriteria)]
				{ResponseCriteria.Response.Ignore, 
				ResponseCriteria.Response.Attack, 
				ResponseCriteria.Response.Avoid, 
				ResponseCriteria.Response.Alert, 
				ResponseCriteria.Response.Talk, 
				ResponseCriteria.Response.Follow, 
				ResponseCriteria.Response.Flirt, 
				ResponseCriteria.Response.Cancel};
		}
	}
	
	void Start()
	{
		int poolSize = DronePool.MaxDroneNumber;
		beamPool = new GameObject[poolSize];
		pulsePool = new GameObject[poolSize];
		talkPool = new GameObject[poolSize][];

		for (int i = 0; i < poolSize; i++) {
			beamPool[i] = ((GameObject)(Instantiate(beam)));
			beamPool[i].transform.SetParent(transform);

			pulsePool[i] = ((GameObject)(Instantiate(pulse)));
			pulsePool[i].transform.SetParent(transform);

			talkPool[i] = new GameObject[1 + (int)(ResponseCriteria.Response.NumCriteria)];
			for (int j = 0; j <= (int)(ResponseCriteria.Response.NumCriteria); j++) {
				talkPool[i][j] = ((GameObject)(Instantiate(talk)));
				talkPool[i][j].transform.SetParent(transform);
			}
		}
	}
	#endregion

	#region Public static methods
	public static void SpawnBeam
	(Vector2 sourcePosition, Vector2 targetPosition, ResponseCriteria.Response type)
	{
		foreach (GameObject beam in instance.beamPool) {
			if (!beam.activeSelf) {
				beam.transform.SetParent(instance.transform.parent);
				beam.SetActive(true);
				beam.GetComponentInChildren<BeamFXScript>().Initialize(sourcePosition, targetPosition, type);
				beam.transform.FindChild("Display").gameObject.SetActive(!speedController.DisplayIsDeactivated());
				return;
			}
		}
		GameObject newBeam = (Instantiate(instance.beam)).gameObject;
		newBeam.transform.SetParent(instance.transform.parent);
		newBeam.SetActive(true);
		newBeam.GetComponentInChildren<BeamFXScript>().Initialize(sourcePosition, targetPosition, type);
		newBeam.transform.FindChild("Display").gameObject.SetActive(!speedController.DisplayIsDeactivated());
		newBeam.GetComponentInChildren<BeamFXScript>().SetAsNotInPool();
	}

	public static void DestroyBeam(GameObject beam)
	{
		//Assumption: those that aren't in the pool killed themselves instead of calling this method
		beam.SetActive(false);
		beam.transform.SetParent(instance.transform);
	}

	public static void SpawnPulse
	(Vector2 sourcePosition, string type)
	{
		foreach (GameObject pulse in instance.pulsePool) {
			if (!pulse.activeSelf) {
				pulse.transform.SetParent(instance.transform.parent);
				pulse.SetActive(true);
				pulse.GetComponentInChildren<PulseFXScript>().Initialize(sourcePosition, type);
				pulse.transform.FindChild("Display").gameObject.SetActive(!speedController.DisplayIsDeactivated());
				return;
			}
		}

		//Pool completely exhausted
		GameObject newPulse = (Instantiate(instance.pulse)).gameObject;
		newPulse.transform.SetParent(instance.transform.parent);
		newPulse.SetActive(true);
		newPulse.GetComponentInChildren<PulseFXScript>().Initialize(sourcePosition, type);
		newPulse.transform.FindChild("Display").gameObject.SetActive(!speedController.DisplayIsDeactivated());
		newPulse.GetComponentInChildren<PulseFXScript>().SetAsNotInPool();
	}

	public static void DestroyPulse(GameObject pulse)
	{
		//Assumption: those that aren't in the pool killed themselves instead of calling this method
		pulse.SetActive(false);
		pulse.transform.SetParent(instance.transform);
	}


	public static void SpawnTalk
		(Vector2 sourcePosition, Vector2 targetPosition, DroneState spokenState, float[] spokenRCValues)
	{
		spokenRCValues = Normalize(spokenRCValues);

		int arrayLastIndex = (int)(ResponseCriteria.Response.NumCriteria);
		for (int i = 0; i < instance.talkPool.Length; i++) {
			if (!instance.talkPool[i][arrayLastIndex].activeSelf) {

				instance.talkPool[i][0].transform.SetParent(instance.transform.parent);
				instance.talkPool[i][0].SetActive(true);
				instance.talkPool[i][0].GetComponentInChildren<TalkFXScript>().Initialize
					(sourcePosition, targetPosition, spokenState, 0f);
				instance.talkPool[i][0].transform.FindChild("Display").
					gameObject.SetActive(!speedController.DisplayIsDeactivated());

				for (int j = 1; j <= arrayLastIndex; j++) {
					instance.talkPool[i][j].transform.SetParent(instance.transform.parent);
					instance.talkPool[i][j].SetActive(true);
					instance.talkPool[i][j].GetComponentInChildren<TalkFXScript>().Initialize
						(sourcePosition, targetPosition, 
						 responseArray[j - 1], spokenRCValues[j - 1], j / 16f);
					instance.talkPool[i][j].transform.FindChild("Display").
						gameObject.SetActive(!speedController.DisplayIsDeactivated());
				}

				return;
			}
		}

		//Pool completely exhausted
		GameObject newTalk = (Instantiate(instance.talk)).gameObject;
		newTalk.transform.SetParent(instance.transform.parent);
		newTalk.SetActive(true);
		newTalk.GetComponentInChildren<TalkFXScript>().Initialize
			(sourcePosition, targetPosition, spokenState, 0f);
		newTalk.transform.FindChild("Display").
			gameObject.SetActive(!speedController.DisplayIsDeactivated());
		newTalk.GetComponentInChildren<TalkFXScript>().SetAsNotInPool();
		
		for (int j = 1; j <= arrayLastIndex; j++) {
			newTalk = (Instantiate(instance.talk)).gameObject;
			newTalk.transform.SetParent(instance.transform.parent);
			newTalk.SetActive(true);
			newTalk.GetComponentInChildren<TalkFXScript>().Initialize(sourcePosition, targetPosition, 
				responseArray[j - 1], spokenRCValues[j - 1], j / 16f);
			newTalk.transform.FindChild("Display").
				gameObject.SetActive(!speedController.DisplayIsDeactivated());
			newTalk.GetComponentInChildren<TalkFXScript>().SetAsNotInPool();
		}
	}
	
	public static void DestroyTalk(GameObject talk)
	{
		//Assumption: those that aren't in the pool killed themselves instead of calling this method
		talk.SetActive(false);
		talk.transform.SetParent(instance.transform);
	}

	#endregion

	#region Private static methods
	static float[] Normalize(float[] RCValues)
	{
		float max = 0f;
		float[] output = new float[RCValues.Length];

		for (int i = 0; i < RCValues.Length; i++) {
			if (RCValues[i] > max) {
				max = RCValues[i];
			}
		}

		for (int i = 0; i < RCValues.Length; i++) {
			output[i] = RCValues[i] / max;
		}

		return output;
	}
	#endregion
}
