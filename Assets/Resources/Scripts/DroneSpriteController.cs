using UnityEngine;
using System.Collections;

public class DroneSpriteController : MonoBehaviour
{
	#region Variables
	public Sprite defaultSprite, followSprite, attackSprite, avoidSprite, flirtSprite,
		defaultPregnantSprite, followPregnantSprite, attackPregnantSprite, avoidPregnantSprite, flirtPregnantSprite;

	DronePregnancyScript pregnancyScript;
	SpriteRenderer spriteRenderer;
	#endregion

	#region MonoBehavior
	void Awake()
	{
		pregnancyScript = transform.parent.GetComponentInChildren<DronePregnancyScript>();
		spriteRenderer = GetComponent<SpriteRenderer>();
		GetComponentInParent<DroneInitializationController>().OnInitialized += Initialize;
	}
	#endregion

	#region Public methods
	public void SetStateSprite(DroneState state)
	{
		switch (state) {
			case DroneState.Default:
				if (pregnancyScript.IsPregnant) {
					spriteRenderer.sprite = defaultPregnantSprite;
				}
				else {
					spriteRenderer.sprite = defaultSprite;
				}
				break;
			case DroneState.Following:
				if (pregnancyScript.IsPregnant) {
					spriteRenderer.sprite = followPregnantSprite;
				}
				else {
					spriteRenderer.sprite = followSprite;
				}

				break;
			case DroneState.Attacking:
				if (pregnancyScript.IsPregnant) {
					spriteRenderer.sprite = attackPregnantSprite;
				}
				else {
					spriteRenderer.sprite = attackSprite;
				}
				break;
			case DroneState.Avoiding:
				if (pregnancyScript.IsPregnant) {
					spriteRenderer.sprite = avoidPregnantSprite;
				}
				else {
					spriteRenderer.sprite = avoidSprite;
				}
				break;
			case DroneState.Flirting:
				if (pregnancyScript.IsPregnant) {
					spriteRenderer.sprite = flirtPregnantSprite;
				}
				else {
					spriteRenderer.sprite = flirtSprite;
				}
				break;
			default:
				throw new UnityException(state + ": Bad data");
		}
	}

	public void SetHealthIndication(float compressedHealth)
	{
		spriteRenderer.color = new Color((0.25f * compressedHealth) + 0.75f,
		                                 (0.5f * compressedHealth) + 0.5f,
		                                 (0.5f * compressedHealth) + 0.5f);
	}
	#endregion

	#region Private methods
	void Initialize()
	{
		SetStateSprite(DroneState.Default);
		SetHealthIndication(1f);
	}
	#endregion
}
