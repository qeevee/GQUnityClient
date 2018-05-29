﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GQ.Client.Model;

public class AnswerCtrl : MonoBehaviour
{


	#region Inspector & internal features

	public Image answerImage;
	public Text answerText;
	public Button answerButton;

	private PageMultipleChoiceQuestion page;
	private Answer answer;

	#endregion


	#region Runtime API

	public static AnswerCtrl Create (PageMultipleChoiceQuestion mcqPage, Transform rootTransform, Answer answer)
	{
		GameObject go = (GameObject)Instantiate (
			                Resources.Load ("Answer"),
			                rootTransform,
			                false
		                );
		go.SetActive (true);

		AnswerCtrl answerCtrl = go.GetComponent<AnswerCtrl> ();
		answerCtrl.page = mcqPage;
		answerCtrl.answer = answer;
		answerCtrl.answerText.text = answer.Text;
		answerCtrl.answerButton.onClick.AddListener (answerCtrl.Select);

		return answerCtrl;
	}

	public void Select ()
	{
		page.Result = answer.Text;
		if (answer.Correct) {
			page.Succeed ();
		} else {
			page.Fail ();
		}
	}

	#endregion

}