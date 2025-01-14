﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.GQClient.Err;
using UnityEngine;

namespace GQClient.Model
{
    /// <summary>
    /// Imports quest infos from JSON files. Either form the servers listing of all quest infos that are available, 
    /// or form the local json file which keeps track of the latest state of local and remote quest infos.
    /// 
    /// In order to import the server info, you need to use a downloader task before and 
    /// simply call the constructor of this class with 'true'). 
    /// 
    /// To load the local json file use 'false' as parameter of the constructor. 
    /// In this case no download task is needed and if existent its result will be ignored.
    /// </summary>
    public class ImportLocalQuestInfos : ImportQuestInfos
    {
        public ImportLocalQuestInfos() : base()
        {
            // import from local quest json file:
            if (File.Exists(QuestInfoManager.LocalQuestInfoJsonPath))
            {
                try
                {
                    InputJson = File.ReadAllText(QuestInfoManager.LocalQuestInfoJsonPath);
                }
                catch (Exception e)
                {
                    Log.SignalErrorToDeveloper("Error while trying to import local quest info json file: " + e.Message);
                    InputJson = "[]";
                }
            }
        }

        protected override void ReadInput(object input = null)
        {
            // we read directly from local file cf. constructor.
        }

        protected override void updateQuestInfoManager(IEnumerable<QuestInfo> quests)
        {
            qim.AddLocalInfos(quests);
            RaiseTaskCompleted();
        }
    }
}