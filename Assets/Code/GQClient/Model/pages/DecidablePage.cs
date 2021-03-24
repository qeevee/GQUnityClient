﻿using System;
using System.Collections.Generic;
using System.Xml;
using Code.GQClient.Err;
using Code.GQClient.Model.actions;
using Code.GQClient.Model.gqml;
using Code.GQClient.Util;
using UnityEngine;

namespace Code.GQClient.Model.pages
{
    public abstract class DecidablePage : Page
    {

        #region XML Serialization
        public DecidablePage(XmlReader reader) : base(reader) { }

        protected override void ReadContent(XmlReader reader)
        {
            switch (reader.LocalName)
            {
                case GQML.ON_SUCCESS:
                    SuccessTrigger = new Trigger(reader);
                    SuccessTrigger.Parent = this;
                    break;
                case GQML.ON_FAIL:
                    FailTrigger = new Trigger(reader);
                    FailTrigger.Parent = this;
                    break;
                default:
                    base.ReadContent(reader);
                    break;
            }
        }

        protected Trigger SuccessTrigger = Trigger.Null;
        protected Trigger FailTrigger = Trigger.Null;

        #endregion


        #region Runtime API

        public override void Start(bool canReturnToPrevious = false)
        {
            base.Start(canReturnToPrevious);
        }

        public void Succeed(bool alsoEnd = true)
        {
            State = GQML.STATE_SUCCEEDED;

            if (SuccessTrigger != Trigger.Null)
            {
                SuccessTrigger.Initiate();
                if (alsoEnd)
                    End(false);
            }
            else
            {
                // end this page after succeeding:
                if (alsoEnd)
                    End(true);
            }
        }

        public void Fail(bool alsoEnd = true)
        {
            State = GQML.STATE_FAILED;

            if (FailTrigger != Trigger.Null)
            {
                FailTrigger.Initiate();
                if (alsoEnd)
                    End(false);
            }
            else
            {
                // end this page after failing:
                if (alsoEnd)
                    End(true);
            }
        }

        public override void End(bool leaveQuestIfEmpty = true)
        {
            if (EndTrigger == Trigger.Null)
            {
                if (leaveQuestIfEmpty)
                {
                    Log.SignalErrorToAuthor(
                        "Quest {0} ({1}, page {2} has no actions onEnd defined, hence we end the quest here.",
                        Quest.Name, Quest.Id,
                        Id
                    );
                    Quest.End();
                }
            }
            else
            {
                EndTrigger.Initiate();
            }
            Resources.UnloadUnusedAssets();
        }

        protected List<IText> ExpectedCodes = new List<IText>();

        public virtual bool AnswerCorrect(string input)
        {
            if (input == null)
                return false;

            foreach (ExpectedCode a in ExpectedCodes)
            {
                // TODO: Extract this into a reusable method that will be called from here and from TextQuestionPage as well.
                string aText = a.Text.Trim().MakeReplacements();

                // Text comparison:
                if (aText == input.Trim().MakeReplacements())
                    return true;

                // Number Ranges:
                if (aText.StartsWith("[[") && aText.EndsWith("]]"))
                {
                    string[] rangeBounds = aText.Substring(2, aText.Length - 4).Split('-');
                    if (rangeBounds.Length == 2)
                    {
                        try
                        {
                            double lowerBound = Convert.ToDouble(rangeBounds[0]);
                            double upperBound = Convert.ToDouble(rangeBounds[1]);

                            if (upperBound < lowerBound)
                            {
                                double swapTmp = upperBound;
                                upperBound = lowerBound;
                                lowerBound = swapTmp;
                            }

                            // bounds are ok:
                            double number;
                            try
                            {
                                number = Convert.ToDouble(input.Trim());

                                // now we test wether input is in range:
                                return (lowerBound <= number && number <= upperBound);
                            }
                            catch (FormatException)
                            {
                                Log.SignalErrorToUser($"Eingabe '{input}' kann nicht als Zahl erkannt werden.");
                                return false;
                            }
                            catch (OverflowException)
                            {
                                Log.SignalErrorToUser($"Eingabe '{input}' zu groß oder zu klein um als Zahl benutzt zu werden.");
                                return false;
                            }
                        }
                        catch (Exception)
                        {
                            Log.SignalErrorToAuthor(
                                $"In Quest {Quest.Id} auf Seite {Id} kann Antwort '{a.Text}' nicht als Zahlenbereich erkannt werden.");
                            return false;
                        }
                    }
                }

                // TODO RegExp
            }


            return false;
        }

        #endregion

    }
}
