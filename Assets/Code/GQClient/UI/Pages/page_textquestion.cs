﻿using UnityEngine;
using UnityEngine.UI;

using System.Collections;
using System.Text.RegularExpressions;
using GQ.Client.Model;
using UnityEngine.SceneManagement;

public class page_textquestion : MonoBehaviour {

	
	
	public questdatabase questdb;
	public Quest quest;
	public Page textquestion;
	public Text questiontext;
	public Button submitbutton;
	public InputField input;
	
	// Use this for initialization
	void Start () {

		GameObject questdbGO = GameObject.Find("QuestDatabase");

		if ( questdbGO == null ) {

			SceneManager.LoadScene("questlist");
			return;
		}

		questdb = questdbGO.GetComponent<questdatabase>();
		quest = questdbGO.GetComponent<questdatabase>().currentquest;
		textquestion = questdbGO.GetComponent<questdatabase>().currentquest.currentpage;

		if ( textquestion.onStart != null ) {
			
			textquestion.onStart.Invoke();
		}
		
		questiontext.text = questdb.GetComponent<actions>().formatString(textquestion.getAttribute("question"));
	}

	public void checkAnswerFinal () {
		
		string x = input.text;

		bool repeat = false;
		
		if ( textquestion.hasAttribute("loopUntilSuccess") ) {
			if ( textquestion.getAttribute("loopUntilSuccess") == "true" ) {
				repeat = true;
			} 
		}

		if ( textquestion.contents_answers.Count > 0 ) {

			bool b = false;
			bool match;

			foreach ( QuestContent y in textquestion.contents_answers ) {
				if ( x == null || y == null || y.content == null )
					continue;
				
				match = Regex.IsMatch(x, y.content, RegexOptions.IgnoreCase);

				Debug.Log("TextQuestion: REGEXP " + x + " MATCH " + y.content + " -> " + match);

				questdb.debug("REGEXP " + x + " MATCH " + y.content + " -> " + match);

				if ( questdb.GetComponent<actions>().formatString(y.content) == x || match ) {
					b = true;
					Debug.Log("TextQuestion: MATCHED");
				}


			}

			if ( b ) {

				textquestion.state = "succeeded";
				onSuccess();
			}
			else {
				
				if ( !repeat ) {
					
					textquestion.state = "failed";
					onFailure();
				}

			}
	
		}
		else {

			textquestion.state = "succeeded";
		}
		textquestion.result = x;

		if ( textquestion.state == "succeeded" || !repeat ) {

			onEnd();
		}
	}

	public void onEnd () {
		
		if ( textquestion.state != "failed" ) {

			textquestion.state = "succeeded";
		}
		
		if ( textquestion.onEnd != null ) {
			
			textquestion.onEnd.Invoke();
		}
		else
		if ( !textquestion.onSuccess.hasMissionAction() && !textquestion.onFailure.hasMissionAction() ) {
			
			questdb.endQuest();
		}
	}

	public void onSuccess () {
		
		textquestion.state = "succeeded";
		
		
		if ( textquestion.onSuccess != null ) {
			
			textquestion.onSuccess.Invoke();
		} 
	}

	public void onFailure () {
		
		textquestion.state = "failed";
		
		
		if ( textquestion.onFailure != null ) {
			
			textquestion.onFailure.Invoke();
		} 
	}
}
