﻿using UnityEngine;
using UnityEngine.UI;

using System;

using UnitySlippyMap;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.Converters.WellKnownText;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using GQ.Geo;
using UnityEngine.EventSystems;
using System.Xml.Serialization;
using System.Text;
using System.Globalization;

/// <summary>
/// Caution: We use the order (LATITUDE, LONGITUDE) throughout our implementation here! 
/// 
/// </summary>
public class page_map : MonoBehaviour
{




	public RectTransform navigationMenu;
	public RectTransform numberInputPanel;

	public Map map;
	public questdatabase questdb;
	public Quest quest;
	public QuestPage mappage;
	public actions questactions;
	Locationcontrol locationController;
	public GPSPosition gpsdata;
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
	private List<Layer> layers;
	private int currentLayerIndex = 0;
	public LocationMarker location;
	private bool zoomin = false;
	private bool zoomout = false;
	public bool showquests = false;
	private string pre;
	public checkmarkcolor positionCheckmark;
	public Toggle positionToggle;
	public bool onStartInvoked = false;
	public Route currentroute;
	private double[] baseDefPos = {
		7d,
		51d
	};

	double[] getCurrentPosition ()
	{
		return new double[] { 
			gpsdata.CoordinatesWGS84 [0],
			gpsdata.CoordinatesWGS84 [1]
		};
	}

	void centerMap (double[] position)
	{
		map.CenterWGS84 = position;
	}

	private void Start ()
	{
		if (GameObject.Find ("QuestDatabase") == null) {
			
			Application.LoadLevel (0);
			return;
			
		} 





		questdb = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ();

		gpsdata = questdb.GetComponent<GPSPosition> ();


		if (questdb.currentquest != null && questdb.currentquest.id != 0) {

			quest = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ().currentquest;
			mappage = GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ().currentquest.currentpage;
			questactions = GameObject.Find ("QuestDatabase").GetComponent<actions> ();
		}

		bool showMap = true;
		if (mappage.type.Equals ("Navigation")) {
			showMap = false;

			if (mappage.getAttribute ("map") == "true") {

				showMap = true;

			}

			numberInputPanel.gameObject.SetActive (true);
			navigationMenu.gameObject.SetActive (true);
		} else {
			numberInputPanel.gameObject.SetActive (false);
			navigationMenu.gameObject.SetActive (false);


		}


		if (showMap) {
		
			// TODO: extract prefix determination in globally accessable method:
			pre = "file://";

			if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) {
			
				pre = "file:";
			}

			if (questdb.currentquest != null) {
				if (Application.platform == RuntimePlatform.Android && questdb.currentquest.predeployed) {
			
					pre = "";
				}
			}

			// setup the gui scale according to the screen resolution
			guiXScale = (Screen.orientation == ScreenOrientation.Landscape ? Screen.width : Screen.height) / 480.0f;
			guiYScale = (Screen.orientation == ScreenOrientation.Landscape ? Screen.height : Screen.width) / 640.0f;
			// setup the gui area
			guiRect = new Rect (16.0f * guiXScale, 4.0f * guiXScale, Screen.width / guiXScale - 32.0f * guiXScale, 32.0f * guiYScale);
		
			// create the map singleton
			map = Map.Instance;
			map.CurrentCamera = Camera.main;
			map.InputDelegate += UnitySlippyMap.Input.MapInput.BasicTouchAndKeyboard;
	

			map.gameObject.AddComponent<mapdisplaytoggle> ();

			initMap ();
		
			layers = new List<Layer> ();
		
			// create an OSM tile layer
			OSMTileLayer osmLayer = map.CreateLayer<OSMTileLayer> ("OSM");
			//osmLayer.BaseURL = "http://a.tile.openstreetmap.org/";

			if (!Configuration.instance.useMapOffline) {

				osmLayer.BaseURL = "http://api.tiles.mapbox.com/v4/" + Configuration.instance.mapboxMapID + "/";
				osmLayer.TileImageExtension = "@2x.png?access_token=" + Configuration.instance.mapboxKey;


			} else {

				osmLayer.BaseURL = pre + Application.streamingAssetsPath + "/mapTiles/";
				osmLayer.TileImageExtension = ".jpg";


			}
			layers.Add (osmLayer);


			updateMapMarker ();

		}
	}

	/// <summary>
	/// Initializes the map position and zoom at Start().
	/// </summary>
	void initMap ()
	{
		// set position to default:
		gpsdata.CoordinatesWGS84 = new double[] {
			Configuration.instance.defaultLatitude,
			Configuration.instance.defaultLongitude
		};
		
		
		if (Application.isWebPlayer || Application.isEditor) {
			initPositionSimulation ();
		} else {
			initPositionBySensor ();
		}

		map.UseLocation = true;
		map.UseOrientation = true; // TODO: should be false, shouldn't it?

		if (!Configuration.instance.useMapOffline) {
			map.MaxZoom = 20.0f;
		} else {
			map.MaxZoom = 18.0f;
		}
		map.MinZoom = 13.0f;
		
		map.InputsEnabled = true;
		map.ShowGUIControls = true;
		
		// Initialize zoom level in map
		map.CurrentZoom = Configuration.instance.storedMapZoom;
		
		// Initialize centering mode:
		if (positionToggle.isOn != Configuration.instance.storedMapPositionModeIsCentering) {
			positionToggle.isOn = Configuration.instance.storedMapPositionModeIsCentering;
			questdb.fixedposition = positionToggle.isOn;
		}

		// Initialize map position:
		centerMap (getInitialMapCenter ());
	}

	private double[] getInitialMapCenter ()
	{
		// if in manual positioning mode,
		if (!Configuration.instance.storedMapPositionModeIsCentering) {
			// if old position is stored, use it:
			if (Configuration.instance.storedMapCenter != null) {
				return Configuration.instance.storedMapCenter;
			}
			// otherwise, use the center of the quest or defaults if no hotspots exist:
			else {
				return calculateCenterOfHotspots ();
			}
		}
		// if in centering mode:
		else {
			// if no gps available
			if (gpsdata == null || gpsdata.CoordinatesWGS84.Length <= 1) {
				// if old stored position available use it:
				if (Configuration.instance.storedMapCenter != null) {
					return Configuration.instance.storedMapCenter;
				}
				// if not, use defaults:
				else {
					return getPositionDefaults ();
				}
			}
			// if positioning available
			else {
				return gpsdata.CoordinatesWGS84;
			}
		}
	}

	void initPositionSimulation ()
	{
		// create the location marker
		var posi = Tile.CreateTileTemplate ().gameObject;
		posi.GetComponent<Renderer> ().material.mainTexture = LocationTexture;
		posi.GetComponent<Renderer> ().material.renderQueue = 4000;
		posi.transform.localScale /= 8.0f;
		GameObject markerPosi = Instantiate (posi) as GameObject;

		// initialize simulated position:
		double[] simulatedPosition;
		if (Configuration.instance.storedSimulatedPosition == null) { 
			// either outside of all hotspots:
			simulatedPosition = calculatePosOutsideOfAllHotspots ();
		} else { 
			// or at the position where you had been before you left the map:
			simulatedPosition = Configuration.instance.storedSimulatedPosition;
		}


		location = map.SetLocationMarker<LocationMarker> (markerPosi, simulatedPosition [0], simulatedPosition [1]);
		location.OrientationMarker = location.transform;
		location.GetComponentInChildren<MeshRenderer> ().material.color = Color.cyan;
		location.gameObject.AddComponent<Locationcontrol> ();
		Locationcontrol lc = location.GetComponent<Locationcontrol> ();
		lc.mapcontroller = this;
		lc.map = map;
		lc.location = location;

		// initialize simulated position markers angle:
		if (Configuration.instance._storedLocationMarkerAngles != default(Vector3)) {
			lc.transform.eulerAngles = Configuration.instance._storedLocationMarkerAngles;
		}

		locationController = lc;
		DestroyImmediate (posi);
		gpsdata.CoordinatesWGS84 [0] = simulatedPosition [0];
		gpsdata.CoordinatesWGS84 [1] = simulatedPosition [1];
	}

	void initPositionBySensor ()
	{
		// create the location marker
		var posi = Tile.CreateTileTemplate ().gameObject;
		posi.GetComponent<Renderer> ().material.mainTexture = LocationTexture;
		posi.GetComponent<Renderer> ().material.renderQueue = 4000;
		posi.transform.localScale /= 8.0f;
		GameObject markerPosi = Instantiate (posi) as GameObject;
		
		// initialize simulated position:
		double[] simulatedPosition;
		if (Configuration.instance.storedSimulatedPosition == null) { 
			// either outside of all hotspots:
			simulatedPosition = calculatePosOutsideOfAllHotspots ();
		} else { 
			// or at the position where you had been before you left the map:
			simulatedPosition = Configuration.instance.storedSimulatedPosition;
		}
		
		
		location = map.SetLocationMarker<LocationMarker> (markerPosi, gpsdata.CoordinatesWGS84 [0], gpsdata.CoordinatesWGS84 [1]);
		location.OrientationMarker = location.transform;
		location.GetComponentInChildren<MeshRenderer> ().material.color = Color.blue;
		location.gameObject.AddComponent<Locationcontrol> ();
		Locationcontrol lc = location.GetComponent<Locationcontrol> ();
		lc.mapcontroller = this;
		lc.map = map;
		lc.location = location;
		
		// initialize simulated position markers angle:
		if (Configuration.instance._storedLocationMarkerAngles != default(Vector3)) {
			lc.transform.eulerAngles = Configuration.instance._storedLocationMarkerAngles;
		}
		
		locationController = lc;
		DestroyImmediate (posi);
		gpsdata.CoordinatesWGS84 [0] = simulatedPosition [0];
		gpsdata.CoordinatesWGS84 [1] = simulatedPosition [1];
	}

	/// <summary>
	/// Returns the position of the center of all hotspots or the default position if no hotspots exist.
	/// </summary>
	/// <returns>The center of hotspots.</returns>
	double[] calculateCenterOfHotspots ()
	{
		if (questdb == null) {
			return getPositionDefaults ();
		}
		GeoPosition centerOfQuest = questdb.getQuestCenter ();
		if (centerOfQuest.Lat != 0f || centerOfQuest.Long != 0f) {
			return new double[] {
				Convert.ToDouble (centerOfQuest.Lat),
				Convert.ToDouble (centerOfQuest.Long)
			};
		} else {
			return getPositionDefaults ();
		}
	}

	double[] calculatePosOutsideOfAllHotspots ()
	{
		if (questdb == null) {
			return getPositionDefaults ();
		}
		List<QuestRuntimeHotspot> activeHotspots = questdb.getActiveHotspots ();

		if (activeHotspots.Count == 0) {
			// if we have no hitspots, we use some default position:
			return getPositionDefaults ();
		}

		// hence we have at least one hotspot:

		QuestRuntimeHotspot mostWesternHotspot = activeHotspots [0];

		for (int i = 1; i < activeHotspots.Count; i++) {
			if (activeHotspots [i].lon < mostWesternHotspot.lon) {
				mostWesternHotspot = activeHotspots [i];
			}
		}

		return getPositionWestOfHotspot (mostWesternHotspot);
	}

	double[] getPositionDefaults ()
	{
		if (Configuration.instance.useDefaultPositionValuesAtStart) {
			return new double[] { 
				Configuration.instance.defaultLongitude,
				Configuration.instance.defaultLatitude 
			};
		} else {
			return baseDefPos;
		}
	}

	private double[] getPositionWestOfHotspot (QuestRuntimeHotspot hotspot)
	{
		// calculate circumference of the latitude circle at the hotpost position in meters:
		double earthRadiusMeter = 6371000d;
		double latitudeCircumference = 2d * 3.1415d * Math.Cos (hotspot.lat) * earthRadiusMeter;
		// divide it by 360 so we get the length of one longitunal degree on the latitude of the hotspot:
		double lengthOfOneLongitudeDegree = latitudeCircumference / 360d;
		// we need to go the defined hotspot radius plus some 50 meters further west:
		double distanceToGoWestFromHotspotCenter = double.Parse (hotspot.hotspot.getAttribute ("radius")) + 50d;
		// get the delta longitude angle that we need to go further west:
		double deltaLongitude = distanceToGoWestFromHotspotCenter / lengthOfOneLongitudeDegree;
		// subtract the delat longitude from the hotspot longitude:
		double westOfHotspotLongitude = hotspot.lon - deltaLongitude;
		// if the value is below -180 degrees, we switch to east of greenwich, i.e. are just below +180:
		if (westOfHotspotLongitude < -180d) {
			westOfHotspotLongitude += 360d;
		}
		// return the new position:
		double[] pointWestOfHostpot = new double[] {
			hotspot.lat,
			westOfHotspotLongitude
		};
		return pointWestOfHostpot;
	}

	public void updateMapMarker ()
	{

		// DELETE ALL MARKERS


		List<Marker> allmarker = new List<Marker> ();
		allmarker.AddRange (map.Markers);

		foreach (Marker m in allmarker) {

			Destroy (m.gameObject);
			map.Markers.Remove (m);

		}


		foreach (QuestRuntimeHotspot qrh in questdb.hotspots) {

			bool show = true;
			
			if (qrh.category != null && qrh.category != "") {

				foreach (MarkerCategorySprite mcs in Configuration.instance.categoryMarker) {

					if (mcs.category == qrh.category) {

						if (!mcs.showOnMap) {

							show = false;
						}

					}

				}

			}

			if (show) {
				WWW www = null;
			
			
			
				if (qrh.hotspot.getAttribute ("img").StartsWith ("@_")) {
				
				
					www = new WWW (pre + "" + questactions.getVariable (qrh.hotspot.getAttribute ("img")).string_value [0]);
				
				
				} else if (qrh.hotspot.getAttribute ("img") != "") {
				
				
				
				
					string url = qrh.hotspot.getAttribute ("img");
					if (!url.StartsWith ("http:") && !url.StartsWith ("https:")) {
						url = pre + "" + qrh.hotspot.getAttribute ("img");
					}
				
					//				Debug.Log(url);
				
				
					if (url.StartsWith ("http:") || url.StartsWith ("https:")) {
						//Debug.Log("webimage");
					
						www = new WWW (url);
						StartCoroutine (createMarkerAfterImageLoaded (www, qrh));
					
					
					} else if (File.Exists (qrh.hotspot.getAttribute ("img"))) {
						www = new WWW (url);
						StartCoroutine (createMarkerAfterImageLoaded (www, qrh));
					} else if (questdb.currentquest.predeployed) {
						www = new WWW (url);
						StartCoroutine (createMarkerAfterImageLoaded (www, qrh));
					}
				
				} else {
				
				
				
				
					createMarker (qrh, qrh.getMarkerImage ().texture);
				
				
				}
			
			}
			
		}





	}

	public void unDrawCurrentRoute ()
	{

		Debug.Log ("unloading Route");
		foreach (RoutePoint rp in currentroute.points) {

			if (rp.marker != null) {
				map.RemoveMarker (rp.marker);
				DestroyImmediate (rp.waypoint);
				rp.marker = null;
				rp.waypoint = null;
			}

		}
			
			
	}

	public void drawCurrentRoute ()
	{


//		Debug.Log (questdb.currentquest.currentpage.type);
		if (questdb.currentquest.currentpage.type == "MapOSM") {


			foreach (RoutePoint rp in currentroute.points) {



				//string lon = rp.lon;
				//string lat = rp.lat;

				float lat = float.Parse (rp.lat, CultureInfo.InvariantCulture);
				float lon = float.Parse (rp.lon, CultureInfo.InvariantCulture);


		
				GameObject waypoint = new GameObject ();
				
				Marker m1 = map.CreateMarker<Marker> ("", new double[2] {
					lat,
					lon
				}, waypoint);
				rp.marker = m1;
				rp.waypoint = waypoint;
			}
			GameObject.Find ("RouteRender").GetComponent<routerender> ().started = false;
		}
	}

	/// <summary>
	/// Gets called by event trigger of toggle in map page in case of PointerClick. 
	/// When this method is called, the positionToggle.isOn state variable of the toggle is already set newly.
	/// </summary>
	/// <param name="obsolete">If set to <c>true</c> obsolete.</param>
	public void positionToggleClicked (bool obsolete /* TODO remove */)
	{
		bool enteringFixedMode = positionToggle.isOn;
		bool enteringManualMode = !enteringFixedMode;

		if (questdb != null) {
			if (enteringManualMode) {
				questdb.fixedposition = false;
//				Debug.Log("TOGGLE: Entering MANUAL mode by CLICK @" + Time.frameCount);
				centerMap (questdb.getQuestCenterPosition ());
				return;
			}
			if (enteringFixedMode) {
//				Debug.Log("TOGGLE: Entering FIXED mode @" + Time.frameCount);
				questdb.fixedposition = true;
				centerMap (gpsdata.CoordinatesWGS84);
				return;
			}
		} 
		Debug.LogError ("Unexpected Behaviour in page_map.togglePositionClicked()");

	}

	public void enterPositionModeManual ()
	{
		bool a = positionToggle.isOn;
//		Debug.Log("TOGGLE: positionToggle changed in Update(). old isON State = " + a + " new will be: false. @" + Time.frameCount);
		questdb.fixedposition = false;
		positionToggle.isOn = false;
	}

	void createMarker (QuestRuntimeHotspot qrh, Texture image)
	{

		if (qrh.lon != 0f || qrh.lat != 0f) {
			// Prefab
			GameObject go = Tile.CreateTileTemplate (Tile.AnchorPoint.BottomCenter).gameObject;
			go.GetComponent<Renderer> ().material.mainTexture = image;
			go.GetComponent<Renderer> ().material.renderQueue = 4001;
		
			int height = go.GetComponent<Renderer> ().material.mainTexture.height;
			int width = go.GetComponent<Renderer> ().material.mainTexture.width;

			float scale = Configuration.instance.markerScale;

			if (Application.isMobilePlatform) {
				scale *= 2.0f;
			}
		
			if (height > width) {
				//Debug.Log(width+"/"+height+"="+width/height);
				go.transform.localScale = new Vector3 ((scale * ((float)width) / ((float)height)), scale, scale);
			
			} else {
			
				go.transform.localScale = new Vector3 (scale, (scale * ((float)width) / ((float)height)), scale);
			}
		
			go.transform.localScale /= 512f;
			go.transform.localScale *= width;
		
			int screenWidth;
			#if UNITY_WEBPLAYER || UNITY_EDITOR 
			screenWidth = 1080;
			#else
			screenWidth = Screen.width;
			#endif
		
			go.transform.localScale *= screenWidth / 600f;
		
			go.AddComponent<onTapMarker> ();
			go.GetComponent<onTapMarker> ().hotspot = qrh;
		
			// TODO do not draw radius circles on real devices:
#if (UNITY_WEBPLAYER || UNITY_EDITOR) 

			if (questdb.currentquest != null && questdb.currentquest.id != 0) {
				go.AddComponent<circletests> ();
		
				if (qrh.hotspot.hasAttribute ("radius")) {
					go.GetComponent<circletests> ().radius = int.Parse (qrh.hotspot.getAttribute ("radius"));
				}
			}

#endif

			go.GetComponent<BoxCollider> ().center = new Vector3 (0f, 0f, 0.5f);
			go.GetComponent<BoxCollider> ().size = new Vector3 (1f, 0.1f, 1f);
			go.AddComponent<CameraFacingBillboard> ().Axis = Vector3.up;
		
			// Instantiate
			GameObject markerGO;
			markerGO = Instantiate (go) as GameObject;
			qrh.renderer = markerGO.GetComponent<MeshRenderer> ();
		
			// CreateMarker(Name,longlat,prefab)
			Marker m = map.CreateMarker<Marker> (qrh.hotspot.getAttribute ("name"), new double[2] {
				qrh.lat,
				qrh.lon
			}, markerGO);
		
			// Destroy Prefab
			DestroyImmediate (go);
		
			if (!qrh.visible) {
				qrh.renderer.enabled = false;
			}
		}
	}

	IEnumerator createMarkerAfterImageLoaded (WWW www, QuestRuntimeHotspot qrh)
	{
		yield return www;
		
		if (www.error == null) {
			createMarker (qrh, www.texture);
		} else {
			Debug.Log (www.error);
		}
	}

	public void initiateZoomIn ()
	{
		zoomin = true;
	}

	public void initiateZoomOut ()
	{
		zoomout = true;
	}

	public void endZoomIn ()
	{
		zoomin = false;
	}

	public void endZoomOut ()
	{
		zoomout = false;
	}

	void OnApplicationQuit ()
	{
		map = null;
	}

	void Update ()
	{
		if (!onStartInvoked && mappage != null) {

			if (mappage.onStart != null) {
				mappage.onStart.Invoke ();
				onStartInvoked = true;
			}

			if (currentroute != null && currentroute.points != null & currentroute.points.Count > 1 && currentroute.points [0].waypoint == null) {
				drawCurrentRoute ();
			}
		}

		if (Input.GetMouseButtonDown (0) && EventSystem.current.currentSelectedGameObject == null) {
			enterPositionModeManual ();
		}

		if (questdb != null && questdb.fixedposition && gpsdata != null && gpsdata.CoordinatesWGS84.Length > 1) {
			centerMap (gpsdata.CoordinatesWGS84);
		} 

		if (zoomin) {
			map.Zoom (0.8f);
		} else if (zoomout) {
			map.Zoom (-0.8f);
		}

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
	}
	
	#if DEBUG_PROFILE
	void LateUpdate()
	{
		Debug.Log("PROFILE:\n" + UnitySlippyMap.Profiler.Dump());
		UnitySlippyMap.Profiler.Reset();
	}
	#endif

	public Route LoadFromText (WWW routewww)
	{
		
		string xmlcontent = routewww.text;
		
		if (xmlcontent != null && xmlcontent.StartsWith ("<error>")) {
			GameObject.Find ("QuestDatabase").GetComponent<questdatabase> ().showmessage (xmlcontent);
			return null;
		}
		
		if (xmlcontent == null) {
			xmlcontent = " ";
		}

		Encoding enc = System.Text.Encoding.UTF8;
		TextReader txr = new StringReader (xmlcontent);
		XmlSerializer serializer = new XmlSerializer (typeof(Route));
		return serializer.Deserialize (txr) as Route; 
	}

	void OnDestroy ()
	{
		// save current zoom and position:
		if (map != null) {
			Configuration.instance.storedMapCenter = map.CenterWGS84;
			Configuration.instance.storedSimulatedPosition = location.CoordinatesWGS84;
			try {
				Configuration.instance._storedLocationMarkerAngles = locationController.transform.eulerAngles;
			} catch (NullReferenceException exc) {
				Configuration.instance._storedLocationMarkerAngles = default(Vector3);
			}
			Configuration.instance.storedMapZoom = map.CurrentZoom;
			Configuration.instance.storedMapPositionModeIsCentering = positionToggle.isOn;
		}
	}
	
	
	
}


[System.Serializable]
public class Route
{
	
	
	public string version;
	public List<RoutePoint> points;

	public void addPoint (string a, string b)
	{

		RoutePoint rp = new RoutePoint ();
		rp.lat = a;
		rp.lon = b;


		if (points == null) {

			points = new List<RoutePoint> ();
		}
		points.Add (rp);

	}

	
}


[System.Serializable]
public class RoutePoint
{


	public string lat;
	public string lon;
	public string description;
	public Marker marker;
	public GameObject waypoint;
	
}

