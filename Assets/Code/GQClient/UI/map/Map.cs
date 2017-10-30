using UnityEngine;

using System;

using UnitySlippyMap.Map;
using UnitySlippyMap.Markers;
using UnitySlippyMap.Layers;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.Converters.WellKnownText;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using GQ.Client.Util;
using GQ.Client.Err;
using GQ.Client.Conf;

namespace GQ.Client.UI
{

	public class Map : MonoBehaviour
	{
		private MapBehaviour map;

		public Texture	LocationTexture;
		public Texture	MarkerTexture;

		private float	guiXScale;
		private float	guiYScale;
		private Rect	guiRect;

		private bool isPerspectiveView = false;
		private float	perspectiveAngle = 30.0f;
		private float	destinationAngle = 0.0f;
		private float	currentAngle = 0.0f;
		private float	animationDuration = 0.5f;
		private float	animationStartTime = 0.0f;

		private List<LayerBehaviour> layers;
		private int currentLayerIndex = 0;

		private string utfGridJsonString = "";

		bool Toolbar (MapBehaviour map)
		{
			GUI.matrix = Matrix4x4.Scale (new Vector3 (guiXScale, guiXScale, 1.0f));

			GUILayout.BeginArea (guiRect);

			GUILayout.BeginHorizontal ();

			//GUILayout.Label("Zoom: " + map.CurrentZoom);

			bool pressed = false;
			if (GUILayout.RepeatButton ("+", GUILayout.ExpandHeight (true))) {
				map.Zoom (1.0f);
				pressed = true;
			}
			if (Event.current.type == EventType.Repaint) {
				Rect rect = GUILayoutUtility.GetLastRect ();
				if (rect.Contains (Event.current.mousePosition))
					pressed = true;
			}

			if (GUILayout.Button ("2D/3D", GUILayout.ExpandHeight (true))) {
				if (isPerspectiveView) {
					destinationAngle = -perspectiveAngle;
				} else {
					destinationAngle = perspectiveAngle;
				}

				animationStartTime = Time.time;

				isPerspectiveView = !isPerspectiveView;
			}
			if (Event.current.type == EventType.Repaint) {
				Rect rect = GUILayoutUtility.GetLastRect ();
				if (rect.Contains (Event.current.mousePosition))
					pressed = true;
			}

			if (GUILayout.Button ("Center", GUILayout.ExpandHeight (true))) {
				map.CenterOnLocation ();
			}
			if (Event.current.type == EventType.Repaint) {
				Rect rect = GUILayoutUtility.GetLastRect ();
				if (rect.Contains (Event.current.mousePosition))
					pressed = true;
			}

			string layerMessage = String.Empty;
			if (map.CurrentZoom > layers [currentLayerIndex].MaxZoom)
				layerMessage = "\nZoom out!";
			else if (map.CurrentZoom < layers [currentLayerIndex].MinZoom)
				layerMessage = "\nZoom in!";
			if (GUILayout.Button (((layers != null && currentLayerIndex < layers.Count) ? layers [currentLayerIndex].name + layerMessage : "Layer"), GUILayout.ExpandHeight (true))) {
				#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
				layers[currentLayerIndex].gameObject.SetActiveRecursively(false);
				#else
				layers [currentLayerIndex].gameObject.SetActive (false);
				#endif
				++currentLayerIndex;
				if (currentLayerIndex >= layers.Count)
					currentLayerIndex = 0;
				#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
				layers[currentLayerIndex].gameObject.SetActiveRecursively(true);
				#else
				layers [currentLayerIndex].gameObject.SetActive (true);
				#endif
				map.IsDirty = true;
			}

			if (GUILayout.RepeatButton ("-", GUILayout.ExpandHeight (true))) {
				map.Zoom (-1.0f);
				pressed = true;
			}
			if (Event.current.type == EventType.Repaint) {
				Rect rect = GUILayoutUtility.GetLastRect ();
				if (rect.Contains (Event.current.mousePosition))
					pressed = true;
			}

			GUILayout.EndHorizontal ();

			GUILayout.EndArea ();

			// Show any mbtiles utf string under the mouse position
			if (!string.IsNullOrEmpty (utfGridJsonString))
				GUILayout.Label (utfGridJsonString);

			return pressed;

		}

		private IEnumerator Start ()
		{
			// setup the gui scale according to the screen resolution
			guiXScale = (Screen.orientation == ScreenOrientation.Landscape ? Screen.width : Screen.height) / 480.0f;
			guiYScale = (Screen.orientation == ScreenOrientation.Landscape ? Screen.height : Screen.width) / 640.0f;
			// setup the gui area
			guiRect = new Rect (16.0f * guiXScale, 4.0f * guiXScale, Screen.width / guiXScale - 32.0f * guiXScale, 32.0f * guiYScale);

			// create the map singleton
			map = MapBehaviour.Instance;
			map.CurrentCamera = Camera.main;
			map.InputDelegate += UnitySlippyMap.Input.MapInput.BasicTouchAndKeyboard;
			map.CurrentZoom = 15.0f;
			// 9 rue Gentil, Lyon
			map.CenterWGS84 = new double[2] { 7.0090314, 50.9603868 };

			LocationSensor.Instance.OnLocationUpdate += 
				(object sender, LocationSensor.LocationEventArgs e) => {
				if (e.Kind == LocationSensor.LocationEventType.Update) {
					Debug.Log (
						string.Format ("--- LOC: {0}, {1}", e.Location.longitude, e.Location.latitude)
						.Yellow ()
					);
				}
				if (e.Kind == LocationSensor.LocationEventType.NotAvailable) {
					Debug.Log (("--- LOC: Unavailable. enabled: " + Input.location.isEnabledByUser).Yellow ());
				}
			};

			map.UsesLocation = true;
			map.InputsEnabled = true;
			map.ShowsGUIControls = false;

			map.GUIDelegate += Toolbar;

			layers = new List<LayerBehaviour> ();

			layers.Add (MapLayer);

//			// create a WMS tile layer
//			WMSTileLayerBehaviour wmsLayer = map.CreateLayer<WMSTileLayerBehaviour> ("WMS");
//			wmsLayer.BaseURL = "http://129.206.228.72/cached/osm?"; // http://www.osm-wms.de : seems to be of very limited use
//			wmsLayer.Layers = "osm_auto:all";
//			#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
//			wmsLayer.gameObject.SetActiveRecursively(false);
//			#else
//			wmsLayer.gameObject.SetActive (false);
//			#endif
//
//			layers.Add (wmsLayer);

//			// create a VirtualEarth tile layer
//			VirtualEarthTileLayerBehaviour virtualEarthLayer = map.CreateLayer<VirtualEarthTileLayerBehaviour> ("VirtualEarth");
//			// Note: this is the key UnitySlippyMap, DO NOT use it for any other purpose than testing
//			virtualEarthLayer.Key = "ArgkafZs0o_PGBuyg468RaapkeIQce996gkyCe8JN30MjY92zC_2hcgBU_rHVUwT";
//
//			#if UNITY_3_0 || UNITY_3_1 || UNITY_3_2 || UNITY_3_3 || UNITY_3_4 || UNITY_3_5 || UNITY_3_6 || UNITY_3_7 || UNITY_3_8 || UNITY_3_9
//			virtualEarthLayer.gameObject.SetActiveRecursively(false);
//			#else
//			virtualEarthLayer.gameObject.SetActive (false);
//			#endif
//
//			layers.Add (virtualEarthLayer);


			// create some test 2D markers
			GameObject go = TileBehaviour.CreateTileTemplate (TileBehaviour.AnchorPoint.BottomCenter).gameObject;
			go.GetComponent<Renderer> ().material.mainTexture = MarkerTexture;
			go.GetComponent<Renderer> ().material.renderQueue = 4001;
			go.transform.localScale = new Vector3 (0.70588235294118f, 1.0f, 1.0f);
			go.transform.localScale /= 7.0f;
			go.AddComponent<CameraFacingBillboard> ().Axis = Vector3.up;

			GameObject markerGO;
			markerGO = Instantiate (go) as GameObject;
			map.CreateMarker<MarkerBehaviour> ("test marker - Holger's Home", new double[2] { 7.0090314, 50.9603868 }, markerGO);

			markerGO = Instantiate (go) as GameObject;
			map.CreateMarker<MarkerBehaviour> ("test marker - der schöne Wiener Platz", new double[2] { 7.0063599, 50.9610017 }, markerGO);

			markerGO = Instantiate (go) as GameObject;
			map.CreateMarker<MarkerBehaviour> ("test marker - Mülheimer Bahnhof - falls man mal weg will ...", new double[2] { 7.0097933, 50.957596 }, markerGO);

			DestroyImmediate (go);

			// create the location marker
			go = TileBehaviour.CreateTileTemplate ().gameObject;
			go.GetComponent<Renderer> ().material.mainTexture = LocationTexture;
			go.GetComponent<Renderer> ().material.renderQueue = 4000;
			go.transform.localScale /= 27.0f;

			markerGO = Instantiate (go) as GameObject;
			map.SetLocationMarker<LocationMarkerBehaviour> (markerGO);

			DestroyImmediate (go);

			yield break;
		}

		private LayerBehaviour MapLayer {
			get {
				LayerBehaviour mapLayer;

				switch (ConfigurationManager.Current.mapProvider) {
				case MapProvider.OpenStreetMap:
					mapLayer = OsmMapLayer;
					break;
				case MapProvider.MapBox:
					mapLayer = MapBoxLayer;
					break;
				default:
					Log.SignalErrorToDeveloper (
						"Unknown map provider defined in configuration: {0}. We use OpenStreetMap instead.", 
						ConfigurationManager.Current.mapProvider
					);
					mapLayer = OsmMapLayer;
					break;
				}

				return mapLayer;
			}
		}

		private LayerBehaviour OsmMapLayer {
			get {
				OSMTileLayer osmLayer = map.CreateLayer<OSMTileLayer> (ConfigurationManager.Current.mapProvider.ToString ());
				osmLayer.BaseURL = ConfigurationManager.Current.mapBaseUrl + "/";
				return osmLayer;
			}
		}

		private LayerBehaviour MapBoxLayer {
			get {
				OSMTileLayer mapBoxLayer = map.CreateLayer<OSMTileLayer> (ConfigurationManager.Current.mapProvider.ToString ());
				mapBoxLayer.BaseURL = "http://api.tiles.mapbox.com/v4/" + ConfigurationManager.Current.mapID + "/";
				mapBoxLayer.TileImageExtension = "@2x.png?access_token=" + ConfigurationManager.Current.mapKey;
				return mapBoxLayer;
			}
		}

		void OnApplicationQuit ()
		{
			map = null;
		}

		void Update ()
		{
			if (destinationAngle != 0.0f) {
				Vector3 cameraLeft = Quaternion.AngleAxis (-90.0f, Camera.main.transform.up) * Camera.main.transform.forward;
				if ((Time.time - animationStartTime) < animationDuration) {
					float angle = Mathf.LerpAngle (0.0f, destinationAngle, (Time.time - animationStartTime) / animationDuration);
					Camera.main.transform.RotateAround (Vector3.zero, cameraLeft, angle - currentAngle);
					currentAngle = angle;
				} else {
					Camera.main.transform.RotateAround (Vector3.zero, cameraLeft, destinationAngle - currentAngle);
					destinationAngle = 0.0f;
					currentAngle = 0.0f;
					map.IsDirty = true;
				}

				map.HasMoved = true;
			}

			// if (Input.GetMouseButtonDown(0))
			foreach (LayerBehaviour _lb in layers)
				if (_lb.GetType () == typeof(MBTilesLayerBehaviour))
					utfGridJsonString = ((MBTilesLayerBehaviour)_lb).UtfGridJsonString ();

		}

		#if DEBUG_PROFILE
		void LateUpdate()
		{
		Debug.Log("PROFILE:\n" + UnitySlippyMap.Profiler.Dump());
		UnitySlippyMap.Profiler.Reset();
		}
		#endif
	}
}