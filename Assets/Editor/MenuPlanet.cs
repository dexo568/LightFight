using UnityEditor;
using UnityEngine;

class MenuPlanet : MonoBehaviour
{
	// Add menu named "Ethereal Planet" to the main menu
	[MenuItem("GameObject/Create Other/Ethereal Planet")]
	static void MenuEtherealPlanet()
	{
		GameObject obj = new GameObject();
		obj.name = "Ethereal Planet";
		obj.AddComponent<CPlanet>();
	}

	// Validate the menu item -- enabling or disabling it when asked by UnityEditor.
	[MenuItem("GameObject/Create Other/Ethereal Planet", true)]
	static bool ValidateMenuEtherealPlanet()
	{
		return true;
	}
}
