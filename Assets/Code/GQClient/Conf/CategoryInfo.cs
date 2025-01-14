using System;
using UnityEngine;

namespace Code.GQClient.Conf {

	/// <summary>
	/// This class is used for derserializing the JSON product configuration into at runtime. 
	/// This is done right in the beginning in Awake() of the ConfigurationManager.
	/// </summary>
	[System.Serializable, Obsolete]
	public class CategoryInfo {

		private string _id;

		public string ID { 
			get {
				return _id;
			}
			set {
				_id = value;
				sprite = Resources.Load<Sprite>("markers/" + _id);
			} 
		}


		public string Name { get; set; }

		[NonSerialized]

		public Sprite sprite;
		public bool showInList = true;
		/// <summary>
		/// Flag that determines whether this category's markers will be shown on the map. 
		/// This is currently also the only state used for category markers and also reflects the state of the buttons etc.
		/// </summary>
		public bool showOnMap = true;

	}

}