﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GQ.Client.UI;
using UnityEngine.UI;
using GQ.Client.Model;

namespace GQ.Client.WCC
{
	
	public class ShowImageCopyRight : MonoBehaviour
	{
		Text copyrightText;

		private const string DEFAULT_COPYRIGHT_TEXT = "© WCC";
		private const string COPYRIGHT_PREFIX = "© ";

		void Start ()
		{
			Reset ();

			/// get copyright text from variable value:
			string copyright = Variables.GetValue ("bildrechte").AsString ();

			// normalize copyright text:
			if (copyright == "") {
				copyright = DEFAULT_COPYRIGHT_TEXT;
			} else {
				if (copyright.StartsWith ("©")) {
					copyright = copyright.Substring (1);
					copyright.TrimStart (' ');
				} else {
					copyright = COPYRIGHT_PREFIX + copyright;
				}
			}

			// replace andy line breaks by one space:
			copyright.Replace('\n', ' ');

			// set copyright text in game object:
			copyrightText.text = copyright;
		}

		void Reset() {
			copyrightText = GetComponent<Text> ();
		}

	}
}
