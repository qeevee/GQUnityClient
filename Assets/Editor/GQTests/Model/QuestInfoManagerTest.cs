﻿using UnityEngine;
using UnityEditor;
using NUnit.Framework;
using GQ.Client.Model;
using GQ.Editor.Util;
using System.IO;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace GQTests.Model {

	public class QuestInfoManagerTest {

		[SetUp]
		public void ResetQMInstance () {
			QuestInfoManager.Reset();
		}

		[Test]
		public void InitQM () {
			// Arrange:
			QuestInfoManager qm = null;

			// Act:
			qm = QuestInfoManager.Instance;

			// Assert:
			Assert.NotNull(qm);
			Assert.AreEqual(0, qm.Count);
		}

		[Test]
		public void ImportNull () {
			// Arrange:
			QuestInfoManager qm = QuestInfoManager.Instance;
			QuestInfo[] quests = null;

			// Act:
			qm.Import(quests);

			// Assert:
			IEnumerable<QuestInfo> questInfos = 
				from entry in qm.QuestDict
				select entry.Value;
			Assert.AreEqual(0, questInfos.Count());
		}



	}




}