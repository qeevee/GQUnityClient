﻿// #define DEBUG_LOG

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code.GQClient.Conf;
using Code.GQClient.Err;
using Code.GQClient.UI.layout;
using Code.GQClient.Util;
using Code.QM.Util;
using GQClient.Model;
using UnityEngine;

namespace Code.GQClient.UI.map
{

    public abstract class MapController : MonoBehaviour
	{
		public OnlineMapsMarkerManager markerManager;
		public OnlineMaps map;
		
		private static Dictionary<int, Marker> markers;
		
		/// <summary>
		/// Marker dictionary is static to support the singleton MapBehaviour from slippy maps well. 
		/// When maps change all markers must be removed from the MapBehaviour as well as from this dictionary.
		/// </summary>
		/// <value>The markers.</value>
		protected static Dictionary<int, Marker> Markers {
			get {
				if (markers == null) {
					markers = new Dictionary<int, Marker> ();
				}
				return markers;
			}
		}

		private static float MARKER_SCALE_FACTOR {
			get {
				// We empirically found this to be close to a correct scaling factor in order to resize the markers according to 
				// UI elements like buttons etc.:
				return (Device.height / 800000f);
			}
		}


		// static protected void calculateMarkerDetails (Texture texture, GameObject markerGO)
		// {
		// 	// Get the category name for the given info regarding the current filter selection ...
		// 	Renderer markerRenderer = markerGO.GetComponent<Renderer> ();
		// 	markerRenderer.material.renderQueue = 4001;
		// 	markerRenderer.material.mainTexture = texture;
		// 	// scale the marker so that it fits inside the surrouding tile holder which is a square:
		// 	float markerWidth = LayoutConfig.Units2Pixels (Math.Min (1.0f, (float)texture.width / (float)texture.height));
		// 	float markerHeight = LayoutConfig.Units2Pixels (Math.Min (1.0f, (float)texture.height / (float)texture.width));
		// 	markerGO.transform.localScale = 
		// 		new Vector3 (markerWidth, 1.0f, markerHeight) * (FoyerMapScreenLayout.MarkerHeightUnits * MARKER_SCALE_FACTOR) / 
		// 		ConfigurationManager.Current.mapScale;
		// 	markerGO.AddComponent<CameraFacingBillboard> ().Axis = Vector3.up;
		// 	markerGO.layer = QuestMarkerInteractions.MARKER_LAYER;
		// 	BoxCollider markerBox = markerGO.GetComponent<BoxCollider> ();
		// 	markerBox.center = new Vector3 (0.0f, 0.0f, 0.5f);
		// }

		private static bool _ignoreInteraction;

		public static bool IgnoreInteraction {
			get {
				return _ignoreInteraction;
			}
			set {
				if (value == true) {
					_ignoreInteraction = true;
				} else {
					Base.Instance.StartCoroutine (_setIgnoreInteractionToFalseAsCoroutine ());
				}
			}

		}

		private static IEnumerator _setIgnoreInteractionToFalseAsCoroutine ()
		{
			yield return new WaitForEndOfFrame ();
			yield return new WaitForEndOfFrame ();
			_ignoreInteraction = false;
			yield break;
		}


		public GameObject MapButtonPanel;
		public Texture CenterTexture;
		public Texture FrameTexture;
		public Texture	LocationTexture;

		#region Center

		public enum Centering
		{
			Centered,
			Framed,
			Manual
		}

		public Centering CenteringState {
			get;
			protected set;
		}

		public void Frame ()
		{
			// center the map so it frames all currently visible markers: TODO

			// let the center button show the centering button icon now, unless local position is not available, in that case show the frame icon and disbale it.

			CenteringState = Centering.Framed;
		}

		public void Center ()
		{
			// center the map so it is centered to the current users position:
			OnlineMapsLocationService locService = map.GetComponent<OnlineMapsLocationService>();
			map.SetPosition(locService.position.x, locService.position.y);

			// let the center button show the centering button icon now
			CenteringState = Centering.Centered;
		}

		public void CenterButtonPressed ()
		{
			Center ();
		}

		#endregion


		#region Zoom

		OverlayButtonLayoutConfig zoomInButton;
		OverlayButtonLayoutConfig zoomOutButton;
		
		public float zoomDeltaFactor = 1.03f;
		
		public void ZoomIn()
		{
			map.floatZoom *= zoomDeltaFactor;
			map.Redraw();
		}

		public void ZoomOut()
		{
			map.floatZoom /= zoomDeltaFactor;
			map.Redraw();
		}

		#endregion

		private void OnEnable()
		{
			Debug.Log("Map enabled".Yellow());
			map = Base.Instance.Map;
			markerManager = map.GetComponent<OnlineMapsMarkerManager>();
			UpdateView();
		}

		protected void locateAtStart()
		{
			Debug.Log($"locateAtStart: mapStartPositionType = {ConfigurationManager.Current.mapStartPositionType}  pos is before: ({map.position.x}, {map.position.y})");
			switch (ConfigurationManager.Current.mapStartPositionType) {
				case MapStartPositionType.CenterOfMarkers:
					// calculate center of markers / quests:
					SetLocationToMiddleOfHotspots();
					break;
				case MapStartPositionType.FixedPosition:
					Debug.Log($"locateAtStart: map.SetPosition({ConfigurationManager.Current.mapStartAtLongitude}, {ConfigurationManager.Current.mapStartAtLatitude}) #Fixed Config Pos");
					map.SetPosition(ConfigurationManager.Current.mapStartAtLongitude,
						ConfigurationManager.Current.mapStartAtLatitude);
					break;
				case MapStartPositionType.PlayerPosition:
					if (Device.location.isEnabledByUser &&
					    Device.location.status != LocationServiceStatus.Running) {
						OnlineMapsLocationService locServ = map.GetComponent<OnlineMapsLocationService>();
						Debug.Log($"locateAtStart: map.SetPosition({locServ.position.x}, {locServ.position.y}) #PlayerPos");
						map.SetPosition(locServ.position.x, locServ.position.y);
					} else
					{
						Debug.Log($"locateAtStart: map.SetPosition({ConfigurationManager.Current.mapStartAtLongitude}, {ConfigurationManager.Current.mapStartAtLatitude}) #Config instead of PLayerPos");
						map.SetPosition(ConfigurationManager.Current.mapStartAtLongitude,
							ConfigurationManager.Current.mapStartAtLatitude);
					}
					break;
			}

		}

		protected abstract void SetLocationToMiddleOfHotspots();

		protected abstract void populateMarkers ();

		private static bool _alreadyLocatedAtStart = false;
		
		public void UpdateView ()
		{
			if (this == null) {
				return;
			}

			// hide and delete all list elements:
			foreach (var kvp in Markers) {
				// TODO CLARIFY WHY THIS CONTINUE IS NECESSARY:
				if (kvp.Value == null)
					continue;
				
				kvp.Value.Hide ();
				// remove marker update as listener to questInfo Changed Events:
				QuestInfoManager.Instance.GetQuestInfo(kvp.Key).OnChanged -= kvp.Value.UpdateView;
			}

			foreach (var marker in markerManager.items.ToList())
			{
				if (marker == OnlineMapsLocationServiceBase.marker)
				{
					// Debug.Log($"KEEP LOCATION MARKER: {marker.position}");
					continue;
				}
				markerManager.Remove(marker);
			}
			Markers.Clear ();

			populateMarkers ();

			if (!_alreadyLocatedAtStart)
			{
				_alreadyLocatedAtStart = true;

				locateAtStart();
			}
		}
	}
}
