﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.GQClient.Conf;
using Code.GQClient.Err;
using GQClient.Model;
using Code.GQClient.UI.Foyer.questinfos;
using Code.GQClient.UI.layout;
using Code.GQClient.Util;
using UnityEngine;
using UnityEngine.UI;

namespace Code.GQClient.UI.Foyer.containers
{
    /// <summary>
    /// Shows all Quest Info objects, e.g. in a scrollable list within the foyer. Drives a dialog while refreshing its content.
    /// </summary>
    public class QuestListController : QuestContainerController
    {
        public Transform InfoList;
        public GameObject HiddenQuests;

        private bool RefreshOnStart;

        public void OnEnable()
        {
            // base.OnEnable();

            if (StartUpdateViewAlreadyDone)
            {
                // if we are already started earlier we refresh the list, since we might have switched from TopicTree
                RecreateTopicTree();
            }
            else
            {
                // let Start do the refresh of the list. Needed in case we switched from TopicTree to List
                RefreshOnStart = true;
            }

            RTConfig.RTConfigChanged.AddListener(RecreateTopicTree);
        }

        public void OnDisable()
        {
            StartUpdateViewAlreadyDone = false;
        }

        private void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            base.Start();

            if (RefreshOnStart && !StartUpdateViewAlreadyDone)
            {
                RecreateTopicTree();
            }

            StartUpdateViewAlreadyDone = true;
        }
        
        


        #region React on Events

        protected override void AddedInfo(QuestInfoChangedEvent e)
        {
            if (QuestInfoControllers.ContainsKey(e.NewQuestInfo.Id))
                return;

            QuestInfoUIC qiCtrl;
            qiCtrl =
                QuestInfoUICListElement.Create(
                    root: InfoList.gameObject,
                    qInfo: e.NewQuestInfo,
                    containerController: this
                ).GetComponent<QuestInfoUICListElement>();
            QuestInfoControllers.Add(e.NewQuestInfo.Id, qiCtrl);
            qiCtrl.Show();
            updateListSorting();
        }

        protected override void ChangedInfo(QuestInfoChangedEvent e)
        {
            QuestInfoUIC qiCtrl;

            if (e.NewQuestInfo == null || !QuestInfoControllers.TryGetValue(e.NewQuestInfo.Id, out qiCtrl))
            {
                Log.SignalErrorToDeveloper(
                    "Quest Info Controller for quest id {0} not found when a Change event occurred.",
                    e.NewQuestInfo.Id
                );
                return;
            }

            qiCtrl.UpdateData(e.NewQuestInfo);
            qiCtrl.Show();
            updateListSorting();
        }

        protected override void RemovedInfo(QuestInfoChangedEvent e)
        {
            QuestInfoUIC qiCtrl;
            if (!QuestInfoControllers.TryGetValue(e.OldQuestInfo.Id, out qiCtrl))
            {
                Log.SignalErrorToDeveloper(
                    "Quest Info Controller for quest id {0} not found when a Remove event occurred.",
                    e.OldQuestInfo.Id
                );
                return;
            }

            qiCtrl.Hide();
            QuestInfoControllers.Remove(e.OldQuestInfo.Id);
            updateElementOrderLayout();
        }

        public override void FilterChanged()
        {
            RegenerateAllAfterFilterChanged();
        }

        protected override void SorterChanged()
        {
            updateListSorting();
        }

        /// <summary>
        /// Sorts the list. Takes the current sorter into account to move the gameobjects in the right order.
        /// </summary>
        private void updateListSorting()
        {
            Base.Instance.StartCoroutine(sortViewAsCoroutine());
        }

        private IEnumerator sortViewAsCoroutine()
        {
            var qcList = new List<QuestInfoUIC>(QuestInfoControllers.Values);
            qcList.Sort();

            for (var i = 0; i < qcList.Count; i++)
            {
                if (qcList[i] == null)
                {
                    yield break;
                }

                qcList[i].transform.SetSiblingIndex(i);

                if (i % 5 == 0)
                    yield return new WaitForEndOfFrame();
            }

            updateElementOrderLayout(); // TODO wird sehr oft aufgerufen!!!
        }

        /// <summary>
        /// Updates the view.
        /// </summary>
        protected override void RecreateTopicTree()
        {
            Base.Instance.StartCoroutine(regenerateAllAsCoroutine());
        }

        /// <summary>
        /// Updates the view in coroutine mode. 
        /// 
        /// What happens is: 
        /// First all quest infos are deleted and the internal list is cleared. 
        /// Collect all filtered quest infos from the QuestInfoManager and create new controls for each.
        /// Sort the list according to the current sorting settings.
        /// 
        /// </summary>
        /// <returns>The view as coroutine.</returns>
        private IEnumerator regenerateAllAsCoroutine()
        {
            if (this == null)
            {
                yield break;
            }

            if (InfoList == null)
            {
                yield break;
            }

            // hide and delete all list elements:
            foreach (KeyValuePair<int, QuestInfoUIC> kvp in QuestInfoControllers)
            {
                kvp.Value.Hide();
                kvp.Value.Destroy();
            }

            QuestInfoControllers.Clear();

            //int steps = 0;

            IEnumerable<QuestInfo> questInfos = Qim.GetFilteredQuestInfos();
            foreach (QuestInfo info in questInfos)
            {
                // create new list elements
                var qiCtrl =
                    QuestInfoUICListElement.Create(
                        root: InfoList.gameObject,
                        qInfo: info,
                        containerController: this
                    ).GetComponent<QuestInfoUICListElement>();
                QuestInfoControllers[info.Id] = qiCtrl;
                
                qiCtrl.Show();

                //if (steps % 3 == 0)
                //{
                //    yield return null;
                //    steps = 0;
                //}
            }

            updateListSorting();
        }

        protected void RegenerateAllAfterFilterChanged()
        {
            if (this == null)
            {
                return;
            }

            if (InfoList == null)
            {
                return;
            }

            // we make a separate list of ids of all old quest infos:
            List<int> rememberedOldIDs = new List<int>(QuestInfoControllers.Keys);

            // we create new qi elements and keep those we can reuse. We remove those from our helper list.
            foreach (QuestInfo info in QuestInfoManager.Instance.GetFilteredQuestInfos())
            {
                QuestInfoUIC qiCtrl;
                if (QuestInfoControllers.TryGetValue(info.Id, out qiCtrl))
                {
                    qiCtrl.Show(); // shows also the hidden quests again???

                    // this new element was already there, hence we keep it:
                    rememberedOldIDs.Remove(info.Id);
                }
                else
                {
                    // Create not yet created qi controller, 
                    // e.g. after starting with this qi filtered out and changed the filter
                    // so we see it now for the first time.
                    QuestInfoUICListElement missingQICtrl =
                        QuestInfoUICListElement.Create(
                            root: InfoList.gameObject,
                            qInfo: info,
                            containerController: this
                        ).GetComponent<QuestInfoUICListElement>();
                    QuestInfoControllers[info.Id] = missingQICtrl;
                    QuestInfoControllers[info.Id].Show();
                }
            }

            // now in the helper list only the old unused elements are left. Hence we delete them:
            foreach (int oldID in rememberedOldIDs)
            {
                QuestInfoControllers[oldID].Hide();
            }

            updateListSorting();
        }


        private void updateElementOrderLayout()
        {
            if (Config.Current.listEntryDividingMode != ListEntryDividingMode.AlternatingColors)
                return;

            for (int i = 0; i < InfoList.childCount; i++)
            {
                QuestInfoUIC qic = InfoList.GetChild(i).GetComponent<QuestInfoUIC>();
                Color bgCol = i % 2 == 0
                    ? Config.Current.listEntryBgColor
                    : Config.Current.listEntrySecondBgColor;
                Color fgCol = i % 2 == 0
                    ? Config.Current.listEntryFgColor
                    : Config.Current.listEntrySecondFgColor;


                qic.gameObject.GetComponent<Image>().color = bgCol;
                FoyerListLayoutConfig.SetQuestInfoEntryLayout(qic.gameObject, "InfoButton", sizeScaleFactor: 0.65f,
                    fgColor: fgCol);
                qic.transform.Find("InfoButton/Image").GetComponent<Image>().color = fgCol;
                FoyerListLayoutConfig.SetQuestInfoEntryLayout(qic.gameObject, "Name", fgColor: fgCol);
                FoyerListLayoutConfig.SetQuestInfoEntryLayout(qic.gameObject, "DownloadButton", fgColor: fgCol);
                FoyerListLayoutConfig.SetQuestInfoEntryLayout(qic.gameObject, "StartButton", fgColor: fgCol);
                FoyerListLayoutConfig.SetQuestInfoEntryLayout(qic.gameObject, "DeleteButton", fgColor: fgCol);
                FoyerListLayoutConfig.SetQuestInfoEntryLayout(qic.gameObject, "UpdateButton", fgColor: fgCol);
            }
        }

        #endregion
    }
}