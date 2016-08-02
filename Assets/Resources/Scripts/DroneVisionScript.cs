using UnityEngine;
using System.Collections;
using System.Linq;

public class DroneVisionScript : MonoBehaviour
{
	#region Variables
	Collider2D[] mostRecentDronesInSight;
	GameObject target;
	bool targetDroneAlreadyInCloseRange;

	static float trackRadius = -1f;
	static float visionRadius = -1f;
	static float closeRangeRadius = -1f;

	float updateTimer;
	const float updateInterval = 1f / 2;
	float flirtDelayTimer;
	const float flirtDelay = 1f;
	float attackBeamDelayTimer;
	const float attackBeamDelay = 1f;


	DroneStateMachine stateMachine;
	DroneResponseCriteriaController RCController;
	DroneMovementScript movementScript;
	DronePregnancyScript pregnancyScript;
	#endregion

	#region Events
	public delegate void DroneSpottedHandler(GameObject newDrone);

	public event DroneSpottedHandler OnDroneSpotted;
	#endregion

	#region Properties
	public Collider2D[] MostRecentDronesInSight {
		get {
			return this.mostRecentDronesInSight;
		}
	}

	public int NumberDronesAroundMe {
		get {
			return this.mostRecentDronesInSight.Length;
		}
	}

	public static float VisionRadius {
		get {
			return visionRadius;
		}
	}

	public static float CloseRangeRadius {
		get {
			return closeRangeRadius;
		}
	}
	#endregion

	#region MonoBehavior
	void Awake()
	{
		if (trackRadius == -1f || visionRadius == -1f || closeRangeRadius == -1f) {
			trackRadius = PlayerPrefs.GetFloat("DroneTrackRadius", 12f);
			visionRadius = PlayerPrefs.GetFloat("DroneVisionRadius", 6f);
			closeRangeRadius = PlayerPrefs.GetFloat("DroneCloseRangeRadius", 3f);
		}
		stateMachine = GetComponent<DroneStateMachine>();
		RCController = GetComponent<DroneResponseCriteriaController>();
		movementScript = GetComponent<DroneMovementScript>();
		pregnancyScript = GetComponent<DronePregnancyScript>();

		GetComponentInParent<DroneInitializationController>().OnInitialized += Initialize;
	}

	void Update()
	{
		if (target != null) {
			DroneState currentState = stateMachine.GetCurrentAction().State;
			if (!target.activeInHierarchy ||
				DistanceToTargetDrone() > trackRadius) {
				stateMachine.CancelCurrentState();
			}
			else if (DistanceToTargetDrone() < closeRangeRadius) {
				if (!targetDroneAlreadyInCloseRange && DistanceToTargetDrone() < closeRangeRadius / 2f) {
					movementScript.SeparateFromOtherDrone
						(target.GetComponent<DroneMovementScript>().Speed);
				}
				switch (currentState) {
				//Assumption: Not Default
					case DroneState.Following:
						if (!targetDroneAlreadyInCloseRange) {
							targetDroneAlreadyInCloseRange = true;
							RCController.ToggleVisionResponses(true);
							target.GetComponent<DroneResponseCriteriaController>()
								.TriggerGetsFollowed(gameObject);

							FXPool.SpawnBeam(transform.parent.position, target.transform.parent.position,
							                 ResponseCriteria.Response.Follow);
						}
						break;
					case DroneState.Attacking:
						if (!targetDroneAlreadyInCloseRange) {
							targetDroneAlreadyInCloseRange = true;
							target.GetComponent<DroneResponseCriteriaController>()
								.TriggerGetsAttacked(gameObject);
						}

						if (attackBeamDelayTimer <= 0) {
							FXPool.SpawnBeam(transform.parent.position, target.transform.parent.position,
							                 ResponseCriteria.Response.Attack);
							attackBeamDelayTimer = SpeedController.AdjustedGameSeconds(attackBeamDelay);
						}

						float damage = Time.deltaTime;
						if (pregnancyScript.IsPregnant) {
							damage /= 2;
						}
						target.GetComponent<DroneHealthScript>().DeductHealth(damage);
						break;
					case DroneState.Avoiding:
						break;
					case DroneState.Flirting:
						if (flirtDelayTimer <= 0) {
							target.GetComponent<DroneResponseCriteriaController>()
								.TriggerGetsFlirted(gameObject);

							flirtDelayTimer = SpeedController.AdjustedGameSeconds(flirtDelay);

							FXPool.SpawnBeam(transform.parent.position, target.transform.parent.position,
							                 ResponseCriteria.Response.Flirt);
						}
						//If successful, other drone will extract genome information and notify this drone.
						//If it doesn't flirt back, continue periodically flirting as if nothing happened.
						break;
					default:
						throw new UnityException(currentState + ": Assertion Failure");
				}
			}
			else {
				if (targetDroneAlreadyInCloseRange) {
					targetDroneAlreadyInCloseRange = false;
					RCController.ToggleVisionResponses(false);
					movementScript.CancelSeparateFromOtherDrone();
				}
			}
		}

		if (flirtDelayTimer > 0) {
			flirtDelayTimer -= Time.deltaTime;
		}

		if (attackBeamDelayTimer > 0) {
			attackBeamDelayTimer -= Time.deltaTime;
		}
		
		updateTimer -= Time.deltaTime;
		if (updateTimer <= 0) {
			updateTimer = SpeedController.AdjustedGameSeconds(updateInterval);
			UpdateSet();
		}
	}

	void OnDestroy()
	{
		OnDroneSpotted = null;
	}
	#endregion

	#region Public Methods
	public bool DroneSpottedEventNotSubscribed()
	{
		return (OnDroneSpotted == null);
	}

	public void ToggleDroneTracking(bool enabled)
	{
		if (enabled) {
			GameObject newTarget = stateMachine.GetCurrentAction().TargetDrone;
			if (newTarget != target) {
				target = newTarget;
				targetDroneAlreadyInCloseRange = false;
				movementScript.CancelSeparateFromOtherDrone();
			}
		}
		else {
			target = null;
			targetDroneAlreadyInCloseRange = false;
			movementScript.CancelSeparateFromOtherDrone();
		}
		RCController.ToggleVisionResponses(!enabled);
	}
	#endregion

	#region Private Methods
	void Initialize()
	{
		mostRecentDronesInSight = new Collider2D[1];
		mostRecentDronesInSight[0] = GetComponent<Collider2D>();
		ToggleDroneTracking(false);
		flirtDelayTimer = 0f;
		attackBeamDelayTimer = 0f;
		updateTimer = SpeedController.AdjustedGameSeconds(updateInterval);
	}

	void UpdateSet()
	{
		Collider2D[] currentDronesInSight = Physics2D.OverlapCircleAll(transform.parent.position, visionRadius);
		if (OnDroneSpotted != null) {
			Collider2D newDroneCollider = GetNewDrone(mostRecentDronesInSight, currentDronesInSight);
			mostRecentDronesInSight = currentDronesInSight;
			if (newDroneCollider != null) {
				OnDroneSpotted(newDroneCollider.gameObject);
				return;
			}
		}
		mostRecentDronesInSight = currentDronesInSight;
	}

	Collider2D GetNewDrone(Collider2D[] past, Collider2D[] present)
	{
		foreach (Collider2D c in present.Except(past)) {
			return c;
		}
		return null;
	}

	float DistanceToTargetDrone()
	{
		Vector2 displacement = transform.parent.position - 
			target.transform.parent.position;
		return displacement.magnitude;
	}
	#endregion
}
