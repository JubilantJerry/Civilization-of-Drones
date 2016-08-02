using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class GUIScript : MonoBehaviour
{
	#region Variables
	public Button statsButton, slowDownButton, startStopButton, speedUpButton;
	public Text frameEfficiencyText;
	public CanvasGroup canvasGroup;
	public Text startStopText;

	int frameEfficiency;
	float frameEfficiencyDisplayTimer;

	float GUIFadeOutTimer;
	float GUIFadeOutTimeSeconds = 10f;

	SpeedController speedController;
	#endregion

	#region MonoBehavior
	void Start()
	{
		statsButton.interactable = false;
		slowDownButton.interactable = false;
		speedUpButton.interactable = false;
		speedController = GetComponent<SpeedController>();
		frameEfficiency = 100;
		frameEfficiencyDisplayTimer = speedController.AdjustedRealSeconds(1f);
		GUIFadeOutTimer = speedController.AdjustedRealSeconds(GUIFadeOutTimeSeconds);
	}

	void Update()
	{
		frameEfficiencyText.text = frameEfficiency.ToString();

		frameEfficiencyDisplayTimer -= Time.deltaTime;
		if (frameEfficiencyDisplayTimer <= 0) {
			frameEfficiencyDisplayTimer = speedController.AdjustedRealSeconds(1f);
			frameEfficiency =
			          (int)(100 / 
				(speedController.UnadjustedRealSeconds(Time.smoothDeltaTime)
				* Application.targetFrameRate));
		}

		if (!speedController.DisplayIsDeactivated() && GUIFadeOutTimer >= 0 && speedController.Running) {
			GUIFadeOutTimer -= Time.deltaTime;
			canvasGroup.alpha = 0.15f + 0.60f * 
				(GUIFadeOutTimer / speedController.AdjustedRealSeconds(GUIFadeOutTimeSeconds));
		}
	}
	#endregion

	#region Public methods
	public void SlowDown()
	{
		speedController.SlowDown();
	}

	public void StartStop()
	{
		speedController.StartStop();
		if (speedController.Running) {
			startStopText.text = "Stop";
			statsButton.interactable = true;
			slowDownButton.interactable = true;
			speedUpButton.interactable = true;
		}
		else {
			startStopText.text = "Start";
			statsButton.interactable = false;
			slowDownButton.interactable = false;
			speedUpButton.interactable = false;
		}
		frameEfficiencyDisplayTimer = 0;
	}

	public void SpeedUp()
	{
		speedController.SpeedUp();
	}

	public void ResetFadeOutTimer()
	{

		canvasGroup.alpha = 0.75f;
		GUIFadeOutTimer = speedController.AdjustedRealSeconds(GUIFadeOutTimeSeconds);
	}
	#endregion
}
