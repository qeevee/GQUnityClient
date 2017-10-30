﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GQ.Client.Conf;

namespace GQ.Client.UI {

	/// <summary>
	/// Configures the header layout based on the seetings in the current apps config data. Attach this script to all header game objects.
	/// </summary>
	[RequireComponent (typeof(Image)), RequireComponent (typeof(LayoutElement))]
	public class HeaderLayoutConfig : LayoutConfig {

		protected override void layout() {
			// set background color:
			Image image = GetComponent<Image> ();
			if (image != null) {
				image.color = ConfigurationManager.Current.headerBgColor;
			}

			// set height:
			LayoutElement layElem = GetComponent<LayoutElement>();
			if (layElem != null) {
				layElem.flexibleHeight = ConfigurationManager.Current.headerHeightPermill;
			}
		}
	}

}