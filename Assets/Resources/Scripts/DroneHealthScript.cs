using UnityEngine;
using System.Collections;

public class DroneHealthScript : MonoBehaviour
{
	#region Variables
	float health;
	static float maxHealth = -1f;
	//Health represents time needed to be killed

	float healthRegenerationTimer;
	static float healthRegenerationDelay = -1f;
	float deathPulseTimer;
	const float deathPulseInterval = 1f;
	bool shouldBeDead = false;

	DroneSpriteController spriteController;
	#endregion

	#region Properties
	public float Health {
		get {
			return this.health;
		}
	}

	public float HealthRegenerationTimer {
		get {
			return this.healthRegenerationTimer;
		}
	}
	#endregion

	#region MonoBehavior
	void Awake()
	{
		if (maxHealth == -1f) {
			maxHealth = PlayerPrefs.GetFloat("DroneMaxHealth", 5f);
			healthRegenerationDelay = PlayerPrefs.GetFloat("DroneHealthRegenerationDelay", 5f);
		}
		spriteController = transform.parent.GetComponentInChildren<DroneSpriteController>();
		GetComponentInParent<DroneInitializationController>().OnInitialized += Initialize;
	}

	void Update()
	{
		if (health < SpeedController.AdjustedGameSeconds(maxHealth)) {
			if (healthRegenerationTimer > 0) {
				healthRegenerationTimer -= Time.deltaTime;
			}
			else {
				health = Mathf.Min(health + Time.deltaTime, maxHealth);
				spriteController.SetHealthIndication(health / SpeedController.AdjustedGameSeconds(maxHealth));
				shouldBeDead = false;
			}
		}

		if (deathPulseTimer > 0) {
			deathPulseTimer -= Time.deltaTime;
		}
	}
	#endregion

	#region Public methods
	public void DeductHealth(float deductionAmount)
	{
		health -= deductionAmount;
		healthRegenerationTimer = SpeedController.AdjustedGameSeconds(healthRegenerationDelay);
		if (health <= 0f) {
			if (deathPulseTimer <= 0) {
				FXPool.SpawnPulse(transform.parent.position, "Death");
				deathPulseTimer = SpeedController.AdjustedGameSeconds(deathPulseInterval);
			}
			if (!shouldBeDead) {
				StatsLogger.IncrementDeaths();
				shouldBeDead = true;
			}
			DronePool.DestroyDrone(gameObject);
			health = 0f;
		}
		else {
			spriteController.SetHealthIndication(health / SpeedController.AdjustedGameSeconds(maxHealth));
		}
	}

	public void SetStartingHealth(float newHealth, float newHealthRegenerationTimer)
	{
		//Assumption: parameters are already in adjusted game seconds
		health = newHealth;
		healthRegenerationTimer = newHealthRegenerationTimer;
		spriteController.SetHealthIndication(health / SpeedController.AdjustedGameSeconds(maxHealth));
	}
	#endregion

	#region Private methods
	void Initialize()
	{
		health = SpeedController.AdjustedGameSeconds(maxHealth);
		healthRegenerationTimer = SpeedController.AdjustedGameSeconds(healthRegenerationDelay);
		deathPulseTimer = 0f;
	}
	#endregion
}
