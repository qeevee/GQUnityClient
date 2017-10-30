﻿using UnityEngine;
using System.Collections;
using GQ.Client.Model;
using UnityEngine.UI;
using System;
using GQ.Client.Util;
using GQ.Client.Err;
using GQ.Client.GQEvents;
using GQ.Client.Conf;
using GQ.Client.UI.Dialogs;
using System.IO;
using GQ.Client.FileIO;

//using UnityEngine.Events;


namespace GQ.Client.UI.Foyer
{

	/// <summary>
	/// Represents one quest info object in a list within the foyer.
	/// </summary>
	public class QuestListElementController : QuestInfoController
	{

		#region Content and Structure

		protected static readonly string PREFAB = "QuestInfoListElement";

		protected const string NAME_PATH = "Name";

		/// <summary>
		/// The download button is available WHEN this quest is on server but not on device.
		/// (IsOnServer && !IsOnDevice)
		/// </summary>
		public Button DownloadButton;

		/// <summary>
		/// The start button is available WHEN this quest is on device.
		/// (IsOnDevice)
		/// </summary>
		public Button StartButton;
		
		/// <summary>
		/// The update button is available WHEN this quest is on device and a newer version is on server.
		/// (HasUpdate)
		/// </summary>
		public Button UpdateButton;

		/// <summary>
		/// The delete button is available WHEN this quest is locally on device.
		/// (IsOnDevice)
		/// If it is not on server a warning is issued before deletion will be executed.
		/// (&& !IsOnServer)
		/// If this quest is also predeployed, an information is issued that the predeployed and older version
		/// will remain in the list. That version is always older, since only a newer version can ever 
		/// have been loaded as an update of the original predeployed version.
		/// (&& IsPredeployed)
		/// </summary>
		// TODO what happens if we take predeployed into account.
		public Button DeleteButton;

		private enum DeletionWarning
		{
			NoWarning,
			WarningNotOnServer,
			InfoPredeployedSurvivesDelete
		}

		private DeletionWarning DeletionWarnState {
			get {
				if (!data.IsOnServer) {
					return DeletionWarning.WarningNotOnServer;
				}
				if (data.IsPredeployed) {
					return DeletionWarning.InfoPredeployedSurvivesDelete;
				}
				return DeletionWarning.NoWarning;
			}
		}

		#endregion


		#region Internal UI Control Functions

		protected void HideAllButtons ()
		{
			DownloadButton.gameObject.SetActive (false);
			StartButton.gameObject.SetActive (false);
			DeleteButton.gameObject.SetActive (false);
			UpdateButton.gameObject.SetActive (false);
		}

		/// <summary>
		/// Shows (additionally) the given buttons and add the given method to the onClick listener.
		/// </summary>
		/// <param name="button">Button.</param>
		/// <param name="actionCallback">Action callback.</param>
		protected void ShowButtons (params Button[] buttons)
		{
			foreach (Button button in buttons) {
				button.gameObject.SetActive (true);
				button.interactable = true;
			}
			// in case we can start this quest, we also allow clicks on the quest name to start it:
			Button.ButtonClickedEvent namebuttonEvent = Name.GetComponent<Button> ().onClick;
			if (StartButton.gameObject.activeInHierarchy) {
				namebuttonEvent.RemoveAllListeners ();
				namebuttonEvent.AddListener (Play);
			} else {
				namebuttonEvent.RemoveAllListeners ();
			}
		}

		#endregion


		#region Event Reaction Methods

		public void Download ()
		{
			// Load quest data: game.xml
			Downloader downloadGameXML = 
				new Downloader (
					url: QuestManager.GetQuestURI (data.Id), 
					timeout: ConfigurationManager.Current.downloadTimeOutSeconds * 1000,
					targetPath: QuestManager.GetLocalPath4Quest (data.Id) + QuestManager.QUEST_FILE_NAME
				);
			new DownloadDialogBehaviour (downloadGameXML, "Loading quest");

			// analyze game.xml, gather all media info compare to local media info and detect missing media
			PrepareMediaInfoList prepareMediaInfosToDownload = 
				new PrepareMediaInfoList ();
			new SimpleDialogBehaviour (
				prepareMediaInfosToDownload,
				"Synching Quest Data",
				"Preparing media information."
			);

			// download all missing media info
			MultiDownloader downloadMediaFiles =
				new MultiDownloader (1);
			new SimpleDialogBehaviour (
				downloadMediaFiles,
				"Synching Quest Data",
				"Loading media files."
			);
			downloadMediaFiles.OnTaskCompleted += (object sender, TaskEventArgs e) => {
				data.LastUpdateOnDevice = data.LastUpdateOnServer;
			};

			// store current media info locally
			ExportMediaInfoList exportLocalMediaInfo =
				new ExportMediaInfoList ();
			new SimpleDialogBehaviour (
				exportLocalMediaInfo,
				"Synching Quest Data",
				"Saving updated media info."
			);

			ExportQuestInfosToJSON exportQuestsInfoJSON = 
				new ExportQuestInfosToJSON ();
			new SimpleDialogBehaviour (
				exportQuestsInfoJSON,
				"Updating quests",
				"Saving Quest Data"
			);

			TaskSequence t = 
				new TaskSequence (downloadGameXML);
			t.AppendIfCompleted (prepareMediaInfosToDownload);
			t.Append (downloadMediaFiles);
			t.AppendIfCompleted (exportLocalMediaInfo);
			t.Append (exportQuestsInfoJSON);

			t.Start ();
		}

		public void Delete ()
		{
			Files.DeleteDirCompletely (QuestManager.GetLocalPath4Quest (data.Id));
			data.LastUpdateOnDevice = null;

			ExportQuestInfosToJSON exportQuestsInfoJSON = 
				new ExportQuestInfosToJSON ();
			new SimpleDialogBehaviour (
				exportQuestsInfoJSON,
				"Updating quests",
				"Saving Quest Data"
			);

			exportQuestsInfoJSON.Start ();
		}

		public void Play ()
		{
			// Load quest data: game.xml
			LocalFileLoader loadGameXML = 
				new LocalFileLoader (
					filePath: QuestManager.GetLocalPath4Quest (data.Id) + QuestManager.QUEST_FILE_NAME
				);
			new DownloadDialogBehaviour (loadGameXML, "Loading quest");

			QuestStarter questStarter = new QuestStarter ();

			TaskSequence t = 
				new TaskSequence (loadGameXML, questStarter);

			t.Start ();
		}


		/// <summary>
		/// Called when the update button is pressed.
		/// </summary>
		public void UpdateQuest ()
		{
			// TODO
			Debug.Log ("TODO: Implement update method! Trying to update quest " + data.Name);
		}

		#endregion


		#region Runtime API

		public static GameObject Create (GameObject root, QuestInfo qInfo)
		{
			// CReate the view object for this controller:
			GameObject go = PrefabController.Create (PREFAB, root);
			go.name = PREFAB + " (" + qInfo.Name + ")";
			QuestListElementController ctrl = go.GetComponent<QuestListElementController> ();
			ctrl.data = qInfo;
			ctrl.data.OnChanged += ctrl.UpdateView;
			ctrl.UpdateView ();
			return go;
		}

		public override void UpdateView ()
		{
			// Update Info-Icon:
			// TODO: enable Info dialog

			// Update Name:
			Name.text = data.Name;
			// Set Name button for download or play or nothing:
			Button nameButton = Name.gameObject.GetComponent<Button> ();
			Button.ButtonClickedEvent namebuttonEvent = nameButton.onClick;
			namebuttonEvent.RemoveAllListeners ();
			if (data.IsOnServer && !data.IsOnDevice) {
				namebuttonEvent.AddListener (() => {
					Download ();
				});
			}
			if (data.IsOnDevice) {
				namebuttonEvent.AddListener (() => {
					Play ();
				});
			}


			// Update Buttons:
			HideAllButtons ();
			// Show DOWNLOAD button if needed:
			if (data.IsOnServer && !data.IsOnDevice) {
				DownloadButton.gameObject.SetActive (true);
				DownloadButton.interactable = true;
			}
			// Show START button if needed:
			if (data.IsOnDevice) {
				StartButton.gameObject.SetActive (true);
				StartButton.interactable = true;
			}
			// Show UPDATE button if needed:
			if (data.HasUpdate) {
				UpdateButton.gameObject.SetActive (true);
				UpdateButton.interactable = true;
			}
			// Show DELETE button if needed:
			if (data.IsOnDevice) {
				DeleteButton.gameObject.SetActive (true);
				DeleteButton.interactable = true;
			}

			ElipsifyOverflowingText elipsify = Name.GetComponent<ElipsifyOverflowingText> ();
			if (elipsify != null) {
				elipsify.ElipsifyText ();
			}
			// TODO make elipsify automatic when content of name text changes....???!!!

			// TODO call the lists sorter ...
		}

		#endregion


		#region Initialization in Editor

		public virtual void Reset ()
		{
			Name = EnsurePrefabVariableIsSet<Text> (Name, "Name", NAME_PATH);

			DownloadButton = EnsurePrefabVariableIsSet<Button> (DownloadButton, "Download Button", "DownloadButton");
			StartButton = EnsurePrefabVariableIsSet<Button> (StartButton, "Start Button", "StartButton");
			DeleteButton = EnsurePrefabVariableIsSet<Button> (DeleteButton, "Delete Button", "DeleteButton");
			UpdateButton = EnsurePrefabVariableIsSet<Button> (UpdateButton, "Update Button", "UpdateButton");
		}

		#endregion
	}

}