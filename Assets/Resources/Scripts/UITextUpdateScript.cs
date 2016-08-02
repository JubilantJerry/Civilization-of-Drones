using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UITextUpdateScript : MonoBehaviour
{

	#region Variables
	Text dynamicText;
	#endregion

	#region MonoBehavior
	void Start()
	{
		dynamicText = GetComponent<Text>();
	}
	#endregion

	#region Public methods
	public void UpdateTextString(string value)
	{
		dynamicText.text = value;
	}
	public void UpdateTextFloatOneDP(float value)
	{
		dynamicText.text = string.Format("{0:#0.#}", value);
	}
	public void UpdateTextFloatTwoDP(float value)
	{
		dynamicText.text = string.Format("{0:0.##}", value);
	}
	#endregion
}
