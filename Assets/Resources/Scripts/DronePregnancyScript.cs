using UnityEngine;
using System.Collections;

public class DronePregnancyScript : MonoBehaviour
{

	#region Variables
	bool isPregnant;
	float pregnancyTimer;
	static float pregnancyDuration = -1f;

	DroneResponseCriteriaController RCController;
	DroneStateMachine stateMachine;
	DroneHealthScript healthScript;
	DroneSpriteController spriteController;

	DroneGenome fatherGenome;

	SpeedController speedController;
	#endregion

	#region Properties
	public bool IsPregnant {
		get {
			return this.isPregnant;
		}
	}
	#endregion

	#region MonoBehavior
	void Awake()
	{
		if (pregnancyDuration == -1f) {
			pregnancyDuration = PlayerPrefs.GetFloat("DronePregnancyDuration", 20f);
		}
		RCController = GetComponent<DroneResponseCriteriaController>();
		stateMachine = GetComponent<DroneStateMachine>();
		healthScript = GetComponent<DroneHealthScript>();
		spriteController = transform.parent.GetComponentInChildren<DroneSpriteController>();

		speedController = GameObject.Find("GUI").GetComponent<SpeedController>();
		
		GetComponentInParent<DroneInitializationController>().OnInitialized += Initialize;
	}

	void Update()
	{
		if (isPregnant) {
			if (pregnancyTimer > 0) {
				pregnancyTimer -= Time.deltaTime;
			}
			else {
				StatsLogger.IncrementBirths();
				GameObject childDrone = DronePool.SpawnDrone(transform.parent.position);
				if (childDrone != null) {
					DroneResponseCriteriaController childRCController = 
						childDrone.GetComponentInChildren<DroneResponseCriteriaController>();
					childRCController.UpdateGeneticInformation(RCController.Genome, fatherGenome);
					childRCController.JustBornResponse(gameObject);

					childDrone.transform.FindChild("Display").gameObject
						.SetActive(!speedController.DisplayIsDeactivated());

					childDrone.GetComponentInChildren<DroneHealthScript>()
						.SetStartingHealth(healthScript.Health, healthScript.HealthRegenerationTimer);
				}
				Initialize();
				spriteController.SetStateSprite(stateMachine.GetCurrentAction().State);
			}
		}
	}
	#endregion

	#region Public methods
	public void Impregnate(DroneGenome fatherGenome)
	{
		this.fatherGenome = fatherGenome;
		isPregnant = true;
		pregnancyTimer = SpeedController.AdjustedGameSeconds(pregnancyDuration);
		spriteController.SetStateSprite(stateMachine.GetCurrentAction().State);
	}
	#endregion

	#region Private methods
	void Initialize()
	{
		fatherGenome = null;
		isPregnant = false;
		pregnancyTimer = SpeedController.AdjustedGameSeconds(pregnancyDuration);
	}
	#endregion
}
