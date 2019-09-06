﻿using UnityEngine;
using GQ.Client.Err;
using TMPro;

namespace GQ.UI
{

    [RequireComponent(typeof(TextMeshProUGUI))]
	public class ShowTextFromProductResource : MonoBehaviour
	{
        public RecourceFiles recourceTextType;

		protected TextMeshProUGUI contentText;

		// Use this for initialization
		void Start ()
        {
			contentText = GetComponent<TextMeshProUGUI> ();

            string recourceTextFileName = recourceTextType.ToString().ToLower();


            TextAsset textAsset = Resources.Load<TextAsset> (recourceTextFileName);
			if (textAsset != null) {
				contentText.text = textAsset.text;
            } else {
                Log.SignalErrorToDeveloper("This product is missing the recource text " + recourceTextFileName);
            }
		
		}
		
	}

    public enum RecourceFiles {
        Imprint,
        Privacy,
        Feedback
    }
}
