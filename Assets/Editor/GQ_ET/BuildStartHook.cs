using UnityEngine;
using UnityEditor;
using System.IO;
using GQ.Conf;
using System;
using System.Globalization;

namespace GQ.ET {
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
				if ( File.Exists(ProductConfigManager.BUILD_TIME_FILE_PATH) ) {
					File.Delete(ProductConfigManager.BUILD_TIME_FILE_PATH);
				}
				File.WriteAllText(ProductConfigManager.BUILD_TIME_FILE_PATH, DateTime.Now.ToString("G", culture));
			} catch ( Exception exc ) {
				Debug.LogWarning("Could not write build time file at " + ProductConfigManager.BUILD_TIME_FILE_PATH);
				return;
			} 
			AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate); 
		}
	}

}