using UnityEngine;
using System.Collections;

public class BeamFXScript : MonoBehaviour
{

	#region Variables
	bool inPool = true;
	float remainingLifetime;
	
	const float InitialAlpha = 1f;
	const float LifetimeInSeconds = 0.5f;
	static float size = -1f;

	public Sprite attackBeamSprite, followBeamSprite, flirtBeamSprite;

	SpriteRenderer spriteRenderer;
	#endregion

	#region MonoBehavior
	void Awake()
	{
		spriteRenderer = transform.parent.GetComponentInChildren<SpriteRenderer>();
	}

	void Start()
	{
		if (size == -1f) {
			size = DroneVisionScript.CloseRangeRadius * 2;
		}
	}

	void Update()
	{
		remainingLifetime -= Time.deltaTime;
		if (remainingLifetime <= 0) {
			if (inPool) {
				FXPool.DestroyBeam(transform.parent.gameObject);
			}
			else {
				GameObject.Destroy(transform.parent.gameObject);
			}
		}
		else {
			spriteRenderer.color = new Color(1f, 1f, 1f, InitialAlpha * 
				(remainingLifetime / SpeedController.AdjustedGameSeconds(LifetimeInSeconds)));
		}
	}
	#endregion

	#region Public methods
	public void SetAsNotInPool()
	{
		inPool = false;
	}

	public void Initialize
		(Vector2 sourcePosition, Vector2 targetPosition, ResponseCriteria.Response type)
	{
		transform.parent.position = sourcePosition;
		transform.parent.rotation = Quaternion.FromToRotation(Vector2.up, targetPosition - sourcePosition);
		switch (type) {
			case ResponseCriteria.Response.Attack:
				spriteRenderer.sprite = attackBeamSprite;
				break;
			case ResponseCriteria.Response.Follow:
				spriteRenderer.sprite = followBeamSprite;
				break;
			case ResponseCriteria.Response.Flirt:
				spriteRenderer.sprite = flirtBeamSprite;
				break;
			default:
				throw new UnityException("Assertion Failure");
		}
		spriteRenderer.color = new Color(1f, 1f, 1f, InitialAlpha);
		transform.parent.localScale = Vector3.one * size;

		remainingLifetime = SpeedController.AdjustedGameSeconds(LifetimeInSeconds);
	}
	#endregion
}
