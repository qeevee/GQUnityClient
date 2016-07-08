namespace GQ.Client.Conf {
	/// <summary>
	/// Config class specifies thetextual parameters of a product. It is used both at runtime to initilize the app's branding details from and 
	/// at editor time to back the product editor view and store the parameters while we use the editor.
	/// </summary>
	public class Config {
		//////////////////////////////////
		// THE ACTUAL PRODUCT CONFIG DATA:	
		
		public string   id     { get; set; }

		public string   name   { get; set; }

		public int   	portal   { get; set; }

		public int   	autoStartQuestID   { get; set; }

		public bool 	autostartIsPredeployed  { get; set; }

		public int   	downloadTimeOutSeconds   { get; set; }

		public string 	nameForQuest { get; set; }

		public string 	questVisualization { get; set; }

		public bool 	questVisualizationChangeable  { get; set; }

		public bool 	showCloudQuestsImmediately  { get; set; }

		public bool 	showTextInLoadingLogo  { get; set; }

		public bool 	showNetConnectionWarning  { get; set; }

		public string 	colorProfile { get; set; }

		public string 	mapboxKey { get; set; }

		public string 	mapboxMapID { get; set; }

		public string 	imprint  { get; set; }

	}

}

