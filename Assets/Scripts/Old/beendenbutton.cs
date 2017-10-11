﻿using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using GQ.Client.Conf;
using GQ.Client.Model;

public class beendenbutton : MonoBehaviour
{


	public questdatabase questdb;




	// Use this for initialization
	void Start ()
	{
		questdb = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ();


	}
	
	// Update is called once per frame
	void Update ()
	{
	
		if (questdb == null) {

			if (GameObject.Find ("QuestDatabase") != null) {
				questdb = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ();
			}

		} else if (QuestManager.Instance.CurrentQuest != null) {

			GetComponent<Image> ().enabled = true;
			GetComponent<Button> ().enabled = true;
			GetComponentInChildren<Text> ().enabled = true;

			if (ConfigurationManager.Current.questVisualization == "map") {
				
				GetComponentInChildren<Text> ().text = "Zurück zur Karte";
			} else {
				GetComponentInChildren<Text> ().text = "Beenden";

			}
		} else {

			GetComponent<Image> ().enabled = false;
			GetComponent<Button> ().enabled = false;
			GetComponentInChildren<Text> ().enabled = false;

		}






	}
}