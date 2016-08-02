using UnityEngine;
using System.Collections;

public class TalkFXScript : MonoBehaviour
{

	#region Variables
	bool inPool = true;
	float intensity;
	float delay;
	float remainingLifetime;
	
	const float InitialAlpha = 2f;
	const float LifetimeInSeconds = 0.5f;
	const float sizeRatio = 4f;

	public Sprite ignoreTalkSprite, attackTalkSprite, avoidTalkSprite, alertTalkSprite,
		talkTalkSprite, followTalkSprite, flirtTalkSprite, cancelDefaultTalkSprite;

	SpriteRenderer spriteRenderer;
	#endregion

	#region MonoBehavior
	void Awake ()
	{
		spriteRenderer = transform.parent.GetComponentInChildren<SpriteRenderer> ();
	}

	void Update ()
	{
		if (delay > 0) {
			delay -= Time.deltaTime;
		} else {
			remainingLifetime -= Time.deltaTime;
			if (remainingLifetime > 0) {
				float remainingLifetimeFraction = 
				(remainingLifetime / SpeedController.AdjustedGameSeconds (LifetimeInSeconds));
				spriteRenderer.color = new Color (1f, 1f, 1f, InitialAlpha * remainingLifetimeFraction * intensity);
				transform.parent.localScale = Vector3.one * (1 - remainingLifetimeFraction) * sizeRatio;
			} else {
				if (inPool) {
					FXPool.DestroyTalk (transform.parent.gameObject);
				} else {
					GameObject.Destroy (transform.parent.gameObject);
				}
			}
		}
	}
	#endregion

	#region Public methods
	public void SetAsNotInPool ()
	{
		inPool = false;
	}

	public void Initialize
		(Vector2 sourcePosition, Vector2 targetPosition, ResponseCriteria.Response type, 
		 float intensity, float delayInSeconds)
	{
		transform.parent.position = sourcePosition;
		transform.parent.rotation = Quaternion.FromToRotation (Vector2.right, targetPosition - sourcePosition);
		switch (type) {
		case ResponseCriteria.Response.Ignore:
			spriteRenderer.sprite = ignoreTalkSprite;
			break;
		case ResponseCriteria.Response.Attack:
			spriteRenderer.sprite = attackTalkSprite;
			break;
		case ResponseCriteria.Response.Avoid:
			spriteRenderer.sprite = avoidTalkSprite;
			break;
		case ResponseCriteria.Response.Alert:
			spriteRenderer.sprite = alertTalkSprite;
			break;
		case ResponseCriteria.Response.Talk:
			spriteRenderer.sprite = talkTalkSprite;
			break;
		case ResponseCriteria.Response.Follow:
			spriteRenderer.sprite = followTalkSprite;
			break;
		case ResponseCriteria.Response.Flirt:
			spriteRenderer.sprite = flirtTalkSprite;
			break;
		case ResponseCriteria.Response.Cancel:
			spriteRenderer.sprite = cancelDefaultTalkSprite;
			break;
		default:
			throw new UnityException ("Bad data");
		}
		this.intensity = Mathf.Pow (intensity, 7);
		this.delay = SpeedController.AdjustedGameSeconds (delayInSeconds);

		spriteRenderer.color = new Color (1f, 1f, 1f, InitialAlpha * intensity);
		transform.parent.localScale = Vector3.zero;

		remainingLifetime = SpeedController.AdjustedGameSeconds (LifetimeInSeconds);
	}

	public void Initialize
		(Vector2 sourcePosition, Vector2 targetPosition, DroneState type, float delayInSeconds)
	{
		transform.parent.position = sourcePosition;
		transform.parent.rotation = Quaternion.FromToRotation (Vector2.right, targetPosition - sourcePosition);
		switch (type) {
		case DroneState.Default:
			spriteRenderer.sprite = cancelDefaultTalkSprite;
			break;
		case DroneState.Attacking:
			spriteRenderer.sprite = attackTalkSprite;
			break;
		case DroneState.Avoiding:
			spriteRenderer.sprite = avoidTalkSprite;
			break;
		case DroneState.Following:
			spriteRenderer.sprite = followTalkSprite;
			break;
		case DroneState.Flirting:
			spriteRenderer.sprite = flirtTalkSprite;
			break;
		default:
			throw new UnityException (type + ": Bad data");
		}
		this.intensity = 1f;
		this.delay = SpeedController.AdjustedGameSeconds (delayInSeconds);
		
		spriteRenderer.color = new Color (1f, 1f, 1f, InitialAlpha);
		transform.parent.localScale = Vector3.zero;
		
		remainingLifetime = SpeedController.AdjustedGameSeconds (LifetimeInSeconds);
	}
	#endregion
}
