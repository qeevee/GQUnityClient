﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.GQClient.UI.author;
using GQClient.Model;
using Code.GQClient.Util.tasks;
using Unity.Profiling.LowLevel.Unsafe;
using UnityEngine;

namespace Code.GQClient.Model.mgmt.quests
{
    public class AutoLoadAndUpdate : Task
    {
        protected override IEnumerator DoTheWork()
        {
            var questInfoList = QuestInfoManager.Instance.GetListOfQuestInfos();
            var downloadList = new List<QuestInfo>();
            var updateList = new List<QuestInfo>();

            foreach (var qi in questInfoList)
            {
                if (!qi.IsHidden() && qi.LoadOptionPossibleInTheory && !qi.LoadModeAllowsManualLoad)
                {
                    downloadList.Add(qi);
                    continue;
                }


                if (qi.UpdateOptionPossibleInTheory &&
                    (
                        !qi.LoadModeAllowsManualUpdate || (
                            qi.IsHidden() && !Author.ShowHiddenQuests
                        )
                    )
                )
                {
                    updateList.Add(qi);
                }
            }

            if (downloadList.Count + updateList.Count == 0)
            {
                RaiseTaskCompleted();
                yield break;
            }

            var counterDialog =
                new CounterDialog(
                    "Es gibt Neuigkeiten! Wir aktualisieren deine App jetzt.",
                    "Bitte habe etwas Geduld, wir laden noch {0} Inhalte ...",
                    downloadList.Count + updateList.Count,
                    3.5f);
            counterDialog.Start();

            foreach (var questInfo in downloadList)
            {
                var downloader = questInfo.Download();
                if (downloader == null)
                    continue;

                downloader.OnTaskEnded += (sender, args) => { counterDialog.Counter--; };
            }

            foreach (var questInfo in updateList)
            {
                var update = questInfo.Update();
                if (update == null)
                    continue;

                update.OnTaskEnded += (sender, args) => { counterDialog.Counter--; };
            }

            //Debug.Log("#### AUTO HAS loaded: " + loadCounter + " and updated: " + updateCounter);
            RaiseTaskCompleted();
        }
    }
}