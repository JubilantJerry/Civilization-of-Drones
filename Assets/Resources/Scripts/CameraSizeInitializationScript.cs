using UnityEngine;
using System.Collections;

public class CameraSizeInitializationScript : MonoBehaviour
{
	#region MonoBehavior
	void Awake()
	{
		GetComponent<Camera>().orthographicSize = PlayerPrefs.GetFloat("CameraOrthographicSize", 16f);
	}
	#endregion
}
