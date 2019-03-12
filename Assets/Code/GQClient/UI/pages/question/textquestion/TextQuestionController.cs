﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GQ.Client.Model;
using GQ.Client.Util;
using GQ.Client.Conf;

namespace GQ.Client.UI
{
	public class TextQuestionController : QuestionController
	{
		
		#region Inspector Features

		public Text questionText;
        public InputField inputField;
		public Text promptPlaceholder;
		public Text answerGiven;
		public Button forwardButton;

		#endregion


		#region Runtime API

		protected PageTextQuestion myPage;

		/// <summary>
		/// Is called during Start() of the base class, which is a MonoBehaviour.
		/// </summary>
		public override void Initialize ()
		{
			myPage = (PageTextQuestion)page;

			// show the question:
			questionText.color = ConfigurationManager.Current.mainFgColor;
			questionText.fontSize = ConfigurationManager.Current.mainFontSize;
			questionText.text = myPage.Question.Decode4HyperText();
			promptPlaceholder.text = myPage.Prompt;
			promptPlaceholder.fontSize = ConfigurationManager.Current.mainFontSize;
			answerGiven.fontSize = ConfigurationManager.Current.mainFontSize;
            answerGiven.text = "";
            inputField.text = "";
			forwardButton.transform.Find ("Text").GetComponent<Text> ().text = "Eingeben";
		}

		public override void OnForward ()
		{
			if (myPage.AnswerCorrect (answerGiven.text)) {
				myPage.Succeed ();
			} else {
                if (myPage.RepeatUntilSuccess)
                {
                    ((TextQuestionController)myPage.PageCtrl).Repeat();
                }
                else
                {
                    myPage.Fail();
                }

            }
        }

		#endregion
	}
}
