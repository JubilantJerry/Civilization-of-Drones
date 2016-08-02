using UnityEngine;
using System.Collections;

public class PulseFXScript : MonoBehaviour
{

	#region Variables
	bool inPool = true;
	float remainingLifetime;
	
	const float InitialAlpha = 1f;
	const float LifetimeInSeconds = 0.5f;
	const float sizeRatio = 2f;

	public Sprite alertPulseSprite, impregnatedPulseSprite, timeoutPulseSprite, 
		cancelPulseSprite, deathPulseSprite;

	SpriteRenderer spriteRenderer;
	#endregion

	#region MonoBehavior
	void Awake()
	{
		spriteRenderer = transform.parent.GetComponentInChildren<SpriteRenderer>();
	}

	void Update()
	{
		remainingLifetime -= Time.deltaTime;
		if (remainingLifetime <= 0) {
			if (inPool) {
				FXPool.DestroyPulse(transform.parent.gameObject);
			}
			else {
				GameObject.Destroy(transform.parent.gameObject);
			}
		}
		else {
			float remainingLifetimeFraction = 
				(remainingLifetime / SpeedController.AdjustedGameSeconds(LifetimeInSeconds));
			spriteRenderer.color = new Color(1f, 1f, 1f, InitialAlpha * remainingLifetimeFraction);
			transform.parent.localScale = Vector3.one * (1 - remainingLifetimeFraction) * sizeRatio;
		}
	}
	#endregion

	#region Public methods
	public void SetAsNotInPool()
	{
		inPool = false;
	}

	public void Initialize
		(Vector2 sourcePosition, string type)
	{
		transform.parent.position = sourcePosition;
		switch (type) {
			case "Alert":
				spriteRenderer.sprite = alertPulseSprite;
				break;
			case "Impregnated":
				spriteRenderer.sprite = impregnatedPulseSprite;
				break;
			case "Timeout":
				spriteRenderer.sprite = timeoutPulseSprite;
				break;
			case "Cancel":
				spriteRenderer.sprite = cancelPulseSprite;
				break;
			case "Death":
				spriteRenderer.sprite = deathPulseSprite;
				break;
			default:
				throw new UnityException("Assertion Failure");
		}
		spriteRenderer.color = new Color(1f, 1f, 1f, InitialAlpha);
		transform.parent.localScale = Vector3.zero;

		remainingLifetime = SpeedController.AdjustedGameSeconds(LifetimeInSeconds);
	}
	#endregion
}
