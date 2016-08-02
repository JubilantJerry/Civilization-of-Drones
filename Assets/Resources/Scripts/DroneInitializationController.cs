using UnityEngine;
using System.Collections;

public class DroneInitializationController : MonoBehaviour
{

	#region Events
	public delegate void InitializationEventHandler();
	public event InitializationEventHandler OnInitialized;
	#endregion

	#region Public methods
	public void InitializeDrone()
	{
		//Will get called from DronePool.Spawn
		if (OnInitialized != null) {
			OnInitialized();
		}
	}
	#endregion
}
