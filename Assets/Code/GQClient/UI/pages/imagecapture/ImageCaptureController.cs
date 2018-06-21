﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GQ.Client.Model;
using System;
using System.Globalization;
using GQ.Client.Util;
using System.IO;
using GQ.Client.FileIO;

namespace GQ.Client.UI
{

	public class ImageCaptureController : PageController {

		#region Inspector Features

		public Text text;
		public Image textbg;

		public Button button;

		bool camIsRotated;
		WebCamTexture cameraTexture;
		public RawImage camRawImage;

		#endregion



		#region Runtime API

		protected PageImageCapture myPage;

		/// <summary>
		/// Is called during Start() of the base class, which is a MonoBehaviour.
		/// </summary>
		public override void Initialize ()
		{
			myPage = (PageImageCapture)page;

			// show the task and button:
			if (myPage.Task != null && myPage.Task != "") {
				text.text = myPage.Task;
			} else {
				text.gameObject.SetActive (false);
			}

			// init web cam;
			Base.Instance.StartCoroutine (initWebCam ());
		}

		IEnumerator initWebCam() {
			Debug.Log ("########### CAMERA: 1");
			string deviceName = null;
			foreach (WebCamDevice wcd in WebCamTexture.devices) {
				if (!wcd.isFrontFacing) {
					deviceName = wcd.name;
					break;
				}
			}

			cameraTexture = new WebCamTexture (deviceName);
			Debug.Log ("########### CAMERA: 2");

			cameraTexture.requestedHeight = 2000;
			cameraTexture.requestedWidth = 3000;

			cameraTexture.Play ();

			// wait for web cam to be ready which is guaranteed after first image update:
			while (!cameraTexture.didUpdateThisFrame)
				yield return null;

			Debug.Log ("########### CAMERA: 3");

			// rotate if needed:
			camRawImage.transform.rotation *= Quaternion.AngleAxis (cameraTexture.videoRotationAngle, Vector3.back);

			camIsRotated = Math.Abs (cameraTexture.videoRotationAngle) == 90 || Math.Abs (cameraTexture.videoRotationAngle) == 270;
			Debug.Log ("########### CAMERA: 4");
			float camHeight = (camIsRotated ? cameraTexture.width : cameraTexture.height);
			float camWidth = (camIsRotated ? cameraTexture.height : cameraTexture.width);

			float panelHeight = camRawImage.rectTransform.rect.height;
			float panelWidth = camRawImage.rectTransform.rect.width;

			float heightScale = panelHeight / camHeight;
			float widthScale = panelWidth / camWidth;
			float fitScale = Math.Min (heightScale, widthScale);

			float goalHeight = cameraTexture.height * fitScale;
			float goalWidth = cameraTexture.width * fitScale;

			heightScale = goalHeight / panelHeight;
			widthScale = goalWidth / panelWidth;

			float mirrorAdjustment = cameraTexture.videoVerticallyMirrored ? -1F : 1F;
			// TODO adjust mirror also correct if cam is not rotated:
			Debug.Log ("########### CAMERA: 5");
			camRawImage.transform.localScale = new Vector3 (widthScale, heightScale * mirrorAdjustment, 1F);

			camRawImage.texture = cameraTexture;
			Debug.Log ("########### CAMERA: 6");
		}

//		public void Update () {
//			Debug.Log ("########### CAMERA: 7");
//			Debug.Log ("Camera name: " + cameraTexture.deviceName);
//			Debug.Log ("Camera isPLaying: " + cameraTexture.isPlaying);
//			Debug.Log ("Camera height: " + cameraTexture.requestedHeight + " width: " + cameraTexture.requestedWidth);
//		}
//
		public void TakeSnapshot ()
		{

			Texture2D photo;

			// we add 360 degrees to avoid any negative values:
			int rotatedClockwiseQuarters = 360 - cameraTexture.videoRotationAngle;

			switch (Input.deviceOrientation) {
			case DeviceOrientation.LandscapeLeft:
				rotatedClockwiseQuarters += 90;
				break;
			case DeviceOrientation.PortraitUpsideDown:
				rotatedClockwiseQuarters += 180;
				break;
			case DeviceOrientation.LandscapeRight:
				rotatedClockwiseQuarters += 270;
				break;
			case DeviceOrientation.Portrait:
			case DeviceOrientation.FaceUp:
			case DeviceOrientation.FaceDown:
			default:
				break;
			}

			rotatedClockwiseQuarters /= 90;  // going from degrees to quarters
			rotatedClockwiseQuarters %= 4; // reducing to 0, 1 ,2 or 3 quarters

			Color[] pixels = cameraTexture.GetPixels ();

			switch (rotatedClockwiseQuarters) {
			case 1:
				photo = new Texture2D (cameraTexture.height, cameraTexture.width);
				photo.SetPixels (pixels.Rotate90 (cameraTexture.height, cameraTexture.width));
				break;
			case 2:
				photo = new Texture2D (cameraTexture.width, cameraTexture.height);
				photo.SetPixels (pixels.Rotate180 (cameraTexture.width, cameraTexture.height));
				break;
			case 3:
				photo = new Texture2D (cameraTexture.height, cameraTexture.width);
				photo.SetPixels (pixels.Rotate270 (cameraTexture.height, cameraTexture.width));
				break;
			case 0:
			default:
				photo = new Texture2D (cameraTexture.width, cameraTexture.height);
				photo.SetPixels (pixels);
				break;
			}
			photo.Apply ();

			cameraTexture.Stop ();

//			QuestRuntimeAsset qra = new QuestRuntimeAsset ("@_" + myPage.File, photo);
//			actioncontroller.addPhoto (qra);

			SaveTextureToCamera (photo);

			OnForward ();
		}

		void SaveTextureToCamera (Texture2D texture)
		{
			DateTime now = DateTime.Now;
			string filename = now.ToString ("yyyy_MM_dd_HH_mm_ss_fff", CultureInfo.InvariantCulture) + ".jpg";
			string filepath = Files.CombinePath (QuestManager.GetRuntimeMediaPath(myPage.Quest.Id), filename);

			byte[] bytes = texture.EncodeToJPG();

			File.WriteAllBytes(filepath, bytes);
			Variables.SetVariableValue (myPage.File, new Value(filename));

			// save media info for local file under the pseudo variable (e.g. @_imagecapture):
			myPage.Quest.MediaStore [GQML.PREFIX_RUNTIME_MEDIA + myPage.File] =
				new MediaInfo (
				myPage.Quest.Id, 
				GQML.PREFIX_RUNTIME_MEDIA + myPage.File, 
				QuestManager.GetRuntimeMediaPath (myPage.Quest.Id),
				filename
			);

			// TODO save to mediainfos.json again

			if (File.Exists (filepath))
				Debug.Log ("CAMERA: Shot saved to file: " + filepath);
			else
				Debug.Log ("CAMERA: ERROR tring to save shot to file: " + filepath);


			if (Application.platform == RuntimePlatform.Android) {
//				GetComponent<AndroidCamera> ().SaveImageToGallery (texture, filename);
			}
			if (Application.platform == RuntimePlatform.IPhonePlayer) {
//				GetComponent<IOSCamera> ().SaveTextureToCameraRoll (texture);
			}
		}
		#endregion
	}
}