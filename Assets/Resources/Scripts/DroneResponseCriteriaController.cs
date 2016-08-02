using UnityEngine;
using System.Collections;
using System;
using System.Text;

public class DroneResponseCriteriaController : MonoBehaviour
{
	#region Variables
	ResponseCriteria currentResponseCriteria;
	DroneGenome genome;

	ResponseCriteria initialRC;
	ResponseCriteria justBornRC;
	VisionResponseCriteriaCalculator defaultRCC, followingRCC;
	TriggerResponseCriteriaCalculator actionTimedOutRCC, getsFollowedRCC, getsAttackedRCC, 
		getsAlertedRCC, getsFlirtedRCC;
	TalkResponseCriteriaCalculator talkedToRCC;
	TriggerWeigher[] SpokenInformationWeighers;

	bool justSpokeOrAlerted = false;

	DroneVisionScript visionScript;
	DroneStateMachine stateMachine;
	DroneHealthScript healthScript;
	DronePregnancyScript pregnancyScript;
	#endregion

	#region Properties
	public ResponseCriteria CurrentResponseCriteria {
		get {
			return this.currentResponseCriteria;
		}
	}

	public DroneGenome Genome {
		get {
			return this.genome;
		}
	}
	#endregion

	#region MonoBehaviors
	void Awake()
	{
		genome = new DroneGenome();
		stateMachine = GetComponent<DroneStateMachine>();
		visionScript = GetComponent<DroneVisionScript>();
		healthScript = GetComponent<DroneHealthScript>();
		pregnancyScript = GetComponent<DronePregnancyScript>();
		GetComponentInParent<DroneInitializationController>().OnInitialized += Initialize;
	}

	void Start()
	{
		initialRC = genome.GetInitialRC();
		justBornRC = genome.GetJustBornRC();
		defaultRCC = genome.GetDefaultRCC();
		followingRCC = genome.GetFollowingRCC();
		
		actionTimedOutRCC = genome.GetActionTimeOutRCC();
		getsFollowedRCC = genome.GetGetsFollowedRCC();
		getsAttackedRCC = genome.GetGetsAttackedRCC();
		getsAlertedRCC = genome.GetGetsAlertedRCC();
		getsFlirtedRCC = genome.GetGetsFlirtedRCC();
		
		talkedToRCC = genome.GetTalkedToRCC();
		SpokenInformationWeighers = genome.GetSpokenInformationWeighers();
		
		currentResponseCriteria = initialRC;
	}

	#endregion
	
	#region Public methods
	public void ToggleVisionResponses(bool enabled)
	{
		if (enabled) {
			if (visionScript.DroneSpottedEventNotSubscribed()) {
				visionScript.OnDroneSpotted += NewDroneSpottedResponse;
			}
		}
		else {
			visionScript.OnDroneSpotted -= NewDroneSpottedResponse;
		}
	}

	public void UpdateGeneticInformation(DroneGenome motherGenome, DroneGenome fatherGenome)
	{
		genome.Fertilize(motherGenome, fatherGenome);
		initialRC = genome.GetInitialRC();
		justBornRC = genome.GetJustBornRC();
		defaultRCC = genome.GetDefaultRCC();
		followingRCC = genome.GetFollowingRCC();
		
		actionTimedOutRCC = genome.GetActionTimeOutRCC();
		getsFollowedRCC = genome.GetGetsFollowedRCC();
		getsAttackedRCC = genome.GetGetsAttackedRCC();
		getsAlertedRCC = genome.GetGetsAlertedRCC();
		getsFlirtedRCC = genome.GetGetsFlirtedRCC();
		
		talkedToRCC = genome.GetTalkedToRCC();
		SpokenInformationWeighers = genome.GetSpokenInformationWeighers();
		
		currentResponseCriteria = initialRC;
	}

	public void TriggerFlirtSuccessful()
	{
		//Fired from another drone's TriggerGetsFlirted
		if (stateMachine.GetCurrentAction().State == DroneState.Flirting) {
			stateMachine.CancelCurrentState();
		}
		else {
			throw new UnityException(stateMachine.GetCurrentAction().State + ": Assertion Failure");
		}
	}

	public void JustBornResponse(GameObject mother)
	{
		//Fired from the mother's pregnancy script
		currentResponseCriteria = justBornRC;
		ResponseCriteria.Response response = CalculateResponseFromResponseCriteria();
		currentResponseCriteria = initialRC;
		MakeResponse(mother, response);
	}

	#region Trigger Responses
	public void TriggerActionTimedOut(GameObject oldTarget)
	{
		//Fired from the state machine
		int numAroundOldTarget = oldTarget.GetComponent<DroneVisionScript>().NumberDronesAroundMe;
		float oldTargetHealth = oldTarget.GetComponent<DroneHealthScript>().Health;
		bool oldTargetIsPregnant = oldTarget.GetComponent<DronePregnancyScript>().IsPregnant;
		float health = healthScript.Health;
		bool isPregnant = pregnancyScript.IsPregnant;
		DroneState currentState = stateMachine.GetCurrentAction().State;

		currentResponseCriteria = 
			actionTimedOutRCC.CalculateResponseCriteriaFromTrigger
				(currentResponseCriteria, numAroundOldTarget, oldTargetHealth, oldTargetIsPregnant, 
				health, isPregnant, currentState);

		ResponseCriteria.Response response = CalculateResponseFromResponseCriteria();
		MakeResponse(oldTarget, response);

		if (response == ResponseCriteria.Response.Attack || response == ResponseCriteria.Response.Avoid ||
			response == ResponseCriteria.Response.Follow || response == ResponseCriteria.Response.Flirt) {
			FXPool.SpawnPulse(transform.parent.position, "Timeout");
		}
	}

	public void TriggerGetsFollowed(GameObject triggerer)
	{
		//Fired from another drone's vision script
		int numAroundTriggerer = triggerer.GetComponent<DroneVisionScript>().NumberDronesAroundMe;
		float triggererHealth = triggerer.GetComponent<DroneHealthScript>().Health;
		bool triggererIsPregnant = triggerer.GetComponent<DronePregnancyScript>().IsPregnant;
		float health = healthScript.Health;
		bool isPregnant = pregnancyScript.IsPregnant;
		DroneState currentState = stateMachine.GetCurrentAction().State;
		
		currentResponseCriteria = 
			getsFollowedRCC.CalculateResponseCriteriaFromTrigger
				(currentResponseCriteria, numAroundTriggerer, triggererHealth, triggererIsPregnant, 
				 health, isPregnant, currentState);
		
		ResponseCriteria.Response response = CalculateResponseFromResponseCriteria();
		MakeResponse(triggerer, response);
	}

	public void TriggerGetsAttacked(GameObject triggerer)
	{
		//Fired from another drone's vision script
		int numAroundTriggerer = triggerer.GetComponent<DroneVisionScript>().NumberDronesAroundMe;
		float triggererHealth = triggerer.GetComponent<DroneHealthScript>().Health;
		bool triggererIsPregnant = triggerer.GetComponent<DronePregnancyScript>().IsPregnant;
		float health = healthScript.Health;
		bool isPregnant = pregnancyScript.IsPregnant;
		DroneState currentState = stateMachine.GetCurrentAction().State;
		
		currentResponseCriteria = 
			getsAttackedRCC.CalculateResponseCriteriaFromTrigger
				(currentResponseCriteria, numAroundTriggerer, triggererHealth, triggererIsPregnant, 
				 health, isPregnant, currentState);
		
		ResponseCriteria.Response response = CalculateResponseFromResponseCriteria();
		MakeResponse(triggerer, response);
	}

	public void TriggerGetsAlerted(GameObject droneToBeWaryOf)
	{
		//Set off by another drone's RC Controller
		int numAroundDroneToBeWaryOf = droneToBeWaryOf.GetComponent<DroneVisionScript>().NumberDronesAroundMe;
		float droneToBeWaryOfHealth = droneToBeWaryOf.GetComponent<DroneHealthScript>().Health;
		bool droneToBeWaryOfIsPregnant = droneToBeWaryOf.GetComponent<DronePregnancyScript>().IsPregnant;
		float health = healthScript.Health;
		bool isPregnant = pregnancyScript.IsPregnant;
		DroneState currentState = stateMachine.GetCurrentAction().State;
		
		currentResponseCriteria = 
			getsAlertedRCC.CalculateResponseCriteriaFromTrigger
				(currentResponseCriteria, numAroundDroneToBeWaryOf, droneToBeWaryOfHealth, droneToBeWaryOfIsPregnant, 
				 health, isPregnant, currentState);
		
		ResponseCriteria.Response response = CalculateResponseFromResponseCriteria();
		if (response != ResponseCriteria.Response.Alert) {
			MakeResponse(droneToBeWaryOf, response);
		}
	}

	public void TriggerGetsFlirted(GameObject triggerer)
	{
		//Fired from another drone's vision script
		//If response is to flirt, and if the drone is not pregnant,
		//	then get impregnated and call TriggerFlirtSuccessful on the other drone
		int numAroundTriggerer = triggerer.GetComponent<DroneVisionScript>().NumberDronesAroundMe;
		float triggererHealth = triggerer.GetComponent<DroneHealthScript>().Health;
		bool triggererIsPregnant = triggerer.GetComponent<DronePregnancyScript>().IsPregnant;
		float health = healthScript.Health;
		bool isPregnant = pregnancyScript.IsPregnant;
		DroneState currentState = stateMachine.GetCurrentAction().State;
		
		currentResponseCriteria = 
			getsFlirtedRCC.CalculateResponseCriteriaFromTrigger
				(currentResponseCriteria, numAroundTriggerer, triggererHealth, triggererIsPregnant, 
				 health, isPregnant, currentState);
		
		ResponseCriteria.Response response = CalculateResponseFromResponseCriteria();

		if (response != ResponseCriteria.Response.Flirt || triggererIsPregnant || isPregnant) {
			MakeResponse(triggerer, response);
		}
		else {
			DroneResponseCriteriaController triggererRCController = 
				triggerer.GetComponent<DroneResponseCriteriaController>();
			pregnancyScript.Impregnate(triggererRCController.Genome);

			FXPool.SpawnPulse(transform.parent.position, "Impregnated");

			triggererRCController.TriggerFlirtSuccessful();

			ResponseCriteria.Response simultaneousResponse = CalculateResponseFromResponseCriteria();
			MakeResponse(triggerer, simultaneousResponse);
		}
	}
	#endregion
	
	#region Talk Responses
	public void TriggerTalkedTo(GameObject talker, float[] spokenResponseValues, DroneState spokenState)
	{
		//Fired from another drone's RCController
		int numAroundTalker = talker.GetComponent<DroneVisionScript>().NumberDronesAroundMe;
		float talkerHealth = talker.GetComponent<DroneHealthScript>().Health;
		bool talkerIsPregnant = talker.GetComponent<DronePregnancyScript>().IsPregnant;
		float health = healthScript.Health;
		bool isPregnant = pregnancyScript.IsPregnant;
		DroneState currentState = stateMachine.GetCurrentAction().State;
		
		currentResponseCriteria = 
			talkedToRCC.CalculateResponseCriteriaFromTalk
				(currentResponseCriteria, spokenResponseValues, numAroundTalker, talkerHealth, talkerIsPregnant, 
				 health, isPregnant, currentState, spokenState);
		
		ResponseCriteria.Response response = CalculateResponseFromResponseCriteria();
		MakeResponse(talker, response);
	}
	#endregion
	#endregion

	#region Private methods
	void Initialize()
	{
		visionScript.ToggleDroneTracking(false);
	}

	#region Vision Responses
	void NewDroneSpottedResponse(GameObject target)
	{
		//Assumption: cannot be called when not in default or following mode
		//Spotted target should be the Simulation GameObject
		switch (stateMachine.GetCurrentAction().State) {
			case DroneState.Default:
				DefaultVisionResponse(target);
				break;
			case DroneState.Following:
				FollowingVisionResponse(target);
				break;
			default:
				throw new UnityException(stateMachine.GetCurrentAction().State + ": Assertion Failure");
		}
	}

	void DefaultVisionResponse(GameObject target)
	{
		int numAroundTarget = target.GetComponent<DroneVisionScript>().NumberDronesAroundMe;
		float targetHealth = target.GetComponent<DroneHealthScript>().Health;
		bool targetIsPregnant = target.GetComponent<DronePregnancyScript>().IsPregnant;
		currentResponseCriteria = 
			defaultRCC.CalculateResponseCriteriaFromVision
				(currentResponseCriteria, numAroundTarget, targetHealth, targetIsPregnant);
		ResponseCriteria.Response response = CalculateResponseFromResponseCriteria();
		MakeResponse(target, response);
	}

	void FollowingVisionResponse(GameObject target)
	{
		int numAroundTarget = target.GetComponent<DroneVisionScript>().NumberDronesAroundMe;
		float targetHealth = target.GetComponent<DroneHealthScript>().Health;
		bool targetIsPregnant = target.GetComponent<DronePregnancyScript>().IsPregnant;
		currentResponseCriteria = 
			followingRCC.CalculateResponseCriteriaFromVision
				(currentResponseCriteria, numAroundTarget, targetHealth, targetIsPregnant);
		ResponseCriteria.Response response = CalculateResponseFromResponseCriteria();
		MakeResponse(target, response);
	}
	#endregion

	ResponseCriteria.Response CalculateResponseFromResponseCriteria()
	{
		float[] values = currentResponseCriteria.ResponseValues;
		float total = 0f, random = 0f;
		for (int i = 0; i < (int)(ResponseCriteria.Response.NumCriteria); i++) {
			total += values[i];
		}

		random = UnityEngine.Random.Range(0, total);
		
		int responseNum = -1;
		while (random >= 0f) {
			responseNum += 1;
			random -= values[responseNum];
		}
		
		return (ResponseCriteria.Response)(responseNum); 
	}

	void MakeResponse(GameObject target, ResponseCriteria.Response response)
	{
		switch (response) {
			case ResponseCriteria.Response.Ignore:
				break;
			case ResponseCriteria.Response.Attack:
				stateMachine.AddToStateMachine(new Action(target, DroneState.Attacking,
			                                          currentResponseCriteria.ResponseCDurations[(int)
				                                           (ResponseCriteria.Response.AttackDurationIndex)]));
				break;
			case ResponseCriteria.Response.Avoid:
				stateMachine.AddToStateMachine(new Action(target, DroneState.Avoiding,
			                                          currentResponseCriteria.ResponseCDurations[(int)
				                                           (ResponseCriteria.Response.AvoidDurationIndex)]));
				break;
			case ResponseCriteria.Response.Alert:
				if (!justSpokeOrAlerted) {
					justSpokeOrAlerted = true;
					StatsLogger.IncrementAlert();

					foreach (Collider2D c in visionScript.MostRecentDronesInSight) {
						if (c.gameObject != target.gameObject && 
							c.gameObject != gameObject) {
							c.gameObject.GetComponent<DroneResponseCriteriaController>().
							TriggerGetsAlerted(target);
						}
					}

					ResponseCriteria.Response simultaneousResponse = CalculateResponseFromResponseCriteria();

					if (simultaneousResponse != ResponseCriteria.Response.Alert) {
						MakeResponse(target, simultaneousResponse);
					}

					FXPool.SpawnPulse(transform.parent.position, "Alert");
				}
				break;
			case ResponseCriteria.Response.Talk:
				if (!justSpokeOrAlerted) {
					justSpokeOrAlerted = true;
					StatsLogger.IncrementTalk();

					int numAroundListener = target.GetComponent<DroneVisionScript>().NumberDronesAroundMe;
					float targetHealth = target.GetComponent<DroneHealthScript>().Health;
					bool targetIsPregnant = target.GetComponent<DronePregnancyScript>().IsPregnant;
					float health = target.GetComponent<DroneHealthScript>().Health;
					bool isPregnant = target.GetComponent<DronePregnancyScript>().IsPregnant;
					float[] spokenValues = CalculateSpokenResponseValues
						(numAroundListener, 
						 targetHealth, targetIsPregnant, health, isPregnant);
					DroneState spokenState = CalculateSpokenState(numAroundListener, targetHealth, targetIsPregnant, 
					                                              health, isPregnant);
					target.GetComponent<DroneResponseCriteriaController>().
						TriggerTalkedTo(gameObject, spokenValues, spokenState);

					FXPool.SpawnTalk(transform.parent.position, target.transform.parent.position, 
					                 spokenState, spokenValues);
				}
				justSpokeOrAlerted = false;
				break;
			case ResponseCriteria.Response.Follow:
				stateMachine.AddToStateMachine(new Action(target, DroneState.Following,
			                                          currentResponseCriteria.ResponseCDurations[(int)
				                                           (ResponseCriteria.Response.FollowDurationIndex)]));
				break;
			case ResponseCriteria.Response.Flirt:
				stateMachine.AddToStateMachine(new Action(target, DroneState.Flirting,
			                                          currentResponseCriteria.ResponseCDurations[(int)
				                                           (ResponseCriteria.Response.FlirtDurationIndex)]));
				break;
			case ResponseCriteria.Response.Cancel:
				stateMachine.CancelCurrentState();

				FXPool.SpawnPulse(transform.parent.position, "Cancel");
				break;
			default:
				throw new UnityException(response + ": Bad data");
		}
	}

	float[] CalculateSpokenResponseValues(int numAroundListener, float listenerHealth, bool listenerIsPregnant, 
	                                      float health, bool isPregnant)
	{
		float[] output = new float[(int)(ResponseCriteria.Response.NumCriteria)];
		for (int i = 0; i < output.Length; i++) {
			float temp = SpokenInformationWeighers[i].CalculateResponseToTrigger
				(currentResponseCriteria, numAroundListener, listenerHealth, listenerIsPregnant, 
				 health, isPregnant, stateMachine.GetCurrentAction().State);
			//Lean the spoken response criterion value toward the true value
			//	by cubing the fractional deviation toward the nearest endpoint
			//This still makes every value in (0,1) possible
			//	e.g. 0.6 is leaned toward 0.2 to become 0.2 + 0.8 * (0.4 / 0.8)^3 = 0.3
			//	e.g. 0.95 is leaned toward 0.2 to become 0.2 + 0.8 * (0.75 / 0.8)^3 = 0.86
			//	e.g. 0.01 is leaned toward 0.2 to become 0.2 + 0.2 * (-0.19 / 0.2)^3 = 0.029
			float trueValue = currentResponseCriteria.ResponseValues[i];
			
			float deviation = temp - trueValue;
			float maxDeviation = (deviation > 0) ? (1 - trueValue) : (trueValue);
			
			output[i] = trueValue + Mathf.Pow(deviation, 3) / Mathf.Pow(maxDeviation, 2);
		}
		return output;
	}

	DroneState CalculateSpokenState(int numAroundListener, float listenerHealth, bool listenerIsPregnant, 
	                                float health, bool isPregnant)
	{
		DroneState currentState = stateMachine.GetCurrentAction().State;
		float raw = SpokenInformationWeighers[(int)(ResponseCriteria.Response.NumCriteria)].CalculateResponseToTrigger
			(currentResponseCriteria, numAroundListener, listenerHealth, listenerIsPregnant, 
			 health, isPregnant, stateMachine.GetCurrentAction().State);
		//Find the state from the raw data by imposing a range for each state
		//The range for the actual state is three times larger than the others
		int output = (int)(raw * ((int)(DroneState.NumStates) + 2));
		if (output > (int)(currentState)) {
			int deviation = output - (int)(currentState);
			if (deviation > 2) {
				output -= 2;
			}
			else {
				output = (int)(currentState);
			}
		}
		return (DroneState)output;
	}
	#endregion
}

#region ResponseCriteria class definition
public class ResponseCriteria
{
	#region Response enum definition
	public enum Response
	{
		Ignore = 0,
		Attack = 1,
		Avoid = 2,
		Alert = 3,
		Talk = 4,
		Follow = 5,
		Flirt = 6,
		Cancel = 7,
		NumCriteria = 8,
		AttackDurationIndex = 0,
		AvoidDurationIndex = 1,
		FollowDurationIndex = 2,
		FlirtDurationIndex = 3,
		NumPersistentCriteria = 4,
	}
	#endregion

	#region Variables
	float[] values, cDurations;
	#endregion

	#region Properties
	public float[] ResponseValues {
		get {
			return this.values;
		}
	}

	public float[] ResponseCDurations {
		get {
			return this.cDurations;
		}
	}
	#endregion

	#region Public methods		
	public ResponseCriteria(float[] values, float[] cDurations)
	{
		//Precondition: all response criteria are between 0 and 1, exclusive
		this.values = values;
		this.cDurations = cDurations;
	}
	

	public override string ToString()
	{
		string n = Environment.NewLine;
		string output = "";
		for (int i = 0; i < (int)(Response.NumCriteria); i++) {
			output = output + i + "| Value: " + values[i]; 
			if (i == 1 || i == 2) {
				output = output + "; cDuration: " + cDurations[i - 1];
			}
			else if (i == 5 || i == 6) {
				output = output + "; cDuration: " + cDurations[i - 3];
			}
			output = output + n;
		}
		return output;
	}
	#endregion
}
#endregion

#region Weigher class definitions

#region Weigher abstract superclass definition
public abstract class Weigher
{
	#region Variables
	protected float constant;
	protected float[] weights, sizeBiases;
	protected bool[] weightIsPositive, weightIsNormal, sizeBiasIsNormal;
	//Weights are each positive or negative, normal or conjugate. Between 0 and infinity
	//SizeBiases are only either normal or conjugate
	#endregion
	
	#region Properties
	float Constant {
		get {
			return this.constant;
		}
	}
	
	float[] Weights {
		get {
			return this.weights;
		}
	}
	
	float[] SizeBiases {
		get {
			return this.sizeBiases;
		}
	}
	
	bool[] WeightIsPositive {
		get {
			return this.weightIsPositive;
		}
	}
	
	bool[] WeightIsNormal {
		get {
			return this.weightIsNormal;
		}
	}
	
	bool[] SizeBiasIsNormal {
		get {
			return this.sizeBiasIsNormal;
		}
	}
	#endregion
	
	#region Public methods
	public Weigher(float constant, float[] weights, float[] weightTypes,
	               float[] sizeBiases, float[] sizeBiasTypes)
	{
		this.constant = constant;
		this.weights = weights;
		this.sizeBiases = sizeBiases;
		
		weightIsPositive = new bool[weightTypes.Length];
		weightIsNormal = new bool[weightTypes.Length];
		SplitIntoTwoBooleanArrays(weightTypes, weightIsPositive, weightIsNormal);
		
		sizeBiasIsNormal = new bool[sizeBiasTypes.Length];
		SplitIntoOneBooleanArray(sizeBiasTypes, sizeBiasIsNormal);
	}
	public override string ToString()
	{
		string n = Environment.NewLine;
		string constantString = "\tConstant: " + constant + "; ";
		string weightString = "\tWeight: ";
		string sizeBiasString = "\tSizeBias: ";
		for (int i = 0; i < weights.Length; i++) {
			weightString = weightString + 
				((weightIsPositive[i] ? "+" : "-") + weights[i] + (weightIsNormal[i] ? "; " : "c; "));
			sizeBiasString = sizeBiasString + 
				(sizeBiases[i] + (sizeBiasIsNormal[i] ? "; " : "c; "));
		}
		return (constantString + n + weightString + n + sizeBiasString);
	}
	#endregion
	
	#region Protected methods
	protected float CalculateFinalResponseValue(float[] factors)
	{
		//Find a weighted average based on a host of other factors and weights
		//Assumption: Every factor has a weight & sizeBias of a certain type
		
		float totalFactorSum = 0, totalWeights = 0;
		
		for (int i = 0; i < factors.Length; i++) {
			//Adjust the factor itself according to the type of weight
			float finalFactor = factors[i];
			if (!weightIsNormal[i]) {
				finalFactor = 1 - finalFactor;
			}
			if (!weightIsPositive[i]) {
				finalFactor = -finalFactor;
			}
			
			//Calculate the weight placed on the factor, depending on the type of sizeBias
			//Equation below is simplified. For example, in the first case,
			//final weight 	= bias' * weight + 2 * bias * weight * factor
			//				= weight * (bias' + 2 * bias * factor)
			//				= weight * (1 - bias + 2 * bias * factor)
			//				= weight * (1 + bias * (2 * factor - 1));
			float finalWeight;
			if (sizeBiasIsNormal[i]) {
				finalWeight = weights[i] * 
					(1 + sizeBiases[i] * (2 * factors[i] - 1));
			}
			else {
				finalWeight = weights[i] * 
					(1 + sizeBiases[i] * (1 - 2 * factors[i]));
			}
			
			//Finalize
			totalFactorSum += finalWeight * finalFactor;
			totalWeights += finalWeight;
		}
		
		//Get weighted average
		float wAvg = totalFactorSum / totalWeights;
		
		//Get final response value
		return constant + (1 - constant) * Mathf.Abs(wAvg);
	}
	#endregion
	
	#region Private methods
	void SplitIntoOneBooleanArray(float[] input, bool[] firstSet)
	{
		//Splits each floating point number into 2 boolean values. Assumption: number between 0 and 1
		//Boolean measures whether it is in the larger half of the whole range
		for (int i = 0; i < input.Length; i++) {
			firstSet[i] = (input[i] >= 0.5f);
		}
	}
	
	void SplitIntoTwoBooleanArrays(float[] input, bool[] firstSet, bool[] secondSet)
	{
		//Splits each floating point number into 2 boolean values. Assumption: number between 0 and 1
		//First boolean measures whether it is in the larger half of the whole range
		//Second boolean measures whether it is in the larger half of the half-range the number resides in
		for (int i = 0; i < input.Length; i++) {
			float temp = input[i];
			if (temp >= 0.5f) {
				temp -= 0.5f;
				firstSet[i] = true;
			}
			else {
				firstSet[i] = false;
			}
			
			secondSet[i] = (temp >= 0.25f);
		}
	}
	#endregion
}
#endregion

#region VisionWeigher subclass definition
public class VisionWeigher : Weigher
{
	#region Public methods
	public VisionWeigher(float constant, float[] weights, float[] weightTypes,
	                     float[] sizeBiases, float[] sizeBiasTypes) : 
		base(constant, weights, weightTypes, sizeBiases, sizeBiasTypes)
	{
	}
	
	public float CalculateResponseToVision
		(ResponseCriteria priorResponseCriteria, int numAroundTarget, float targetHealth, bool targetIsPregnant)
	{
		float[] factors = prepareVisionFactors
			(priorResponseCriteria.ResponseValues, numAroundTarget, targetHealth, targetIsPregnant);
		
		return CalculateFinalResponseValue(factors);
	}
	
	public float CalculateResponseCDurationToVision
		(ResponseCriteria priorResponseCriteria, int numAroundTarget, float targetHealth, bool targetIsPregnant)
	{
		float[] factors = prepareVisionFactors
			(priorResponseCriteria.ResponseCDurations, numAroundTarget, targetHealth, targetIsPregnant);
		
		return CalculateFinalResponseValue(factors);
	}
	#endregion
	
	#region Private methods
	float[] prepareVisionFactors(float[] criteriaFactors, int numAroundTarget, float targetHealth, bool targetIsPregnant)
	{
		float[] factors = new float[(int)(ResponseCriteria.Response.NumCriteria) + 3];
		
		Array.Copy(criteriaFactors, factors, (int)(ResponseCriteria.Response.NumCriteria));

		factors[(int)(ResponseCriteria.Response.NumCriteria)]
			= numAroundTarget * 1f / DronePool.MaxDroneNumber;
		factors[(int)(ResponseCriteria.Response.NumCriteria) + 1]
			= targetHealth;
		factors[(int)(ResponseCriteria.Response.NumCriteria) + 2]
			= (targetIsPregnant ? 1f : 0f);
		
		return factors;
	}
	#endregion
}
#endregion

#region TriggerWeigher subclass definition
public class TriggerWeigher : Weigher
{
	#region Public methods
	public TriggerWeigher(float constant, float[] weights, float[] weightTypes,
	                      float[] sizeBiases, float[] sizeBiasTypes) : 
		base(constant, weights, weightTypes, sizeBiases, sizeBiasTypes)
	{
	}
	
	public float CalculateResponseToTrigger
		(ResponseCriteria priorResponseCriteria, 
		 int numAroundTriggerer, float targetHealth, bool targetIsPregnant, float health, bool isPregnant, 
		 DroneState currentState)
	{
		float[] factors = prepareTriggerFactors
			(priorResponseCriteria.ResponseValues, numAroundTriggerer, targetHealth, targetIsPregnant, 
			 health, isPregnant, currentState);
		
		return CalculateFinalResponseValue(factors);
	}
	
	public float CalculateResponseCDurationToTrigger
		(ResponseCriteria priorResponseCriteria, 
		 int numAroundTriggerer, float targetHealth, bool targetIsPregnant, float health, bool isPregnant, 
		 DroneState currentState)
	{
		float[] factors = prepareTriggerFactors
			(priorResponseCriteria.ResponseCDurations, numAroundTriggerer, targetHealth, targetIsPregnant, 
			 health, isPregnant, currentState);
		
		return CalculateFinalResponseValue(factors);
	}
	#endregion
	
	#region Private methods
	float[] prepareTriggerFactors
		(float[] criteriaFactors, int numAroundTarget, float targetHealth, bool targetIsPregnant, float health, bool isPregnant, DroneState currentState)
	{
		float[] factors = new float
			[(int)(ResponseCriteria.Response.NumCriteria) + 5 + 
			(int)(DroneState.NumStates)];
		
		Array.Copy(criteriaFactors, factors, (int)(ResponseCriteria.Response.NumCriteria));

		factors[(int)(ResponseCriteria.Response.NumCriteria)]
			= numAroundTarget * 1f / DronePool.MaxDroneNumber;
		factors[(int)(ResponseCriteria.Response.NumCriteria) + 1]
			= targetHealth;
		factors[(int)(ResponseCriteria.Response.NumCriteria) + 2]
			= (targetIsPregnant ? 1f : 0f);

		factors[(int)(ResponseCriteria.Response.NumCriteria) + 3]
		= health;
		factors[(int)(ResponseCriteria.Response.NumCriteria) + 4]
		= (isPregnant ? 1f : 0f);

		factors[(int)(ResponseCriteria.Response.NumCriteria) + 5 + (int)(currentState)] = 1;
		return factors;
	}
	#endregion
}
#endregion

#region TalkWeigher subclass definition
public class TalkWeigher : Weigher
{
	#region Public methods
	public TalkWeigher(float constant, float[] weights, float[] weightTypes,
	                   float[] sizeBiases, float[] sizeBiasTypes) : 
		base(constant, weights, weightTypes, sizeBiases, sizeBiasTypes)
	{
	}
	
	public float CalculateResponseToTalk
		(ResponseCriteria priorResponseCriteria, float[] spokenResponseCriteriaValues, 
		 int numAroundTalker, float targetHealth, bool targetIsPregnant, float health, bool isPregnant, 
		 DroneState currentState, DroneState spokenState)
	{
		float[] factors = prepareTalkFactors
			(priorResponseCriteria.ResponseValues, spokenResponseCriteriaValues, 
			 numAroundTalker, targetHealth, targetIsPregnant, health, isPregnant, currentState, spokenState);
		
		return CalculateFinalResponseValue(factors);
	}
	
	public float CalculateResponseCDurationToTalk
		(ResponseCriteria priorResponseCriteria, float[] spokenResponseCriteriaValues, 
		 int numAroundTalker, float targetHealth, bool targetIsPregnant, float health, bool isPregnant, 
		 DroneState currentState, DroneState spokenState)
	{
		float[] factors = prepareTalkFactors
			(priorResponseCriteria.ResponseCDurations, spokenResponseCriteriaValues, 
			 numAroundTalker, targetHealth, targetIsPregnant, health, isPregnant, currentState, spokenState);
		
		return CalculateFinalResponseValue(factors);
	}
	#endregion
	
	#region Private methods
	float[] prepareTalkFactors
		(float[] criteriaFactors, float[] spokenResponseCriteriaValues, 
		 int numAroundTarget, float targetHealth, bool targetIsPregnant, float health, bool isPregnant, 
		 DroneState currentState, DroneState spokenState)
	{
		//Factor order: Prior response criteria, 
		//				Response criteria heard,
		//				Number around target (fraction of population),
		//				State Factors (binary 0 or 1), 
		//				State Factors heard
		//Assumption: A new float[] is initially populated with 0f's
		float[] factors = new float
			[((int)(ResponseCriteria.Response.NumCriteria)) * 2 + 5 +
			((int)(DroneState.NumStates)) * 2];
		
		Array.Copy(criteriaFactors, factors, (int)(ResponseCriteria.Response.NumCriteria));
		Array.Copy(spokenResponseCriteriaValues, 0, 
			 factors, (int)(ResponseCriteria.Response.NumCriteria), 
			 (int)(ResponseCriteria.Response.NumCriteria));

		factors[(int)(ResponseCriteria.Response.NumCriteria) * 2]
			= numAroundTarget * 1f / DronePool.MaxDroneNumber;
		factors[(int)(ResponseCriteria.Response.NumCriteria) * 2 + 1]
			= targetHealth;
		factors[(int)(ResponseCriteria.Response.NumCriteria) * 2 + 2]
			= (targetIsPregnant ? 1f : 0f);
		
		factors[(int)(ResponseCriteria.Response.NumCriteria) * 2 + 3]
			= health;
		factors[(int)(ResponseCriteria.Response.NumCriteria) * 2 + 4]
			= (isPregnant ? 1f : 0f);

		factors
			[(int)(ResponseCriteria.Response.NumCriteria) * 2 + 5 + (int)(currentState)] = 1;
		factors
			[(int)(ResponseCriteria.Response.NumCriteria) * 2 + 5 + 
			(int)(DroneState.NumStates) + (int)(spokenState)] = 1;
		
		return factors;
	}
	#endregion
}
#endregion
#endregion

#region ResponseCriteriaCalculator class definitions

#region ResponseCriteriaCalculator abstract superclass definition
public abstract class ResponseCriteriaCalculator
{
	#region Variables
	protected Weigher[] valueWeighers, cDurationWeighers;
	#endregion

	#region Public methods
	public ResponseCriteriaCalculator(Weigher[] valueWeighers, Weigher[] cDurationWeighers)
	{
		this.valueWeighers = valueWeighers;
		this.cDurationWeighers = cDurationWeighers;
	}

	public override string ToString()
	{
		string n = Environment.NewLine;
		string output = "";
		for (int i = 0; i < (int)(ResponseCriteria.Response.NumCriteria); i++) {
			output = output + i + "| Response:" + n + valueWeighers[i].ToString();
			if (i == 1 || i == 2) {
				output = output + n + i + "| cDuration:" + n + cDurationWeighers[i - 1].ToString();
			}
			else if (i == 5 || i == 6) {
				output = output + n + i + "| cDuration:" + n + cDurationWeighers[i - 3].ToString();
			}
			output = output + n + "------" + n;
		}
		return output;
	}
	#endregion
}
#endregion

#region VisionResponseCriteriaCalculator subclass definition
public class VisionResponseCriteriaCalculator : ResponseCriteriaCalculator
{
	#region Public methods
	public VisionResponseCriteriaCalculator(VisionWeigher[] valueWeighers, VisionWeigher[] cDurationWeighers) : 
	base(valueWeighers, cDurationWeighers)
	{
	}

	public ResponseCriteria CalculateResponseCriteriaFromVision
		(ResponseCriteria priorResponseCriteria, int numAroundTarget, float targetHealth, bool targetIsPregnant)
	{
		float[] newValues = new float[(int)(ResponseCriteria.Response.NumCriteria)];
		float[] newCDurations = new float[(int)(ResponseCriteria.Response.NumCriteria)];
		
		for (int i = 0; i < (int)(ResponseCriteria.Response.NumCriteria); i++) {
			newValues[i] = ((VisionWeigher)valueWeighers[i]).CalculateResponseToVision
				(priorResponseCriteria, numAroundTarget, targetHealth, targetIsPregnant);
			if (i < (int)(ResponseCriteria.Response.NumPersistentCriteria)) {
				newCDurations[i] = ((VisionWeigher)cDurationWeighers[i]).CalculateResponseCDurationToVision
				(priorResponseCriteria, numAroundTarget, targetHealth, targetIsPregnant);
			}
		}
		return new ResponseCriteria(newValues, newCDurations);
	}
	#endregion
}
#endregion

#region TriggerResponseCriteriaCalculator subclass definition
public class TriggerResponseCriteriaCalculator : ResponseCriteriaCalculator
{
	#region Public methods
	public TriggerResponseCriteriaCalculator(TriggerWeigher[] valueWeighers, TriggerWeigher[] cDurationWeighers) : 
	base(valueWeighers, cDurationWeighers)
	{
	}
	
	public ResponseCriteria CalculateResponseCriteriaFromTrigger
		(ResponseCriteria priorResponseCriteria, int numAroundTriggerer, float targetHealth, bool targetIsPregnant, 
		 float health, bool isPregnant, DroneState currentState)
	{
		float[] newValues = new float[(int)(ResponseCriteria.Response.NumCriteria)];
		float[] newCDurations = new float[(int)(ResponseCriteria.Response.NumCriteria)];
		
		for (int i = 0; i < (int)(ResponseCriteria.Response.NumCriteria); i++) {
			newValues[i] = ((TriggerWeigher)valueWeighers[i]).CalculateResponseToTrigger
				(priorResponseCriteria, numAroundTriggerer, targetHealth, targetIsPregnant, 
				 health, isPregnant, currentState);
			if (i < (int)(ResponseCriteria.Response.NumPersistentCriteria)) {
				newCDurations[i] = ((TriggerWeigher)cDurationWeighers[i]).CalculateResponseToTrigger
				(priorResponseCriteria, numAroundTriggerer, targetHealth, targetIsPregnant, 
				 health, isPregnant, currentState);
			}
		}
		return new ResponseCriteria(newValues, newCDurations);
	}
	#endregion
}
#endregion

#region TalkResponseCriteriaCalculator subclass definition
public class TalkResponseCriteriaCalculator : ResponseCriteriaCalculator
{
	#region Public methods
	public TalkResponseCriteriaCalculator(TalkWeigher[] valueWeighers, TalkWeigher[] cDurationWeighers) : 
	base(valueWeighers, cDurationWeighers)
	{
	}
	
	public ResponseCriteria CalculateResponseCriteriaFromTalk
		(ResponseCriteria priorResponseCriteria, float[] spokenResponseCriteriaValues, 
		 int numAroundTalker, float targetHealth, bool targetIsPregnant, float health, bool isPregnant, 
		 DroneState currentState, DroneState spokenState)
	{
		float[] newValues = new float[(int)(ResponseCriteria.Response.NumCriteria)];
		float[] newCDurations = new float[(int)(ResponseCriteria.Response.NumCriteria)];
		
		for (int i = 0; i < (int)(ResponseCriteria.Response.NumCriteria); i++) {
			newValues[i] = ((TalkWeigher)valueWeighers[i]).CalculateResponseToTalk
				(priorResponseCriteria, spokenResponseCriteriaValues,
				 numAroundTalker, targetHealth, targetIsPregnant, health, isPregnant, currentState, spokenState);
			if (i < (int)(ResponseCriteria.Response.NumPersistentCriteria)) {
				newCDurations[i] = ((TalkWeigher)cDurationWeighers[i]).CalculateResponseToTalk
				(priorResponseCriteria, spokenResponseCriteriaValues,
				 numAroundTalker, targetHealth, targetIsPregnant, health, isPregnant, currentState, spokenState);
			}
		}
		return new ResponseCriteria(newValues, newCDurations);
	}
	#endregion
}
#endregion
#endregion