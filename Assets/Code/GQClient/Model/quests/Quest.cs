﻿using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Linq;
using System.Text;
using GQ.Geo;
using GQ.Util;
using GQ.Client.Conf;
using System.ComponentModel;
using System.Xml.Schema;
using System;
using GQ.Client.Err;

namespace GQ.Client.Model
{

	[System.Serializable]
	[XmlRoot (GQML.QUEST)]
	public class Quest  : IComparable<Quest>, IXmlSerializable
	{

		#region Attributes

		public string Name { get; set; }

		public int Id { get; set; }
		// TODO: make setter protected

		public long LastUpdate { get; set; }

		public string XmlFormat { get; set; }

		public bool IndividualReturnDefinitions { get; set; }

		public bool IsHidden {
			get {
				// TODO change the latter two checks to test a flag stored in game.xml base element as an attribute
				return (ConfigurationManager.Current.hideHiddenQuests && Name != null && Name.StartsWith ("---"));
			}
		}

		#endregion

		#region State Pages

		[Obsolete]
		public List<Page>
			PageList = new List<Page> ();

		protected Dictionary<int, IPage> pageDict = new Dictionary<int, IPage> ();

		public IPage GetPageWithID (int id)
		{
			IPage page;
			if (pageDict.TryGetValue (id, out page)) {
				return page;
			} else {
				return Page.Null;
			}
		}

		protected IPage startPage;

		public IPage StartPage {
			get {
				if (startPage == null) {
					Log.SignalErrorToDeveloper ("Quest {0}({1}) can not be started, sonce start page is not set.", Name, Id);
					return null;
				} else
					return startPage;
			}
			set {
				startPage = value;
			}
		}

		protected IPage currentPage;

		public IPage CurrentPage {
			get {
				return currentPage;
			}
			internal set {
				currentPage = value;
			}
		}

		#endregion

		#region Hotspots

		[Obsolete]
		public List<QuestHotspot>
			hotspotList = new List<QuestHotspot> ();

		protected Dictionary<int, Hotspot> hotspotDict = new Dictionary<int, Hotspot> ();

		public Hotspot GetHotspotWithID (int id)
		{
			Hotspot hotspot;
			hotspotDict.TryGetValue (id, out hotspot);
			return hotspot;
		}

		public Dictionary<int, Hotspot>.ValueCollection AllHotspots {
			get {
				return hotspotDict.Values;
			}
		}
			
		#endregion

		#region Move to QuestImporter // TODO use event system instead

		private static Quest currentlyParsingQuest;

		public static Quest CurrentlyParsingQuest {
			get {
				if (currentlyParsingQuest == null)
					currentlyParsingQuest = Quest.Null;
				return currentlyParsingQuest;
			}
			private set {
				currentlyParsingQuest = value;
			}
		}

		#endregion


		#region Structure

		public System.Xml.Schema.XmlSchema GetSchema ()
		{
			return null;
		}

		public void ReadXml (System.Xml.XmlReader reader)
		{
			CurrentlyParsingQuest = this; // TODO use event system instead

			// proceed to quest start element:
			while (!GQML.IsReaderAtStart (reader, GQML.QUEST)) {
				reader.Read ();
			}

			ReadAttributes (reader);

			// consume the begin quest element:
			reader.Read ();

			// Content:
			XmlRootAttribute xmlRootAttr = new XmlRootAttribute ();
			xmlRootAttr.IsNullable = true;
			XmlSerializer serializer;

			while (!GQML.IsReaderAtEnd (reader, GQML.QUEST)) {

				if (reader.NodeType != XmlNodeType.Element && !reader.Read ()) {
					return;
				}

				// now we are at an element:

				switch (reader.LocalName) {
				case GQML.PAGE:
					ReadPage (reader);
					break;
				case GQML.HOTSPOT:
					ReadHotspot (reader);
					break;
				}
			}

			CurrentlyParsingQuest = Null;
		}

		private void ReadAttributes (XmlReader reader)
		{
			Name = GQML.GetStringAttribute (GQML.QUEST_NAME, reader);
			Id = GQML.GetIntAttribute (GQML.QUEST_ID, reader);
			XmlFormat = GQML.GetStringAttribute (GQML.QUEST_XMLFORMAT, reader);
			LastUpdate = GQML.GetLongAttribute (GQML.QUEST_LASTUPDATE, reader);
			IndividualReturnDefinitions = GQML.GetOptionalBoolAttribute (GQML.QUEST_INDIVIDUAL_RETURN_DEFINITIONS, reader);
		}

		private void ReadPage (XmlReader reader)
		{
			// now the reader is at a page element:
			string pageTypeName = reader.GetAttribute (GQML.PAGE_TYPE);
			if (pageTypeName == null) {
				Log.SignalErrorToDeveloper ("Page without type attribute found.");
				reader.Skip ();
				return;
			}

			// Determine the full name of the according page type (e.g. GQ.Client.Model.XML.PageNPCTalk) 
			//		where SetVariable is taken form ath type attribute of the xml action element.
			string ruleTypeFullName = this.GetType ().FullName;
			int lastDotIndex = ruleTypeFullName.LastIndexOf (".");
			string modelNamespace = ruleTypeFullName.Substring (0, lastDotIndex);
			Type pageType = Type.GetType (string.Format ("{0}.Page{1}", modelNamespace, pageTypeName));

			if (pageType == null) {
				Log.SignalErrorToDeveloper ("No Implementation for Page Type {0} found.", pageTypeName);
				reader.Skip ();
				return;
			}

			XmlSerializer serializer = new XmlSerializer (pageType);
			IPage page = (IPage)serializer.Deserialize (reader);
			page.Parent = this;
			if (pageDict.Count == 0)
				StartPage = page;
			pageDict.Add (page.Id, page);

			// TODO: get rid:
			PageList.Add ((Page)page);
		}

		private void ReadHotspot (XmlReader reader)
		{
			XmlSerializer serializer = new XmlSerializer (typeof(Hotspot));
			Hotspot hotspot = (Hotspot)serializer.Deserialize (reader);
			hotspotDict.Add (hotspot.Id, hotspot);

			// TODO: get rid:
//			hotspotList.Add (hotspot);
		}
			

		public void WriteXml (System.Xml.XmlWriter writer)
		{
			Debug.LogWarning ("WriteXML not implemented for " + GetType ().Name);
		}

		#endregion


		#region Runtime API

		public virtual void Start ()
		{
			if (StartPage == null)
				return;

			StartPage.Start ();
		}

		public void GoBackOnePage ()
		{
			Page show = previouspages [previouspages.Count - 1];
			previouspages.Remove (previouspages [previouspages.Count - 1]);

			if (_allowReturn > 0)
				_allowReturn--;

			questdatabase questdb = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ();
			questdb.changePage (show.Id);

		}

		[SerializeField]
		private int _allowReturn = 0;

		public bool AllowReturn {
			get {
				if (IndividualReturnDefinitions) {
					return (
					    _allowReturn > 0
					    && previouspages.Count > 0
					    && previouspages [previouspages.Count - 1] != null
					);
				} else {
					return (
					    previouspages.Count > 0
					    && previouspages [previouspages.Count - 1] != null
					    && !previouspages [previouspages.Count - 1].type.Equals (GQML.PAGE_TYPE_TEXT_QUESTION)
					    && !previouspages [previouspages.Count - 1].type.Equals (GQML.PAGE_TYPE_MULTIPLE_CHOICE_QUESTION)
					);
				}
			}
			set { 
				if (value == false)
					_allowReturn = 0;
				else
					_allowReturn++;
			}
		}

		public bool UsesLocation {
			get {
				return hotspotDict.Count > 0;
				// TODO check whether this quest contains map page or uses location system variables
			}
		}

		public bool SendsDataToServer {
			get {
				return false;
				// TODO check whether this quest contains map page or uses location system variables
			}
		}

		#endregion


		#region Null Object

		public static readonly Quest Null = new NullQuest ();

		private class NullQuest : Quest
		{

			public NullQuest ()
				: base ()
			{
				Name = "Null Quest";
				Id = 0;
				LastUpdate = 0;
				XmlFormat = "0";
				IndividualReturnDefinitions = false;
			}

			public override void Start ()
			{
				Log.WarnDeveloper ("Null Quest started.");
			}
		}

		#endregion


		#region Old Stuff TODO Reorganize

		public string filepath;



		[XmlAnyAttribute ()]
		public XmlAttribute[]
			help_attributes;
		public List<QuestAttribute> attributes;
		public List<QuestMetaData> metadata;
		public bool hasData = false;
		public Page currentpage;
		public List<Page> previouspages = new List<Page> ();
		public string xmlcontent;
		public float start_longitude;
		public float start_latitude;
		public string meta_combined;
		public string meta_Search_Combined;

		public bool predeployed = false;
		public string version;
		public bool acceptedDS = false;

		[XmlIgnore]
		public WWW www;

		public string alternateDownloadLink;

		public Quest ()
		{
			predeployed = false;
		}

		public long getLastUpdate ()
		{
			if (LastUpdate == 0) {
				long result;
				if (PlayerPrefs.HasKey (Id + "_lastUpdate") && long.TryParse (PlayerPrefs.GetString (Id + "_lastUpdate"), out result)) {
					return result;
				} else
					return 0;
			} else
				return LastUpdate;
		}

		public static Quest CreateQuest (int id)
		{
			Quest q = new Quest ();
			return q.LoadFromText (id, true);
		}

		public string getCategory ()
		{

			string x = "";

			if (hasMeta ("category")) {

				x = getMeta ("category");

			}

			return x;

		}

		public int CompareTo (Quest q)
		{

			if (q == null) {
				return 1;
			} else {

				return this.Name.ToUpper ().CompareTo (q.Name.ToUpper ());
			}

		}

		/// <summary>
		/// redo equals localload in case a download of the quest has preceeded. In this case it is false. (hm)
		/// </summary>
		/// <returns>The from text.</returns>
		/// <param name="id">Identifier.</param>
		/// <param name="redo">If set to <c>true</c> redo.</param>
		public  Quest LoadFromText (int id, bool redo)
		{
			string fp = filepath;
			string xmlfilepath = filepath;
			string xmlcontent_copy = xmlcontent;

			if (xmlcontent_copy != null && xmlcontent_copy.StartsWith ("<error>")) {
				string errMsg = xmlcontent_copy;

				GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ().showmessage (errMsg);
				return null;
			}

			if (filepath == null) {
				xmlfilepath = " ";

			}

			if (xmlcontent_copy == null) {

				xmlcontent_copy = " ";
			}

			Encoding enc = System.Text.Encoding.UTF8;

			TextReader txr = new StringReader (xmlcontent_copy);

			//		Debug.Log ("XML:"+xmlcontent_copy);

			if (!predeployed && xmlfilepath != null && xmlfilepath.Length > 9) {

				//			Debug.Log(xmlfilepath);

				if (!xmlfilepath.Contains ("game.xml")) {

					xmlfilepath = xmlfilepath + "game.xml";

				}
				txr = new StreamReader (xmlfilepath, enc);

			}
			XmlSerializer serializer = new XmlSerializer (typeof(Quest));

			Quest q = serializer.Deserialize (txr) as Quest; 
			QuestManager.Instance.CurrentQuest = q;

			q.xmlcontent = xmlcontent;
			q.predeployed = predeployed;

			q.filepath = fp;
			q.hasData = true;

			//q.id = id;
			//		Debug.Log ("my id is " + id + " -> " + q.id);
			q.deserializeAttributes (redo);
			q.meta_Search_Combined += q.Name + "; ";
			q.meta_combined += q.Name;


			if (metadata != null) {

				metadata.Clear ();
			} else {

				metadata = new List<QuestMetaData> ();
			}

			if (q.hasAttribute ("author")) {

				q.addMetaData (new QuestMetaData ("author", q.getAttribute ("author")));

			}

			if (q.hasAttribute ("version")) {

				q.addMetaData (new QuestMetaData ("version", q.getAttribute ("version")));

			}

			foreach (Page qp in q.PageList) {
				if (qp.type == "MetaData") {

					foreach (QuestContent qc in qp.contents_stringmeta) {
						if (qc.hasAttribute ("key") && qc.hasAttribute ("value")) {
							QuestMetaData newmeta = new QuestMetaData ();
							newmeta.key = qc.getAttribute ("key");
							newmeta.value = qc.getAttribute ("value");
							q.addMetaData (newmeta);

						}

					}

				}
			}

			if (q.PageList != null &&
			    q.PageList.Count > 0 &&
			    q.PageList [0].onStart != null &&
			    q.PageList [0].onStart.actions != null &&
			    q.PageList [0].onStart.actions.Count > 0) {

				foreach (QuestAction qameta in q.PageList[0].onStart.actions) {

					if (qameta.type == "SetVariable") {
						//	Debug.Log ("found setVar");
						if (qameta.hasAttribute ("var")) {

							QuestMetaData newmeta = new QuestMetaData ();
							newmeta.key = qameta.getAttribute ("var");
							if (qameta.value != null && qameta.value.string_value != null && qameta.value.string_value.Count > 0) {
								newmeta.value = qameta.value.string_value [0];
							} else {
								continue;
							}

							q.addMetaData (newmeta);
						}

					}

				}

			}

			return q;
		}

		public void addMetaData (QuestMetaData meta)
		{

			string key = meta.key;

			List<QuestMetaData> todelete = new List<QuestMetaData> ();

			if (metadata == null) {

				metadata = new List<QuestMetaData> ();

			} else {
				foreach (QuestMetaData qmd in metadata) {

					if (qmd.key == key) {
						todelete.Add (qmd);
					}

				}

				foreach (QuestMetaData qmd in todelete) {
					metadata.Remove (qmd);
				}

			}

			metadata.Add (meta);



			if (Configuration.instance.metaCategoryIsSearchable (meta.key)) {

				meta_Search_Combined += " " + meta.value;


			}
			if (Configuration.instance.getMetaCategory (meta.key) != null) {

				Configuration.instance.getMetaCategory (meta.key).addPossibleValues (meta.value);

			}


			meta_combined += ";" + meta.value;

			questdatabase questdb = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ();

			if (!questdb.allmetakeys.Contains (meta.key)) {

				questdb.allmetakeys.Add (meta.key);
			}

		}

		public void deserializeAttributes (bool redo)
		{

			attributes = new List<QuestAttribute> ();

			if (help_attributes != null) {
				foreach (XmlAttribute xmla in help_attributes) {

					if (xmla.Value.StartsWith ("http://") || xmla.Value.StartsWith ("https://")) {

						string[] splitted = xmla.Value.Split ('/');

						questdatabase questdb = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ();

						string filename = "files/" + splitted [splitted.Length - 1];

						//					int i = 0;
						//					while ( questdb.loadedfiles.Contains(filename) ) {
						//						i++;
						//						filename = "files/" + i + "_" + splitted[splitted.Length - 1];
						//						
						//					}
						//					
						//					questdb.loadedfiles.Add(filename);

						if (!Application.isWebPlayer) {

							if (!redo) {
								Debug.Log ("GETTING Image for: " + Id + " in " + filename);
								questdb.downloadAsset (xmla.Value, Application.persistentDataPath + "/quests/" + Id + "/" + filename);
							}
							if (splitted.Length > 3) {

								if (predeployed) {
									xmla.Value = questdb.PATH_2_PREDEPLOYED_QUESTS + "/" + Id + "/" + filename;

								} else {

									xmla.Value = Application.persistentDataPath + "/quests/" + Id + "/" + filename;

								}
								questdb.performSpriteConversion (xmla.Value);

							}
						}

					}	

					attributes.Add (new QuestAttribute (xmla.Name, xmla.Value));

				}
			}

			if (PageList != null) {
				foreach (Page qp in PageList) {
					qp.deserializeAttributes (Id, redo);
				}
			} else {

				Debug.Log ("no pages");
			}
			if (hotspotList != null) {

				foreach (QuestHotspot qh in hotspotList) {
					qh.deserializeAttributes (Id, redo);
				}
			}

		}

		public string getAttribute (string k)
		{

			foreach (QuestAttribute qa in attributes) {

				if (qa.key.Equals (k)) {
					return qa.value;
				}

			}

			return "";

		}

		public bool getBoolAttribute (string attName)
		{
			string attValue = getAttribute (attName);
			if (attValue.Trim ().Equals ("") || attValue.Equals ("0") || attValue.ToLower ().Equals ("false"))
				return false;
			else
				return true;
		}

		public string getMeta (string k)
		{
			if (metadata != null) {
				foreach (QuestMetaData qa in metadata) {

					if (qa.key.Equals (k)) {
						return qa.value;
					}

				}
			}

			return "";

		}

		public string getMetaComparer (string k)
		{
			if (metadata != null) {
				foreach (QuestMetaData qa in metadata) {

					if (qa.key.Equals (k)) {
						return qa.value;
					}

				}
			}

			return ((char)0xFF).ToString ();

		}

		public bool hasMeta (string k)
		{

			bool h = false;
			if (metadata != null) {
				foreach (QuestMetaData qa in metadata) {

					if (qa.key != null) {
						if (qa.key.Equals (k)) {
							h = true;
						}
					}
				}
			}

			return h;

		}

		public bool hasAttribute (string k)
		{

			bool h = false;
			foreach (QuestAttribute qa in attributes) {

				if (qa.key.Equals (k)) {
					h = true;
				}

			}

			return h;

		}

		public bool hasActionInChildren (string type1)
		{

			bool b = false;

			foreach (Page qp in PageList) {
				if (!b) {
					if (qp.hasActionInChildren (type1)) {
						b = true;
					}
				}
			}
			foreach (QuestHotspot qh in hotspotList) {
				if (!b) {
					if (qh.hasActionInChildren (type1)) {
						b = true;
					}
				}
			}

			return b;

		}

		#endregion

	}

}
