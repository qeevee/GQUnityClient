﻿using UnityEngine;

namespace Code.GQClient.Util.input
{
	public class TestInputService : MonoBehaviour
	{

		// Use this for initialization
		void Start ()
		{
		
		}
	
		// Update is called once per frame
		void Update ()
		{
			if (Application.platform == RuntimePlatform.Android) {
				if (Input.GetKey (KeyCode.Escape)) {
					Debug.Log ("Android BACK BUTTON PRESSED");
					return;
				}
			}
		}
	}
}
