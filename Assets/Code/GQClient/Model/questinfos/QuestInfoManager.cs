﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using System;
using GQ.Client.Util;
using GQ.Client.Conf;
using GQ.Client.Model;
using GQ.Client.Util;
using Newtonsoft.Json;
using System.IO;
using GQ.Client.Err;
using GQ.Client.UI.Dialogs;


namespace GQ.Client.Model {

	/// <summary>
	/// Manages the meta data for all quests available: locally on the device as well as remotely on the server.
	/// </summary>
	public class QuestInfoManager {

		#region store & access data

		public static string LocalQuestsPath {
			get {
				if (!Directory.Exists(Application.persistentDataPath + "/quests/")) {
					Directory.CreateDirectory(Application.persistentDataPath + "/quests/");
				}
				return Application.persistentDataPath + "/quests/";
			}
		}

		public static string LocalQuestInfoJSONPath {
			get {
				return LocalQuestsPath + "infos.json";
			}
		}

		protected Dictionary<int, QuestInfo> QuestDict {
			get;
			set;
		}

		public List<QuestInfo> GetListOfQuestInfos() {
			return new List<QuestInfo> (QuestDict.Values);
		}

		public bool ContainsQuestInfo(int id) {
			return QuestDict.ContainsKey (id);
		}

		public int Count {
			get {
				return QuestDict.Count;
			}
		}

		public QuestInfo GetQuestInfo (int id) {
			QuestInfo questInfo;
			return (QuestDict.TryGetValue(id, out questInfo) ? questInfo : null);
		}

		#endregion


		#region Quest Info Changes

		public void AddInfo(QuestInfo newInfo) {
			QuestDict.Add (newInfo.Id, newInfo);

			// TODO Run through filter and raise event if involved

			raiseChange (
				new QuestInfoChangedEvent (
					String.Format ("Info for quest {0} added.", newInfo.Name),
					ChangeType.Added,
					newQuestInfo: newInfo
				)
			);
		}

		public void RemoveInfo(QuestInfo oldInfo) {
			QuestDict.Remove (oldInfo.Id);

			// TODO Run through filter and raise event if involved

			raiseChange (
				new QuestInfoChangedEvent (
					String.Format ("Info for quest {0} removed.", oldInfo.Name),
					ChangeType.Removed,
					oldQuestInfo: oldInfo
				)
			);
		}

		public void ChangeInfo(QuestInfo info) {
			QuestInfo oldInfo;
			if (!QuestDict.TryGetValue (info.Id, out oldInfo)) {
				Log.SignalErrorToDeveloper (
					"Trying to change quest info {0} but it deos not exist in QuestInfoManager.", 
					info.Id.ToString()
				);
				return;
			}

			QuestDict.Remove (info.Id);
			QuestDict.Add (info.Id, info);

			// TODO Run through filter and raise event if involved

			raiseChange (
				new QuestInfoChangedEvent (
					String.Format ("Info for quest {0} changed.", info.Name),
					ChangeType.Changed,
					newQuestInfo: info,
					oldQuestInfo: oldInfo
				)
			);
		}

		public void UpdateQuestInfos() {
			ImportQuestInfosFromJSON importLocal = 
				new ImportQuestInfosFromJSON (false);
			new SimpleDialogBehaviour (
				importLocal,
				"Updating quests",
				"Reading local quests."
			);

			Downloader downloader = 
				new Downloader (
					url: ConfigurationManager.UrlPublicQuestsJSON, 
					timeout: ConfigurationManager.Current.downloadTimeOutSeconds * 1000);
			new DownloadDialogBehaviour (downloader, "Updating quests");

			ImportQuestInfosFromJSON importFromServer = 
				new ImportQuestInfosFromJSON (true);
			new SimpleDialogBehaviour (
				importFromServer,
				"Updating quests",
				"Reading all found quests into the local data store."
			);

			ExportQuestInfosToJSON exporter = 
				new ExportQuestInfosToJSON ();
			new SimpleDialogBehaviour (
				exporter,
				"Updating quests",
				"Saving Quest Data"
			);

			TaskSequence t = new TaskSequence (importLocal, downloader);
			t.AppendIfCompleted (importFromServer);
			t.Append(exporter);
			t.Start ();
		}

		public delegate void ChangeCallback (object sender, QuestInfoChangedEvent e);

		public event ChangeCallback OnChange;

		protected virtual void raiseChange (QuestInfoChangedEvent e)
		{
			if (OnChange != null)
				OnChange (this, e);
		}

		#endregion


		#region singleton

		private static QuestInfoManager _instance = null;

		public static QuestInfoManager Instance {
			get {
				if ( _instance == null ) {
					_instance = new QuestInfoManager();
				}
				return _instance;
			}
			set {
				_instance = value;
			}
		}

		public static void Reset () {
			_instance = null;
		}

		public QuestInfoManager() {
			QuestDict = new Dictionary<int, QuestInfo> ();
		}

		#endregion
	}

	public class QuestInfoChangedEvent : EventArgs 
	{
		public string Message { get; protected set; }
		public ChangeType ChangeType { get; protected set; }
		public QuestInfo NewQuestInfo { get; protected set; }
		public QuestInfo OldQuestInfo { get; protected set; }

		public QuestInfoChangedEvent(
			string message = "", 
			ChangeType type = ChangeType.Changed, 
			QuestInfo newQuestInfo = null, 
			QuestInfo oldQuestInfo = null
		)
		{
			Message = message;
			ChangeType = type;
			NewQuestInfo = newQuestInfo;
			OldQuestInfo = oldQuestInfo;
		}

	}

	public enum ChangeType {
		Added, Removed, Changed
	}
		

}
