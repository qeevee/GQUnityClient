﻿using UnityEngine;
using System.Collections;

public class videotest : MonoBehaviour
{

	// Use this for initialization
	void Start ()
	{
#if !UNITY_WEBPLAYER

		//		string dpath = "file://" + Application.dataPath + "testvideo.mp4";
		
		//		string dpath = Application.dataPath + "testvideo.mp4";
		
		string dpath = "testvideo.mp4";
		StartCoroutine(PlayStreamingVideo(dpath));

		Handheld.PlayFullScreenMovie (dpath);

#endif                      
	

	}
	
	private IEnumerator PlayStreamingVideo(string url)
	{
		Handheld.PlayFullScreenMovie(url, Color.black, FullScreenMovieControlMode.Full);
//		yield return new WaitForEndOfFrame();
//		yield return new WaitForEndOfFrame();
		yield return new WaitForSeconds(1.0f);
		Handheld.PlayFullScreenMovie(url, Color.black, FullScreenMovieControlMode.Full);
		//		Debug.Log("Video playback completed.");
	}


	// Update is called once per frame
	void Update ()
	{
	
	}
}
