﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GQ.Client.Conf;

namespace GQ.Client.UI
{

	/// <summary>
	/// Add this script to map screens.
	/// </summary>
	public class MapLayoutConfig : ScreenLayoutConfig
	{

		public GameObject MapButtonPanel;

		public override void layout ()
		{
			base.layout ();

			// TODO set background color for button panel:

			// TODO set button background color & height:
			for (int i = 0; i < MapButtonPanel.transform.childCount; i++) {
				GameObject perhapsAButton = MapButtonPanel.transform.GetChild (i).gameObject;
				Button button = perhapsAButton.GetComponent<Button> ();
				if (button != null) {
					LayoutElement layElem = perhapsAButton.GetComponent<LayoutElement> ();
					if (layElem != null) {
						layElem.preferredHeight = Units2Pixels (MapButtonHeightUnits);
						layElem.preferredWidth = layElem.preferredHeight;
					}
				}
			}
		}

		static public float MapButtonHeightUnits {
			get {
				float result = 
					calculateRestrictedHeight (
						ConfigurationManager.Current.mapButtonHeightUnits,
						ConfigurationManager.Current.mapButtonHeightMinMM,
						ConfigurationManager.Current.mapButtonHeightMaxMM
					);
				return result;
			}
		}

		static public float MarkerHeightUnits {
			get {
				float result = 
					calculateRestrictedHeight (
						ConfigurationManager.Current.markerHeightUnits,
						ConfigurationManager.Current.markerHeightMinMM,
						ConfigurationManager.Current.markerHeightMaxMM
					);
				return result;
			}
		}

	}

}