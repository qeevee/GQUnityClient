﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GQ.Client.Err;

public class ElipsifyOverflowingText : MonoBehaviour {

	public Text Text;

	void Reset () {
		Text foundText = gameObject.GetComponent<Text> ();
		if (foundText != null) {
			Text = foundText;
		}
		else {
			Log.SignalErrorToDeveloper ("Script " + GetType ().Name + " needs a Text GameObject to be set to its text variable.");
		}
	}

	// Use this for initialization
	void Start () {
		Canvas.ForceUpdateCanvases();

		if (LayoutUtility.GetPreferredWidth(Text.rectTransform) <= Text.rectTransform.rect.width) {
			// text just fits well:
			return;
		}

		// we have to elipsify the text:
		int reduceLastChars = 2; // maybe if the text ends with two long characters like "mm" we just need to replace these with the elipse ("...").

		do {
			Text.text = Text.text.Substring (0, Text.text.Length - reduceLastChars) + "...";
			reduceLastChars++; // already increase for next round in this loop.
		} while (LayoutUtility.GetPreferredWidth (Text.rectTransform) > Text.rectTransform.rect.width);
	}

}
