﻿using UnityEngine;
using System.Collections;
using GQ.Client.Conf;

public class deactivateGameObjectInDownloadAllQuestsMode : MonoBehaviour {




	public GameObject elseActivate;
	// Use this for initialization
	void Start () {

		if ( Configuration.instance.downloadAllCloudQuestOnStart ) {


			gameObject.SetActive(false);

			if ( elseActivate != null ) {

				elseActivate.SetActive(true);

			}


		}


	}
	

}
