using UnityEngine;
using UnityEditor;
using System.IO;
using GQ.Client.Conf;
using System;
using System.Globalization;

namespace GQ.Editor.Building {
	[InitializeOnLoad]
	public class BuildStartHook {
		static BuildStartHook () {
			writeBuildDate();
		}

		/// <summary>
		/// Writes the current build date into a tiny file in the ConfigAssets. 
		/// It will be read by the application on start and used as additional version number.
		/// </summary>
		static void writeBuildDate () {
			try {
				CultureInfo culture = new CultureInfo("de-DE"); 
				if ( File.Exists(ConfigurationManager.BUILD_TIME_FILE_PATH) ) {
					File.Delete(ConfigurationManager.BUILD_TIME_FILE_PATH);
				}
				File.WriteAllText(ConfigurationManager.BUILD_TIME_FILE_PATH, DateTime.Now.ToString("G", culture));
			} catch ( Exception exc ) {
				Debug.LogWarning("Could not write build time file at " + ConfigurationManager.BUILD_TIME_FILE_PATH + "\n" + exc.Message);
				return;
			} 
			// TODO sometimes too early, so it will not work.
//			AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport); 
		}
	}

}
