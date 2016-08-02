using UnityEngine;
using System.Collections;

public class DroneMovementScript : MonoBehaviour
{
	#region Variables
	float speed;
	float maxSpeed;

	Quaternion direction;
	float angularSpeed;
	float maxAngularSpeed;

	static readonly Quaternion left = Quaternion.FromToRotation(Vector3.right, Vector3.up);
	static readonly Quaternion right = Quaternion.FromToRotation(Vector3.up, Vector3.right);

	float changeAngularVelocityTimer;
	const int changeAngularVelocityInterval = 2;

	Transform parentTransform;

	static float radius = -1;
	static Rect bounds;
	DroneStateMachine stateMachine;
	DronePregnancyScript pregnancyScript;
	#endregion

	#region Properties
	public float Speed {
		get {
			return this.speed;
		}
	}

	public Quaternion Direction {
		get {
			return this.direction;
		}
	}
	#endregion

	#region MonoBehavior
	void Awake()
	{
		maxSpeed = SpeedController.AdjustedGameRate(4f);
		maxAngularSpeed = SpeedController.AdjustedGameRate(128f);

		parentTransform = transform.parent;
		angularSpeed = Random.Range(-maxAngularSpeed, maxAngularSpeed);
		
		pregnancyScript = GetComponent<DronePregnancyScript>();
		stateMachine = GetComponent<DroneStateMachine>();
		GetComponentInParent<DroneInitializationController>().OnInitialized += Initialize;
	}

	void Start()
	{
		if (radius == -1) {
			radius = GetComponent<CircleCollider2D>().radius;
			OrthographicCameraBoundary cameraBounds = new OrthographicCameraBoundary(Camera.main);
			bounds = new Rect(cameraBounds.Left + radius, 
			                  cameraBounds.Bottom + radius, //Opposite coordinate orientation
			                  cameraBounds.Width - 2 * radius, 
			                  cameraBounds.Height - 2 * radius);
		}
	}

	void Update()
	{
		//State machine dependent
		Vector3 targetDroneDisplacement;
		Quaternion finalDirection;
		Action currentAction = stateMachine.GetCurrentAction();
		switch (currentAction.State) {
			case (DroneState.Default):
				finalDirection = direction * ((angularSpeed > 0) ? right : left);
				direction = Quaternion.RotateTowards(direction, finalDirection, Mathf.Abs(angularSpeed) * Time.deltaTime);
				ChangeAngularVelocityTimerTick();
				break;
			case (DroneState.Avoiding):
				//Check distance to target
				targetDroneDisplacement = 
					currentAction.TargetDrone.transform.parent.position - transform.parent.position;
				finalDirection = Quaternion.FromToRotation(Vector3.right, targetDroneDisplacement);
				direction = Quaternion.RotateTowards(
					direction, finalDirection, -Mathf.Abs(maxAngularSpeed) * Time.deltaTime);
				break;
			case (DroneState.Attacking):
			case (DroneState.Flirting):
			case (DroneState.Following):
				targetDroneDisplacement = 
					currentAction.TargetDrone.transform.parent.position - transform.parent.position;
				finalDirection = Quaternion.FromToRotation(Vector3.right, targetDroneDisplacement);
				direction = Quaternion.RotateTowards(
					direction, finalDirection, Mathf.Abs(maxAngularSpeed) * Time.deltaTime);
				break;
			default:
				throw new UnityException(currentAction.State + ": Bad data");
		}

		//Required updates
		Vector2 velocity = direction * Vector2.right * speed;
		if (pregnancyScript.IsPregnant) {
			velocity *= 0.5f;
		}

		if (parentTransform.position.x > bounds.xMax) {
			velocity.x = -Mathf.Abs(velocity.x);
			direction = Quaternion.FromToRotation(Vector3.right, velocity);
			angularSpeed = -angularSpeed;
		}
		else if (parentTransform.position.x < bounds.xMin) {
			velocity.x = Mathf.Abs(velocity.x);
			direction = Quaternion.FromToRotation(Vector3.right, velocity);
			angularSpeed = -angularSpeed;
		}
		if (parentTransform.position.y > bounds.yMax) {
			velocity.y = -Mathf.Abs(velocity.y);
			direction = Quaternion.FromToRotation(Vector3.right, velocity);
			angularSpeed = -angularSpeed;
		}
		else if (parentTransform.position.y < bounds.yMin) {
			velocity.y = Mathf.Abs(velocity.y);
			direction = Quaternion.FromToRotation(Vector3.right, velocity);
			angularSpeed = -angularSpeed;
		}

		parentTransform.Translate(velocity * Time.deltaTime);
		
	}
	#endregion

	#region Public methods
	public void SeparateFromOtherDrone(float otherDroneSpeed)
	{
		speed = Mathf.Max(otherDroneSpeed * 0.90f, maxSpeed * 0.60f);
	}

	public void CancelSeparateFromOtherDrone()
	{
		speed = maxSpeed;
	}
	#endregion

	#region Private methods
	void Initialize()
	{
		speed = maxSpeed;
		angularSpeed = Random.Range(-maxAngularSpeed, maxAngularSpeed);
		direction = Quaternion.AngleAxis(Random.Range(0, 360), Vector3.forward);
		changeAngularVelocityTimer = SpeedController.AdjustedGameSeconds(changeAngularVelocityInterval);
	}

	void ChangeAngularVelocityTimerTick()
	{
		changeAngularVelocityTimer -= Time.deltaTime;
		if (changeAngularVelocityTimer <= 0) {
			changeAngularVelocityTimer = SpeedController.AdjustedGameSeconds(changeAngularVelocityInterval);
			angularSpeed = SpeedController.AdjustedGameRate(Mathf.Pow(Random.Range(-2f, 2f), 7));
		}
	}
	#endregion
}
