using UnityEngine;
using System.Collections;

public class DroneGenome
{
	#region Variables
	float[] genome;

	const int RCSize = (int)(ResponseCriteria.Response.NumCriteria) + 
		(int)(ResponseCriteria.Response.NumPersistentCriteria);

	const int VisionWeigherNumFactors = 
		(int)(ResponseCriteria.Response.NumCriteria) + 3;
	const int TriggerWeigherNumFactors = 
		(int)(ResponseCriteria.Response.NumCriteria) + 5 + (int)(DroneState.NumStates);
	const int TalkWeigherNumFactors = 
		(int)(ResponseCriteria.Response.NumCriteria) * 2 + 5 + (int)(DroneState.NumStates) * 2;

	const int VisionWeigherSize = 1 + VisionWeigherNumFactors * 4;
	const int TriggerWeigherSize = 1 + TriggerWeigherNumFactors * 4;
	const int TalkWeigherSize = 1 + TalkWeigherNumFactors * 4;

	const int VisionRCCSize = ((int)(ResponseCriteria.Response.NumCriteria) +
		(int)(ResponseCriteria.Response.NumPersistentCriteria)) * VisionWeigherSize;
	const int TriggerRCCSize = ((int)(ResponseCriteria.Response.NumCriteria) +
		(int)(ResponseCriteria.Response.NumPersistentCriteria)) * TriggerWeigherSize;
	const int TalkRCCSize = ((int)(ResponseCriteria.Response.NumCriteria) +
		(int)(ResponseCriteria.Response.NumPersistentCriteria)) * TalkWeigherSize;

	const int TotalGenes = 
		2 * RCSize + 2 * VisionRCCSize + 5 * TriggerRCCSize + TalkRCCSize + 9 * TriggerWeigherSize;

	static int[] sectionBoundaries;
	static float fractionMutations;
	static float mutationStrength;
	#endregion
	
	#region Properties
	float[] Genome {
		get {
			return this.genome;
		}
	}
	#endregion

	#region Disclaimer
	//Genes are floats between 0 and 1, exclusive

	//RC: 8 + 4 = 12 genes
	//Vision Weigher: 11 * 4 + 1 = 45 genes
	//Trigger Weigher: 18 * 4 + 1 = 73 genes
	//Talk Weigher: 31 * 4 + 1 = 125 genes
	//Vision RCC: (8 + 4) * 45 = 540 genes
	//Trigger RCC: (8 + 4) * 73 = 876 genes
	//Talk RCC: (8 + 4) * 125 genes = 1500 genes

	//A drone's genome:
	//0	: [0000, 0011]		Initial RC
	//1	: [0012, 0023]		JustBorn RC (Action on mother before RC is set to Initial RC)
	//2	: [0024, 0563]		Default RCC (Vision)
	//3	: [0564, 1103]		Following RCC (Vision)
	//4	: [1104, 1979]		ActionTimedOut RCC (Trigger)
	//5	: [1980, 2855]		GetsFollowed RCC (Trigger)
	//6	: [2856, 3731]		GetsAttacked RCC (Trigger)
	//7	: [3732, 4607]		GetsAlerted RCC (Trigger)
	//8	: [4608, 5483]		GetsFlirted RCC (Trigger)
	//9	: [5484, 6983]		TalkedTo RCC (Talk)
	//10: [6984, 7640]		SpokenResponseValuesWeighers (9 Trigger Weighers)

	//Total genes: 7641
	#endregion

	#region Public methods
	public DroneGenome()
	{
		genome = new float[TotalGenes];
		for (int i = 0; i < TotalGenes; i++) {
			genome[i] = GenerateRandomGene();
		}
	}

	public void Fertilize(DroneGenome parent1, DroneGenome parent2)
	{
		//Equal probability for 3 types of inherited genes (parent 1, parent 2, average of 2 parents)

		for (int i = 0; i < TotalGenes; i++) {
			if (RandomBool(1f / 3)) {
				genome[i] = parent1.Genome[i];
			}
			else if (RandomBool(1f / 2)) {
				genome[i] = parent2.Genome[i];
			}
			else {
				genome[i] = (parent1.Genome[i] + parent2.Genome[i]) / 2;
			}
		}

		Mutate();
	}

	public ResponseCriteria GetInitialRC()
	{
		return GetRCFromGenome(sectionBoundaries[0]);
	}

	public ResponseCriteria GetJustBornRC()
	{
		return GetRCFromGenome(sectionBoundaries[1]);
	}

	public VisionResponseCriteriaCalculator GetDefaultRCC()
	{
		return (VisionResponseCriteriaCalculator)GetVisionRCCFromGenome(sectionBoundaries[2]);
	}

	public VisionResponseCriteriaCalculator GetFollowingRCC()
	{
		return (VisionResponseCriteriaCalculator)GetVisionRCCFromGenome(sectionBoundaries[3]);
	}

	public TriggerResponseCriteriaCalculator GetActionTimeOutRCC()
	{
		return (TriggerResponseCriteriaCalculator)GetTriggerRCCFromGenome(sectionBoundaries[4]);
	}

	public TriggerResponseCriteriaCalculator GetGetsFollowedRCC()
	{
		return (TriggerResponseCriteriaCalculator)GetTriggerRCCFromGenome(sectionBoundaries[5]);
	}

	public TriggerResponseCriteriaCalculator GetGetsAttackedRCC()
	{
		return (TriggerResponseCriteriaCalculator)GetTriggerRCCFromGenome(sectionBoundaries[6]);
	}

	public TriggerResponseCriteriaCalculator GetGetsAlertedRCC()
	{
		return (TriggerResponseCriteriaCalculator)GetTriggerRCCFromGenome(sectionBoundaries[7]);
	}

	public TriggerResponseCriteriaCalculator GetGetsFlirtedRCC()
	{
		return (TriggerResponseCriteriaCalculator)GetTriggerRCCFromGenome(sectionBoundaries[8]);
	}

	public TalkResponseCriteriaCalculator GetTalkedToRCC()
	{
		return (TalkResponseCriteriaCalculator)GetTalkRCCFromGenome(sectionBoundaries[9]);
	}

	public TriggerWeigher[] GetSpokenInformationWeighers()
	{
		TriggerWeigher[] output = new TriggerWeigher[9];
		int location = sectionBoundaries[10];

		for (int i = 0; i < output.Length; i++) { 
			output[i] = 
				GetTriggerWeigherFromGenome(location);
			location += TriggerWeigherSize;
		}

		return output;
	}
	#endregion

	#region Private methods
	float GenerateRandomGene()
	{
		float random;
		do {
			random = Random.Range(0f, 1f);
		}
		while (random == 0f);
		return random;
	}

	bool RandomBool(float probability)
	{
		float random = Random.Range(0, probability);
		return (random <= probability);
	}

	void Mutate()
	{
		float probGradualMutation = 1 / (mutationStrength + 1);
		//Every mutation is either a gradual or a random mutation. The expected strength should be equal.

		int numMutations = (int)(fractionMutations * TotalGenes);
		while (numMutations > 0) {
			int gene = (int)(Random.Range(0, TotalGenes));
			
			if (RandomBool(probGradualMutation)) {
				float random = Random.Range(Mathf.Pow((1 - mutationStrength), 4), 1f);
				float geneTemp;
				if (RandomBool(1 / 2f)) {
					geneTemp = genome[gene] * random;
				}
				else {
					geneTemp = 1 - ((1 - genome[gene]) * random);
				}
				if (geneTemp != 0f && geneTemp != 1f) {
					genome[gene] = geneTemp;
				}
			}
			else {
				genome[gene] = GenerateRandomGene();
			}
			
			numMutations--;
		}
	}

	#region RC Retrival Methods
	ResponseCriteria GetRCFromGenome(int location)
	{
		float[] values = new float[(int)(ResponseCriteria.Response.NumCriteria)];
		float[] cDurations = new float[(int)(ResponseCriteria.Response.NumCriteria)];

		System.Array.Copy(genome, location, 
		                  values, 0, (int)(ResponseCriteria.Response.NumCriteria));

		System.Array.Copy(genome, location + (int)(ResponseCriteria.Response.NumCriteria), 
		                  cDurations, 0, (int)(ResponseCriteria.Response.NumCriteria));

		return new ResponseCriteria(values, cDurations);
	}
	#endregion

	#region Weigher retrival methods
	VisionWeigher GetVisionWeigherFromGenome(int location)
	{
		int numFactors = VisionWeigherNumFactors;

		float constant;
		float[] weights = new float[numFactors];
		float[] weightTypes = new float[numFactors];
		float[] sizeBiases = new float[numFactors];
		float[] sizeBiasTypes = new float[numFactors];

		constant = genome[location];
		PrepareWeigherArrays(weights, weightTypes, sizeBiases, sizeBiasTypes, location, numFactors);

		return new VisionWeigher(constant, weights, weightTypes, sizeBiases, sizeBiasTypes);
	}

	TriggerWeigher GetTriggerWeigherFromGenome(int location)
	{
		int numFactors = TriggerWeigherNumFactors;
		
		float constant;
		float[] weights = new float[numFactors];
		float[] weightTypes = new float[numFactors];
		float[] sizeBiases = new float[numFactors];
		float[] sizeBiasTypes = new float[numFactors];
		
		constant = genome[location];
		PrepareWeigherArrays(weights, weightTypes, sizeBiases, sizeBiasTypes, location, numFactors);
		
		return new TriggerWeigher(constant, weights, weightTypes, sizeBiases, sizeBiasTypes);
	}

	TalkWeigher GetTalkWeigherFromGenome(int location)
	{
		int numFactors = TalkWeigherNumFactors;
		
		float constant;
		float[] weights = new float[numFactors];
		float[] weightTypes = new float[numFactors];
		float[] sizeBiases = new float[numFactors];
		float[] sizeBiasTypes = new float[numFactors];
		
		constant = genome[location];
		PrepareWeigherArrays(weights, weightTypes, sizeBiases, sizeBiasTypes, location, numFactors);
		
		return new TalkWeigher(constant, weights, weightTypes, sizeBiases, sizeBiasTypes);
	}

	void PrepareWeigherArrays(float[] weights, float[] weightTypes, 
	                          float[] sizeBiases, float[] sizeBiasTypes, int location, int numFactors)
	{
		System.Array.Copy(genome, location + 1, weights, 0, numFactors);
		System.Array.Copy(genome, location + numFactors + 1, weightTypes, 0, numFactors);
		System.Array.Copy(genome, location + numFactors * 2 + 1, sizeBiases, 0, numFactors);
		System.Array.Copy(genome, location + numFactors * 3 + 1, sizeBiasTypes, 0, numFactors);
		for (int i = 0; i < numFactors; i++) {
			weights[i] = weights[i] / (1 - weights[i]);
		}
	}
	#endregion

	#region RCC retrieval methods
	VisionResponseCriteriaCalculator GetVisionRCCFromGenome(int location)
	{
		VisionWeigher[] valueWeighers = new VisionWeigher[(int)(ResponseCriteria.Response.NumCriteria)];
		VisionWeigher[] cDurationWeighers = new VisionWeigher[(int)(ResponseCriteria.Response.NumPersistentCriteria)];
		int weigherSize = VisionWeigherSize;

		for (int i = 0; i < (int)(ResponseCriteria.Response.NumCriteria); i++) {
			valueWeighers[i] = GetVisionWeigherFromGenome(location);
			location += weigherSize;
			if (i < (int)(ResponseCriteria.Response.NumPersistentCriteria)) {
				cDurationWeighers[i] = GetVisionWeigherFromGenome(location);
			}
			location += weigherSize;
		}
	
		return new VisionResponseCriteriaCalculator(valueWeighers, cDurationWeighers);
	}

	TriggerResponseCriteriaCalculator GetTriggerRCCFromGenome(int location)
	{
		TriggerWeigher[] valueWeighers = new TriggerWeigher[(int)(ResponseCriteria.Response.NumCriteria)];
		TriggerWeigher[] cDurationWeighers = new TriggerWeigher[(int)(ResponseCriteria.Response.NumPersistentCriteria)];
		int weigherSize = TriggerWeigherSize;
		
		for (int i = 0; i < (int)(ResponseCriteria.Response.NumCriteria); i++) {
			valueWeighers[i] = GetTriggerWeigherFromGenome(location);
			location += weigherSize;
			if (i < (int)(ResponseCriteria.Response.NumPersistentCriteria)) {
				cDurationWeighers[i] = GetTriggerWeigherFromGenome(location);
			}
			location += weigherSize;
		}
		
		return new TriggerResponseCriteriaCalculator(valueWeighers, cDurationWeighers);
	}

	TalkResponseCriteriaCalculator GetTalkRCCFromGenome(int location)
	{
		TalkWeigher[] valueWeighers = new TalkWeigher[(int)(ResponseCriteria.Response.NumCriteria)];
		TalkWeigher[] cDurationWeighers = new TalkWeigher[(int)(ResponseCriteria.Response.NumPersistentCriteria)];
		int weigherSize = TalkWeigherSize;
		
		for (int i = 0; i < (int)(ResponseCriteria.Response.NumCriteria); i++) {
			valueWeighers[i] = GetTalkWeigherFromGenome(location);
			location += weigherSize;
			if (i < (int)(ResponseCriteria.Response.NumPersistentCriteria)) {
				cDurationWeighers[i] = GetTalkWeigherFromGenome(location);
			}
			location += weigherSize;
		}
		
		return new TalkResponseCriteriaCalculator(valueWeighers, cDurationWeighers);
	}
	#endregion

	#endregion

	#region Public static methods
	public static void DisplaySizesAndBoundaries()
	{
		Debug.Log("RC Size: " + RCSize);
		Debug.Log("VisionWeigherSize: " + VisionWeigherSize);
		Debug.Log("TriggerWeigherSize: " + TriggerWeigherSize);
		Debug.Log("TalkWeigherSize: " + TalkWeigherSize);
		Debug.Log("VisionRCCSize: " + VisionRCCSize);
		Debug.Log("TriggerRCCSize: " + TriggerRCCSize);
		Debug.Log("TalkRCCSize: " + TalkRCCSize);
		for (int i = 0; i < sectionBoundaries.Length; i++) {
			Debug.Log("Section " + i + ": " + sectionBoundaries[i]);
		}
		Debug.Log("Total: " + TotalGenes);
	}
	#endregion

	#region Private static methods
	static DroneGenome()
	{
		//Generate the section boundaries
		sectionBoundaries = new int[11];
		int location = 0;
		int index = 0;
		location = AddToSectionBoundaries(index++, location, RCSize);
		location = AddToSectionBoundaries(index++, location, RCSize);
		location = AddToSectionBoundaries(index++, location, VisionRCCSize);
		location = AddToSectionBoundaries(index++, location, VisionRCCSize);
		location = AddToSectionBoundaries(index++, location, TriggerRCCSize);
		location = AddToSectionBoundaries(index++, location, TriggerRCCSize);
		location = AddToSectionBoundaries(index++, location, TriggerRCCSize);
		location = AddToSectionBoundaries(index++, location, TriggerRCCSize);
		location = AddToSectionBoundaries(index++, location, TriggerRCCSize);
		location = AddToSectionBoundaries(index++, location, TalkRCCSize);
		AddToSectionBoundaries(index, location, 0);
		//Load PlayerPrefs
		fractionMutations = PlayerPrefs.GetFloat("DroneMutationFraction", 0.02f);
		mutationStrength = PlayerPrefs.GetFloat("DroneMutationStrength", 0.1f);
	}

	static int AddToSectionBoundaries(int boundaryIndex, int geneLocation, int length)
	{
		sectionBoundaries[boundaryIndex] = geneLocation;
		return geneLocation + length;
	}
	#endregion
}
