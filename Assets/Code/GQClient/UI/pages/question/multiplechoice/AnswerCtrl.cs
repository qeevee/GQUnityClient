﻿using Code.GQClient.Model.pages;
using Code.GQClient.start;
using Code.GQClient.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.GQClient.UI.pages.question.multiplechoice
{
	public class AnswerCtrl : MonoBehaviour
	{


		#region Inspector & internal features

		public Image answerImage;
		public TextMeshProUGUI answerText;
		public Button answerButton;

		private PageMultipleChoiceQuestion page;
		private MCQAnswer answer;

		#endregion


		#region Runtime API

		public static AnswerCtrl Create (PageMultipleChoiceQuestion mcqPage, Transform rootTransform, MCQAnswer answer)
		{
			GameObject go = (GameObject)Instantiate (
				AssetBundles.Asset ("prefabs", "Answer"),
				rootTransform,
				false
			);
			go.SetActive (true);

			AnswerCtrl answerCtrl = go.GetComponent<AnswerCtrl> ();
			answerCtrl.page = mcqPage;
			answerCtrl.answer = answer;

			// TODO SPECIAL Interpretation for Key-Value-Pairs (Key->Display, Value->Result)
			// TODO maybe we should move that into the Answer model as separate Display and Value fields also in editor.
			answerCtrl.answerText.text = answer.Text.HTMLDecode().DisplayString().Decode4TMP(false);
			answerCtrl.answerButton.onClick.AddListener (answerCtrl.Select);

			return answerCtrl;
		}

		public void Select ()
		{
			// TODO SPECIAL Interpretation for Key-Value-Pairs (Key->Display, Value->Result)
			// TODO maybe we should move that into the Answer model as separate Display and Value fields also in editor.
			page.Result = answer.Text.HTMLDecode().DisplayValueString().MakeReplacements();
			if (answer.Correct) {
				page.Succeed (alsoEnd: true);
			} else {
				if (page.RepeatUntilSuccess)
				{
					page.Fail(alsoEnd: false);
					((MultipleChoiceQuestionController)page.PageCtrl).Repeat();
				}
				else
				{
					page.Fail(alsoEnd: true);
				}
			}
		}

		#endregion

	}
}
