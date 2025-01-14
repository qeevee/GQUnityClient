﻿using Code.GQClient.Err;
using UnityEngine;
using UnityEngine.UI;

namespace Code.QM.UI
{

	/// <summary>
	/// Add this component to a button to toggle the given GameObject on and off. 
	/// 
	/// Initial state depends on the activity setting of the gameobject whatToToggle.
	/// GameObject to toggle must be set in Inspector, otherwise the script will warn you and simply not work.
	/// </summary>
	[RequireComponent (typeof(Button))]
	public class OnOffToggler : MonoBehaviour
	{
		public GameObject whatToToggle;

		Button toggleButton;

		void Start ()
		{
			toggleButton = gameObject.GetComponentInChildren<Button> ();

			if (toggleButton == null) {
				Log.SignalErrorToDeveloper (
					"{0} script could not be activated: missing a Button script on gameobject {1}.",
					this.GetType ().Name,
					gameObject.name
				);
				return;
			} 

			toggleButton.onClick.AddListener (ToggleOnOff);
		}

		public void ToggleOnOff ()
		{
			whatToToggle.SetActive (!whatToToggle.activeSelf);
			Canvas canvasToToggle = whatToToggle.GetComponent<Canvas> ();
			if (canvasToToggle != null) {
				bool cache = canvasToToggle.overrideSorting;
				canvasToToggle.overrideSorting = false;
				canvasToToggle.overrideSorting = cache; 
			}
		}

	}
}
