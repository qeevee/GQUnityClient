﻿using System.Collections;
using Code.GQClient.Err;
using Code.GQClient.Util.tasks;

namespace Code.GQClient.Model.mgmt.quests
{

    public class QuestStarter : Task
	{

		public QuestStarter () : base ()
		{
		}

		private string gameXML { get; set; }

        protected override void ReadInput(object input = null)
        {
            if (input == null)
            {
                RaiseTaskFailed();
                return;
            }

            if (input is string)
            {
                gameXML = input as string;
            }
            else
            {
                Log.SignalErrorToDeveloper(
                    "Improper TaskEventArg received in SyncQuestData Task. Should be of type string but was " +
                    input.GetType().Name);
                RaiseTaskFailed();
                return;
            }
        }

        protected override IEnumerator DoTheWork ()
		{
            // step 1 deserialize game.xml:
            long xmlLength = gameXML.Length;
            QuestManager.Instance.SetCurrentQuestFromXml (gameXML);
            QuestManager.Instance.CurrentQuest.InitMediaStore();
            yield return null;

            // step 2 start the quest:
            QuestManager.Instance.CurrentQuest.Start ();
            RaiseTaskCompleted();
		}
	}
}
