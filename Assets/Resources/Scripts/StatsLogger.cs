using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class StatsLogger : MonoBehaviour
{
	
	#region Variables
	static StatsLogger instance;
	SpeedController speedController;
	DronePool dronePool;


	int statNum = 0;
	float timeInSecs = 0;

	int numDeaths, numBirths, numTalk, numAlert, numChangeAction;
	#endregion
	
	#region MonoBehavior
	void Awake()
	{
		instance = this;
		speedController = GameObject.Find("GUI").GetComponent<SpeedController>();
		dronePool = GameObject.Find("DronePool").GetComponent<DronePool>();
	}

	void Update()
	{
		if (speedController.Running) {
			timeInSecs += SpeedController.UnadjustedGameSeconds(Time.deltaTime);
		}
	}
	#endregion

	#region Public methods
	public void PrintStats()
	{		

		#if UNITY_EDITOR
		Directory.CreateDirectory(@"Stats");
		string filePath = @"Stats/" + statNum.ToString() + ".txt";
		#elif UNITY_STANDALONE
			Directory.CreateDirectory(@"Stats");
			string filePath = @"Stats/" + statNum.ToString() + ".txt";
		#else
			Directory.CreateDirectory(Application.persistentDataPath + @"/Stats");
			string filePath = Application.persistentDataPath + @"/Stats/" + statNum.ToString() + ".txt";
		#endif
		string n = Environment.NewLine;
		FileStream stream = new FileStream(filePath, FileMode.Create);
		StreamWriter f = new StreamWriter(stream);
		f.AutoFlush = true;

		f.WriteLine("--Current Stats--");
		f.WriteLine("Time: " + timeInSecs);
		f.WriteLine("Population: " + dronePool.CurrentDroneNumber);
		f.WriteLine("Max Population: " + DronePool.MaxDroneNumber);
		f.WriteLine("Deaths: " + numDeaths);
		f.WriteLine("Births: " + numBirths);
		f.WriteLine("Talk Events: " + numTalk);
		f.WriteLine("Alert Events: " + numAlert);
		f.WriteLine("Action Change Events: " + numChangeAction);
		
		f.WriteLine(n + "--Genome Disclaimer--");
		f.WriteLine("Response Criteria Order:" + n + "\tIgnore, Attack (with duration), Avoid (with duration), " +
			"Alert, Talk, Follow (with duration), Flirt (with duration), Cancel");
		f.WriteLine("Vision Weigher Factor Order:" + n + "\tPrior Response Criteria (8 factors), " +
			"Number around target, Target health, Target IsPregnant");
		f.WriteLine("Trigger Weigher Factor Order:" + n + "\tPrior Response Criteria (8 factors), " +
			"Number around target, Target health, Target IsPregnant, Health, IsPregnant, Current State (5 factors)");
		f.WriteLine("Talk Weigher Factor Order:" + n + "\tPrior Response Criteria (8 factors), " +
			"Spoken Response Criteria (8 factors), Number around target, Target health, " +
			"Target IsPregnant, Health, IsPregnant, Current State (5 factors), Spoken State (5 factors)");
		f.WriteLine("Response Criteria Calculator Order:" + n + "\tIgnore, Attack (with duration), Avoid (with duration), " +
			"Alert, Talk, Follow (with duration), Flirt (with duration), Cancel");
		f.WriteLine("Spoken Response Values Order:" + n + "\tIgnore, Attack, Avoid, Alert, Talk, Follow, Flirt, " +
			"Cancel, Current State");
		f.WriteLine("cDuration:" + n + "\tValue compressed between 0 and 1. Uncompressed value is 1 / (1 - cDuration)");
		f.WriteLine("Weights and Size Biases:" + n + "\t+ / - means positive or negative, c means conjugate");
		
		f.WriteLine(n + "--Genome of a random drone--");
		DroneGenome genome = dronePool.RandomDrone.
			GetComponentInChildren<DroneResponseCriteriaController>().Genome;
		f.WriteLine("Initial RC" + n + genome.GetInitialRC().ToString());
		f.WriteLine("JustBornRC" + n + genome.GetJustBornRC().ToString());
		f.WriteLine("DefaultRCC (Vision)" + n + genome.GetDefaultRCC().ToString());
		f.WriteLine("FollowingRCC (Vision)" + n + genome.GetFollowingRCC().ToString());
		f.WriteLine("GetsAttackedRCC (Trigger)" + n + genome.GetGetsAttackedRCC().ToString());
		f.WriteLine("GetsAlertedRCC (Trigger)" + n + genome.GetGetsAlertedRCC().ToString());
		f.WriteLine("GetsFlirtedRCC (Trigger)" + n + genome.GetGetsFlirtedRCC().ToString());
		f.WriteLine("TalkedToRCC (Talk)" + n + genome.GetTalkedToRCC().ToString());
		f.WriteLine("SpokenResponseValueWeighers (9 Trigger Weighers)" + n);
		TriggerWeigher[] weighers = genome.GetSpokenInformationWeighers();
		for (int i = 0; i < weighers.Length; i++) {
			f.WriteLine(i + "| Spoken value:" + n + weighers[i].ToString());
			f.WriteLine("------");
		}
		statNum++;
	}
	#endregion

	#region Public static methods
	public static void IncrementDeaths()
	{
		instance.numDeaths++;
	}
	public static void IncrementBirths()
	{
		instance.numBirths++;
	}
	public static void IncrementTalk()
	{
		instance.numTalk++;
	}
	public static void IncrementAlert()
	{
		instance.numAlert++;
	}
	public static void IncrementChangeAction()
	{
		instance.numChangeAction++;
	}
	#endregion
}
