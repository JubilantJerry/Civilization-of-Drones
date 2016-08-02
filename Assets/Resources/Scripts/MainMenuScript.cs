using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainMenuScript : MonoBehaviour
{

	public Slider
		WorldSize, MaximumDronePopulation,
		DroneHealth, DroneHealthRegen,
		DronePregnancyDuration, DroneVisionRadius,
		DroneTrackRadiusMultiplier, DroneCloseRangeMultiplier,
		DroneMutationPercentage, DroneMutationStrength;

	public void StartGame()
	{
		PlayerPrefs.SetFloat("CameraOrthographicSize", WorldSize.value);
		PlayerPrefs.SetInt("DronePopulationMax", (int)(MaximumDronePopulation.value));
		PlayerPrefs.SetFloat("DroneMaxHealth", DroneHealth.value);
		PlayerPrefs.SetFloat("DroneHealthRegenerationDelay", DroneHealthRegen.value);
		PlayerPrefs.SetFloat("DronePregnancyDuration", DronePregnancyDuration.value);
		PlayerPrefs.SetFloat("DroneVisionRadius", DroneVisionRadius.value);
		PlayerPrefs.SetFloat("DroneTrackRadius", DroneVisionRadius.value * DroneTrackRadiusMultiplier.value);
		PlayerPrefs.SetFloat("DroneCloseRangeRadius", DroneVisionRadius.value * DroneCloseRangeMultiplier.value);
		PlayerPrefs.SetFloat("DroneMutationFraction", DroneMutationPercentage.value / 100);
		PlayerPrefs.SetFloat("DroneMutationStrength", DroneMutationStrength.value);
		Application.LoadLevel("Evolution Chamber");
	}
}
