using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DroneStateMachine : MonoBehaviour
{

	#region Variables
	Stack<Action> stack;

	DroneSpriteController spriteController;
	DroneResponseCriteriaController RCController;
	DroneVisionScript visionScript;
	#endregion

	#region MonoBehavior
	void Awake()
	{
		stack = new Stack<Action>();
		spriteController = transform.parent.GetComponentInChildren<DroneSpriteController>();
		RCController = transform.GetComponent<DroneResponseCriteriaController>();
		visionScript = transform.GetComponent<DroneVisionScript>();
		GetComponentInParent<DroneInitializationController>().OnInitialized += Initialize;
	}

	void Update()
	{
		Action topAction = stack.Peek();
		if (topAction.Tick(Time.deltaTime)) {
			stack.Pop();
			StatsLogger.IncrementChangeAction();

			if (topAction.NewResponseAtEnd) {
				RCController.TriggerActionTimedOut(topAction.TargetDrone);
			}

			topAction = stack.Peek();
			if (topAction.State != DroneState.Default) {
				visionScript.ToggleDroneTracking(true);
			}
			else {
				visionScript.ToggleDroneTracking(false);
			}

			DroneState newState = topAction.State;
			spriteController.SetStateSprite(newState);
		}


	}
	#endregion

	#region Public methods
	public void AddToStateMachine
		(Action newAction)
	{
		//Assumption: newAction is not a default action
		stack.Push(newAction);
		spriteController.SetStateSprite(newAction.State);
		if (newAction.State != DroneState.Default) {
			visionScript.ToggleDroneTracking(true);
		}
		else {
			throw new UnityException(newAction.State + ": Assertion failure");
		}
	}

	public Action GetCurrentAction()
	{
		//Assumption: The bottom item is impossible to pop, so the stack is never empty
		return stack.Peek();
	}

	public void CancelCurrentState()
	{
		if (stack.Count > 1) {
			stack.Peek().Cancel();
		}
	}
	#endregion

	#region Private methods
	void Initialize()
	{
		stack.Clear();
		stack.Push(new Action(null, DroneState.Default, 1));
	}
	#endregion
}
#region Action class definition
public class Action
{
	#region Variables
	GameObject targetDrone;//Should be the simulation gameObject
	DroneState state;
	float duration; //Between 1 and Infinity
	bool newResponseAtEnd = true; //Instantly decide new response on timeout with same target
	#endregion

	#region Properties
	public GameObject TargetDrone {
		get {
			//Should be the Simulation game component
			return this.targetDrone;
		}
	}

	public DroneState State {
		get {
			return this.state;
		}
	}

	public float Duration {
		get {
			return this.duration;
		}
	}

	public bool NewResponseAtEnd {
		get {
			return this.newResponseAtEnd;
		}
	}
	#endregion

	#region Public methods
	public Action(GameObject targetDrone, DroneState stackedState, float compressedDuration)
	{
		this.targetDrone = targetDrone;
		this.state = stackedState;
		this.duration = SpeedController.AdjustedGameSeconds
				(1f / (1f - compressedDuration));
	}
		
		
	public bool Tick(float deltaTime)
	{
		duration -= deltaTime;
		return (duration <= 0);
	}

	public void Cancel()
	{
		//On a cancel action or when goal is accomplished
		newResponseAtEnd = false;
		duration = 0;
	}

	public override string ToString()
	{
		return string.Format("[Action: StackedState={0}, Duration={1}, NewResponseAtEnd = {3}]", 
			                     State, Duration);
	}
	#endregion
}
#endregion
