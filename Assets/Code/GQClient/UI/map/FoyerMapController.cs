﻿using System;
using Code.GQClient.Conf;
using Code.GQClient.Err;
using Code.GQClient.UI.layout;
using GQClient.Model;
using UnityEngine;

namespace Code.GQClient.UI.map
{

	/// <summary>
	/// Shows all Quest Info objects, on a map within the foyer. Refreshing its content silently (no dialogs shown etc.).
	/// </summary>
	public class FoyerMapController : MapController
	{
		#region Initialize

		private QuestInfoManager _qim;

		protected void Start ()
		{
			// at last we register for changes on quest infos with the quest info manager:
			_qim = QuestInfoManager.Instance;
			_qim.DataChange.AddListener(OnMarkerChanged);
			_qim.FilterChange.AddListener(OnFilterChanged);

			//_qim.OnFilterChange += OnMarkerChanged;
			RTConfig.RTConfigChanged.AddListener(UpdateView);
		}

		#endregion

		#region React on Events

		private void OnMarkerChanged (QuestInfoChangedEvent e)
		{
			Marker m;
			switch (e.ChangeType) {
			case ChangeType.AddedInfo:
				UpdateView ();
				break;
			case ChangeType.ChangedInfo:
				if (!Markers.TryGetValue (e.OldQuestInfo.Id, out m)) {
					Log.SignalErrorToDeveloper (
						"Quest Info Controller for quest id {0} not found when a Change event occurred.",
						e.OldQuestInfo.Id
					);
					break;
				}
				// m.UpdateMarker();
				m.Show ();
				break;
			case ChangeType.RemovedInfo:
				if (!Markers.TryGetValue (e.OldQuestInfo.Id, out m)) {
					Log.SignalErrorToDeveloper (
						"Quest Info Controller for quest id {0} not found when a Remove event occurred.",
						e.OldQuestInfo.Id
					);
					break;
				}
				m.Hide ();
				e.OldQuestInfo.OnChanged -= m.UpdateView;
				Markers.Remove (e.OldQuestInfo.Id);
				break;							
			case ChangeType.ListChanged:
				UpdateView ();
				break;							
			case ChangeType.FilterChanged:
				UpdateView ();
				break;
			case ChangeType.SorterChanged:
				break;
			default:
				throw new ArgumentOutOfRangeException();
			}
		}

		public void OnFilterChanged()
		{
			UpdateView();
		}

		#endregion

		#region Map & Markers

		protected override void populateMarkers ()
		{
			foreach (var info in QuestInfoManager.Instance.GetFilteredQuestInfos()) {
				// create new list elements
				CreateMarker (info);
			}
		}

		private void CreateMarker (QuestInfo info)
		{
			if (info.MarkerHotspot.Equals (HotspotInfo.NULL)) {
				return;
			}
			
			QuestMarker newMarker = new QuestMarker(info);

			Markers.Add (info.Id, newMarker);
			OnlineMapsMarker ommarker = markerManager.Create(info.MarkerHotspot.Longitude, info.MarkerHotspot.Latitude, newMarker.Texture);
			ommarker.OnClick += newMarker.OnTouchOMM;
			ommarker.scale = LayoutConfig.Units2Pixels(Config.Current.markerHeightUnits) /
			                 newMarker.Texture.height;

			// TODO: info.OnChanged += newMarker.UpdateView;
		}
		
		public Texture markerSymbolTexture;

		protected override void SetLocationToMiddleOfHotspots()
		{
			double sumLong = 0f;
			double sumLat = 0f;
			var counter = 0;
			foreach (var qi in QuestInfoManager.Instance.GetListOfQuestInfos())
			{
				var hi = qi.MarkerHotspot;
				if (hi == HotspotInfo.NULL)
					continue;

				sumLong += hi.Longitude;
				sumLat += hi.Latitude;
				counter++;
			}

			if (counter == 0)
			{
				map.SetPosition(Config.Current.mapStartAtLongitude,
					Config.Current.mapStartAtLatitude);
			}
			else
			{
				map.SetPosition(sumLong / counter,
					sumLat / counter);
			}
		}

		#endregion
	}
}