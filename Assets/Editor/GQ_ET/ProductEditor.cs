using UnityEditor;
using System.Collections;
using UnityEngine;
using System.IO;
using System.Linq;
using LitJson;
using GQ.Conf;
using System;

namespace GQ_ET
{
	public class ProductEditor : EditorWindow
	{
		static private int selectedProductIndex;
		static private string[] productIDs;
		static private bool initialized = false;

		[MenuItem ("Window/GQ Product Editor")]
		public static void  ShowWindow ()
		{
			EditorWindow.GetWindow (typeof(ProductEditor));
		}

		void initialize ()
		{
			this.titleContent = new GUIContent ("GQ Product");
			productIDs = retrieveProductNames ();
			if (productIDs.Length < 1) {
				Debug.LogWarning ("No product definitions found!");
				// TODO display error message in editor
			}
			if (EditorPrefs.HasKey ("ProductIndex")) {
				selectedProductIndex = EditorPrefs.GetInt ("ProductIndex");
				GUI.enabled = false;
				ProductConfigManager.load (productIDs [selectedProductIndex]);
				GUI.enabled = true;
			}
			if (!Directory.Exists (ProductConfigManager.PRODUCTS_DIR)) {    
				Directory.CreateDirectory (ProductConfigManager.PRODUCTS_DIR);
				Debug.LogWarning ("No product directory found. Created an empty one. No products defined!");
			}
			initialized = true;
		}

		void OnGUI ()
		{
			if (!initialized)
				initialize ();

			createGUIProductSelection ();

			EditorGUILayout.Space ();

			createGUIShowDetails ();

			EditorGUILayout.Space ();

			createGUIEditSpec ();
		}

		void createGUIProductSelection ()
		{
			GUILayout.Label ("Select Product to Build", EditorStyles.boldLabel);
			changeProduct (EditorGUILayout.Popup ("Product ID", selectedProductIndex, productIDs));
		}

		bool allowChanges = false;
		
		void createGUIShowDetails ()
		{
			GUILayout.Label ("Details of Selected Product", EditorStyles.boldLabel);

			GUI.enabled = allowChanges;

			if (!initialized) {
				// TODO if not initialized show warnings
				return;
			} 
			GUI.enabled = allowChanges;

			ProductConfigManager.current.name = 
				EditorGUILayout.TextField (
					"Name", 
					ProductConfigManager.current.name, 
					GUILayout.Height (EditorGUIUtility.singleLineHeight));

			ProductConfigManager.appIcon = 
				(Texture2D)EditorGUILayout.ObjectField (
					"App Icon", 
					ProductConfigManager.appIcon,
			        typeof(Texture),
					false);

			ProductConfigManager.current.portal = 
				EditorGUILayout.IntField (
					"Portal", 
					ProductConfigManager.current.portal, 
					GUILayout.Height (EditorGUIUtility.singleLineHeight));
			// TODO check and offer selection from server

			ProductConfigManager.current.autoStartQuestID = 
				EditorGUILayout.IntField (
					"Autostart Quest ID", 
					ProductConfigManager.current.autoStartQuestID, 
					GUILayout.Height (EditorGUIUtility.singleLineHeight));
			// TODO check at server and offer browser to select driectly from server

			if (ProductConfigManager.current.autoStartQuestID != 0) {
				ProductConfigManager.current.autostartIsPredeployed =
				EditorGUILayout.Toggle ("Autostart Predeployed?", ProductConfigManager.current.autostartIsPredeployed);
			} else {
				ProductConfigManager.current.autostartIsPredeployed = false;
			}
			
			ProductConfigManager.current.downloadTimeOutSeconds = 
				EditorGUILayout.IntField (
					"Download Timeout (s)", 
					ProductConfigManager.current.downloadTimeOutSeconds);
			// TODO limit to a value bigger than something (5s?)

			ProductConfigManager.current.nameForQuest = 
				EditorGUILayout.TextField (
					"Name for 'Quest'", 
					ProductConfigManager.current.nameForQuest, 
					GUILayout.Height (EditorGUIUtility.singleLineHeight));
			if (ProductConfigManager.current.nameForQuest == null || ProductConfigManager.current.nameForQuest.Equals ("")) {
				ProductConfigManager.current.nameForQuest = "Quest";
			}
			
			QuestVisualizationMethod mIn;
			if (ProductConfigManager.current.questVisualization == null) {
				mIn = QuestVisualizationMethod.list;
			} else {
				mIn = (QuestVisualizationMethod)Enum.Parse (typeof(QuestVisualizationMethod), ProductConfigManager.current.questVisualization.ToLower ());
			}
			string questVisLabel = "Quest Visualization";
			if (ProductConfigManager.current.questVisualizationChangeable) {
				questVisLabel = "Initial " + questVisLabel;
			}
			QuestVisualizationMethod m =
				(QuestVisualizationMethod)EditorGUILayout.EnumPopup (questVisLabel, mIn);
			if (m != null) {
				ProductConfigManager.current.questVisualization = m.ToString ().ToLower ();
			}

			ProductConfigManager.current.questVisualizationChangeable =
				EditorGUILayout.Toggle ("Visualization Changeable?", ProductConfigManager.current.questVisualizationChangeable);
			
			ProductConfigManager.current.showCloudQuestsImmediately =
				EditorGUILayout.Toggle ("Load cloud quests asap?", ProductConfigManager.current.showCloudQuestsImmediately);

			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PrefixLabel ("Imprint");
			ProductConfigManager.current.imprint = 
				EditorGUILayout.TextArea (
					ProductConfigManager.current.imprint);
			EditorGUILayout.EndHorizontal ();

			// TODO splash screen
			ProductConfigManager.splashScreen = 
				(Texture2D)EditorGUILayout.ObjectField (
					"Splash Screen", 
					ProductConfigManager.splashScreen,
					typeof(Texture),
					false);

			ProductConfigManager.current.colorProfile = 
				EditorGUILayout.TextField (
					"Color Profile", 
					ProductConfigManager.current.colorProfile, 
					GUILayout.Height (EditorGUIUtility.singleLineHeight));
			// TODO change to better representation of Color Profile
			
			ProductConfigManager.current.showTextInLoadingLogo =
				EditorGUILayout.Toggle ("Show Loading Text?", ProductConfigManager.current.showTextInLoadingLogo);
			
			// TODO Animation Loading Logo
			
			ProductConfigManager.current.showNetConnectionWarning =
				EditorGUILayout.Toggle ("Show Connection Warning?", ProductConfigManager.current.showNetConnectionWarning);

			ProductConfigManager.topLogo = 
				(Sprite)EditorGUILayout.ObjectField (
					"Top Bar Logo", 
					ProductConfigManager.topLogo,
					typeof(Sprite),
					false);
			// TODO resize visualization in editor to correct 

			ProductConfigManager.current.mapboxMapID = 
				EditorGUILayout.TextField (
					"Mapbox Map ID", 
					ProductConfigManager.current.mapboxMapID, 
					GUILayout.Height (EditorGUIUtility.singleLineHeight));
			
			ProductConfigManager.current.mapboxKey = 
				EditorGUILayout.TextField (
					"Mapbox User Key", 
					ProductConfigManager.current.mapboxKey, 
					GUILayout.Height (EditorGUIUtility.singleLineHeight));
			// TODO make generic representation for map types (google, OSM, Mapbox)
			
			// TODO default marker
			ProductConfigManager.defaultMarker = 
				(Sprite)EditorGUILayout.ObjectField (
					"Default Marker", 
					ProductConfigManager.defaultMarker,
					typeof(Sprite),
					false);
			// TODO resize visualization in editor to correct 

			// TODO marker categories ...

			GUI.enabled = true;
			return;
		}

		string editedProdID = "not set";

		enum Save
		{
			Overwrite,
			AsNew
		}

		void createGUIEditSpec ()
		{
			// TODO rework the complete editing UI in the editor to buttons
			GUILayout.Label ("Edit Product Configuration", EditorStyles.boldLabel);

			bool oldAllowChanges = allowChanges;
			allowChanges = EditorGUILayout.Toggle ("Allow To Edit ...", allowChanges);
			if (!oldAllowChanges && allowChanges) {
				// siwtching allowChanges ON:
				editedProdID = productIDs [selectedProductIndex];
			}
			if (oldAllowChanges && !allowChanges) {
			}

			if (allowChanges) {
				editedProdID = EditorGUILayout.TextField ("ID of Edited Product", editedProdID);

				Save saveType;
				string[] buttonText = { "Overwrite Existing Product", "Store New Product" };
				string[] dialogTitle = {
					"Overwrite Existing Product",
					"Save as New Product"
				};
				string[] dialogMessagePrefix = {
					"You current settings for product '",
					"This will create a new product specification named '"
				};
				string[] dialogMessagePostfix = { "' will be lost.", "'." };
				string[] okText = { "Overwrite", "Create" };

				if (productIDs.Contains (editedProdID)) {
					saveType = Save.Overwrite;
					// TODO add warning icon 
				} else {
					saveType = Save.AsNew;
				}

				if (GUILayout.Button (buttonText [(int)saveType])) {
					bool okPressed = EditorUtility.DisplayDialog (
						dialogTitle [(int)saveType], 
						dialogMessagePrefix [(int)saveType] + editedProdID + 
						dialogMessagePostfix [(int)saveType], 
					    okText [(int)saveType], 
					    "Cancel");
					if (okPressed) {
						performSaveConfig (editedProdID);
					}
				}
			}

		}

		void performSaveConfig (string productID)
		{
			ProductConfigManager.current.id = productID;
			ProductConfigManager.save (productID);
		}

		void OnProjectChange ()
		{
			initialize ();
		}

		void changeProduct (int index)
		{
			if (index.Equals (selectedProductIndex))
				return;

			try {
				GUI.enabled = false;
				ProductConfigManager.load (productIDs [index]);
				GUI.enabled = true;
				selectedProductIndex = index;
				EditorPrefs.SetInt ("ProductIndex", index);
				allowChanges = false;
			} catch (System.IndexOutOfRangeException e) {
				Debug.LogWarning (e.Message);
				initialized = false;
			}
		}

		static string[] retrieveProductNames ()
		{
			return Directory.GetDirectories (ProductConfigManager.PRODUCTS_DIR).Select (d => new DirectoryInfo (d).Name).ToArray ();
		}
	}
}

